"""Next-chunk-size prediction.

Forecasts the size of the *next* chunk in a stream of chunks, combining:

* **Holt double exponential smoothing** (level + trend) over the observed
  byte sizes, so drifting chunk sizes are tracked, and
* a **structural prior** -- running mean size per chunk ``kind`` (section /
  preamble / document) -- blended in when the next chunk's kind is known.

Prediction intervals come from the running standard deviation of one-step
forecast errors (Welford's algorithm).  Useful for pre-allocating buffers,
batching embedding calls, or tuning ``max_chunk_bytes``.  Stdlib only.
"""

from __future__ import annotations

from collections import defaultdict
from dataclasses import dataclass

from .text_structure_chunker import Chunk, ChunkingResult

__all__ = ["HoltSmoother", "ChunkSizeForecast", "ChunkSizePredictor"]


class HoltSmoother:
    """Double exponential smoothing: tracks a level and a linear trend."""

    def __init__(self, alpha: float = 0.4, beta: float = 0.2) -> None:
        self.alpha = alpha
        self.beta = beta
        self.level: float | None = None
        self.trend = 0.0

    def update(self, value: float) -> None:
        if self.level is None:
            self.level = value
            return
        previous_level = self.level
        self.level = self.alpha * value + (1 - self.alpha) * (self.level + self.trend)
        self.trend = self.beta * (self.level - previous_level) + (1 - self.beta) * self.trend

    def forecast(self, steps: int = 1) -> float:
        if self.level is None:
            return 0.0
        return self.level + steps * self.trend


@dataclass(frozen=True)
class ChunkSizeForecast:
    bytes: int
    lines: int
    low: int  # lower bound of the ~95% interval
    high: int  # upper bound of the ~95% interval
    confidence: float  # in (0, 1]; higher = steadier history
    based_on: int  # number of observed chunks


class ChunkSizePredictor:
    """Online predictor for the byte/line size of the next chunk."""

    def __init__(self, alpha: float = 0.4, beta: float = 0.2) -> None:
        self._bytes = HoltSmoother(alpha, beta)
        self._lines = HoltSmoother(alpha, beta)
        self._kind_totals: defaultdict[str, list[float]] = defaultdict(lambda: [0.0, 0.0])
        self.observations = 0
        # Welford accumulators over one-step forecast errors (bytes).
        self._error_mean = 0.0
        self._error_m2 = 0.0
        self._error_count = 0

    # -- observing ---------------------------------------------------------------

    def observe(self, nbytes: int, nlines: int = 0, kind: str | None = None) -> None:
        if self.observations > 0:
            error = nbytes - self._bytes.forecast()
            self._error_count += 1
            delta = error - self._error_mean
            self._error_mean += delta / self._error_count
            self._error_m2 += delta * (error - self._error_mean)
        self._bytes.update(float(nbytes))
        self._lines.update(float(nlines))
        if kind:
            totals = self._kind_totals[kind]
            totals[0] += nbytes
            totals[1] += 1
        self.observations += 1

    def observe_chunk(self, chunk: Chunk) -> None:
        self.observe(len(chunk.content.encode("utf-8")), chunk.line_span, chunk.kind)

    def fit_result(self, result: ChunkingResult) -> "ChunkSizePredictor":
        for chunk in result.chunks:
            self.observe_chunk(chunk)
        return self

    # -- predicting --------------------------------------------------------------

    def _error_std(self) -> float:
        if self._error_count < 2:
            return 0.0
        return (self._error_m2 / (self._error_count - 1)) ** 0.5

    def predict(self, kind: str | None = None, steps: int = 1) -> ChunkSizeForecast:
        """Forecast the size of the next chunk (``steps`` ahead).

        When ``kind`` has been seen before, the trend forecast is blended
        50/50 with that kind's mean size.
        """

        if self.observations == 0:
            return ChunkSizeForecast(0, 0, 0, 0, 0.0, 0)

        predicted = self._bytes.forecast(steps)
        if kind and self._kind_totals.get(kind, [0.0, 0.0])[1] > 0:
            total, count = self._kind_totals[kind]
            predicted = 0.5 * predicted + 0.5 * (total / count)
        predicted = max(predicted, 0.0)

        std = self._error_std()
        margin = 1.96 * std
        mean_size = self._bytes.level or 1.0
        confidence = 1.0 / (1.0 + (std / mean_size if mean_size else 0.0))

        return ChunkSizeForecast(
            bytes=round(predicted),
            lines=max(round(self._lines.forecast(steps)), 0),
            low=max(round(predicted - margin), 0),
            high=round(predicted + margin),
            confidence=round(confidence, 4),
            based_on=self.observations,
        )
