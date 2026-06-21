"""Chunk text into different shapes and sizes using multiple strategies.

This module implements the ``chunk_text`` tool for the codex agent framework.
Its single responsibility is to take a piece of text (supplied inline or read
from a file) and split it into chunks using one of several well defined
strategies:

``fixed``
    Consecutive, non-overlapping chunks of a fixed ``size``.
``sliding``
    A sliding window of ``size`` units that advances by ``size - overlap``.
``variable``
    Consecutive chunks whose sizes are taken, in order, from ``sizes``.
``paragraph``
    Split on blank lines, optionally grouping ``size`` paragraphs per chunk.
``sentence``
    Split on sentence boundaries, optionally grouping ``size`` sentences.
``regex``
    Split on a caller supplied regular expression separator.

The size based strategies (``fixed``/``sliding``/``variable``) measure ``size``
and ``overlap`` in a configurable ``unit``: ``chars``, ``lines`` or ``words``.
The structural strategies (``paragraph``/``sentence``/``regex``) measure ``size``
in items (paragraphs, sentences or segments) and ignore ``unit``.

Every chunk carries the character offsets ``[start, end)`` of the slice of the
original content it came from, which makes results easy to verify and to map
back onto the source document.
"""

from __future__ import annotations

import re
from pathlib import Path
from typing import TYPE_CHECKING, Callable, ClassVar, NamedTuple

from pydantic import BaseModel, Field

from codex.core.tools.base import (
    BaseTool,
    BaseToolConfig,
    BaseToolState,
    ToolError,
    ToolPermission,
)
from codex.core.tools.ui import ToolCallDisplay, ToolResultDisplay, ToolUIData

if TYPE_CHECKING:
    from codex.core.types import ToolCallEvent, ToolResultEvent


# --------------------------------------------------------------------------- #
# Strategy vocabulary and shared regular expressions
# --------------------------------------------------------------------------- #

SIZE_MODES = ("fixed", "sliding", "variable")
STRUCTURAL_MODES = ("paragraph", "sentence", "regex")
ALL_MODES = SIZE_MODES + STRUCTURAL_MODES
SIZE_UNITS = ("chars", "lines", "words")

# Human readable unit reported for each structural mode.
STRUCTURAL_UNITS = {
    "paragraph": "paragraphs",
    "sentence": "sentences",
    "regex": "segments",
}

# Joiner used when several structural items are grouped into a single chunk.
STRUCTURAL_JOINERS = {
    "paragraph": "\n\n",
    "sentence": " ",
    "regex": "",
}

# Blank-line separated paragraphs.
PARAGRAPH_SPLIT = re.compile(r"\n\s*\n+")
# Naive sentence boundary: terminal punctuation followed by whitespace.
SENTENCE_SPLIT = re.compile(r"(?<=[.!?])\s+")
# A single line including its trailing newline (split only on "\n").
LINE_RE = re.compile(r"[^\n]*\n|[^\n]+\Z")
# A run of non-whitespace characters.
WORD_RE = re.compile(r"\S+")


class Chunk(NamedTuple):
    """A single chunk together with where it came from in the source text.

    ``size`` is expressed in the chunk's effective unit: characters, lines or
    words for the size based modes, and a count of items for the structural
    modes.
    """

    text: str
    start: int
    end: int
    size: int


# --------------------------------------------------------------------------- #
# Pure chunking helpers (no dependency on the tool framework or I/O)
# --------------------------------------------------------------------------- #


def _spans(content: str, unit: str) -> list[tuple[int, int]]:
    """Return ``[start, end)`` character spans for each unit item in *content*."""
    pattern = LINE_RE if unit == "lines" else WORD_RE
    return [(m.start(), m.end()) for m in pattern.finditer(content)]


def _size_based_chunks(
    content: str,
    mode: str,
    unit: str,
    size: int,
    overlap: int,
    sizes: list[int],
    repeat_sizes: bool,
) -> list[Chunk]:
    """Chunk *content* by characters, lines or words.

    ``chars`` is handled directly with index arithmetic; ``lines`` and ``words``
    are handled as a sequence of unit spans. In every case a chunk's text is the
    exact slice ``content[start:end]`` so the offsets are always faithful.
    """
    if unit == "chars":
        count = len(content)

        def slice_range(i: int, j: int) -> Chunk:
            return Chunk(content[i:j], i, j, j - i)
    else:
        spans = _spans(content, unit)
        count = len(spans)

        def slice_range(i: int, j: int) -> Chunk:
            start, end = spans[i][0], spans[j - 1][1]
            return Chunk(content[start:end], start, end, j - i)

    if count == 0:
        return []

    chunks: list[Chunk] = []
    if mode == "fixed":
        index = 0
        while index < count:
            stop = min(index + size, count)
            chunks.append(slice_range(index, stop))
            index = stop
    elif mode == "sliding":
        step = size - overlap
        index = 0
        while index < count:
            stop = min(index + size, count)
            chunks.append(slice_range(index, stop))
            index += step
    else:  # variable
        index = 0
        size_index = 0
        while index < count:
            if size_index >= len(sizes):
                if repeat_sizes:
                    size_index = 0
                else:
                    # Sizes exhausted: emit the remainder as a final chunk.
                    chunks.append(slice_range(index, count))
                    break
            stop = min(index + sizes[size_index], count)
            chunks.append(slice_range(index, stop))
            index = stop
            size_index += 1
    return chunks


def _trim(content: str, start: int, end: int, trim_leading: bool) -> tuple[int, int]:
    """Shrink ``[start, end)`` past surrounding whitespace.

    Trailing whitespace is always trimmed; leading whitespace is only trimmed
    when *trim_leading* is set (it is kept when a separator is deliberately
    attached to the start of a chunk).
    """
    if trim_leading:
        while start < end and content[start].isspace():
            start += 1
    while end > start and content[end - 1].isspace():
        end -= 1
    return start, end


def _split_items(
    content: str, regex: re.Pattern[str], include_separator: bool
) -> list[Chunk]:
    """Split *content* on *regex* into trimmed, offset-bearing items.

    When *include_separator* is true each separator is kept at the start of the
    item that follows it (and leading whitespace is preserved so the separator
    survives); otherwise separators are discarded and both ends are trimmed.
    """
    matches = list(regex.finditer(content))
    for match in matches:
        if match.start() == match.end():
            raise ToolError("Separator pattern must not match an empty string.")

    raw_bounds: list[tuple[int, int]] = []
    if include_separator:
        # Each chunk starts at a separator; the text before the first separator
        # (if any) forms a leading chunk of its own.
        cut_points = sorted({m.start() for m in matches if m.start() > 0})
        boundaries = [0, *cut_points, len(content)]
        raw_bounds = [
            (boundaries[i], boundaries[i + 1])
            for i in range(len(boundaries) - 1)
            if boundaries[i] < boundaries[i + 1]
        ]
        trim_leading = False
    else:
        pos = 0
        for match in matches:
            if pos < match.start():
                raw_bounds.append((pos, match.start()))
            pos = match.end()
        if pos < len(content):
            raw_bounds.append((pos, len(content)))
        trim_leading = True

    items: list[Chunk] = []
    for start, end in raw_bounds:
        start, end = _trim(content, start, end, trim_leading)
        if start < end:
            items.append(Chunk(content[start:end], start, end, 1))
    return items


def _group_items(items: list[Chunk], size: int | None, joiner: str) -> list[Chunk]:
    """Group structural *items* into chunks of at most *size* items each."""
    if not items:
        return []
    if size is None:
        return items
    if size <= 0:
        raise ToolError("size must be a positive integer.")

    grouped: list[Chunk] = []
    for start in range(0, len(items), size):
        batch = items[start : start + size]
        text = joiner.join(item.text for item in batch)
        grouped.append(Chunk(text, batch[0].start, batch[-1].end, len(batch)))
    return grouped


def compute_chunks(
    content: str,
    *,
    mode: str,
    unit: str,
    size: int | None,
    sizes: list[int] | None,
    overlap: int,
    separator: str | None,
    include_separator: bool,
    repeat_sizes: bool,
) -> tuple[list[Chunk], str]:
    """Dispatch to the requested strategy and return ``(chunks, effective_unit)``.

    This is the pure core of the tool: it validates the arguments for the chosen
    mode and raises :class:`ToolError` on any inconsistency.
    """
    mode = mode.strip().lower()
    unit = unit.strip().lower()

    if mode in SIZE_MODES:
        if unit not in SIZE_UNITS:
            raise ToolError(f"unit must be one of: {', '.join(SIZE_UNITS)}.")

        if mode == "fixed":
            size = _require_positive(size, "size")
            chunks = _size_based_chunks(content, mode, unit, size, 0, [], True)
        elif mode == "sliding":
            size = _require_positive(size, "size")
            overlap = _require_non_negative(overlap, "overlap")
            if overlap >= size:
                raise ToolError("overlap must be smaller than size.")
            chunks = _size_based_chunks(content, mode, unit, size, overlap, [], True)
        else:  # variable
            sizes = _require_sizes(sizes)
            chunks = _size_based_chunks(content, mode, unit, 0, 0, sizes, repeat_sizes)
        return chunks, unit

    if mode in STRUCTURAL_MODES:
        if size is not None and size <= 0:
            raise ToolError("size must be a positive integer.")

        if mode == "regex":
            pattern = (separator or "").strip()
            if not pattern:
                raise ToolError("separator is required for regex mode.")
            try:
                regex = re.compile(pattern, re.MULTILINE)
            except re.error as exc:
                raise ToolError(f"Invalid regex separator: {exc}") from exc
        else:
            regex = PARAGRAPH_SPLIT if mode == "paragraph" else SENTENCE_SPLIT
            include_separator = False

        items = _split_items(content, regex, include_separator)
        chunks = _group_items(items, size, STRUCTURAL_JOINERS[mode])
        return chunks, STRUCTURAL_UNITS[mode]

    raise ToolError(f"mode must be one of: {', '.join(ALL_MODES)}.")


def _require_positive(value: int | None, name: str) -> int:
    if value is None:
        raise ToolError(f"{name} is required for this mode.")
    if value <= 0:
        raise ToolError(f"{name} must be a positive integer.")
    return value


def _require_non_negative(value: int, name: str) -> int:
    if value < 0:
        raise ToolError(f"{name} must be a non-negative integer.")
    return value


def _require_sizes(sizes: list[int] | None) -> list[int]:
    if not sizes:
        raise ToolError("sizes must be provided for variable mode.")
    if any(size <= 0 for size in sizes):
        raise ToolError("All sizes must be positive integers.")
    return list(sizes)


# --------------------------------------------------------------------------- #
# Tool definition
# --------------------------------------------------------------------------- #


class ChunkTextConfig(BaseToolConfig):
    permission: ToolPermission = ToolPermission.ALWAYS
    max_input_bytes: int = Field(
        default=1_000_000,
        description="Maximum bytes allowed for input content.",
    )
    max_chunks: int = Field(
        default=200,
        description="Maximum number of chunks to return.",
    )


class ChunkTextState(BaseToolState):
    pass


class ChunkTextArgs(BaseModel):
    content: str | None = Field(default=None, description="Raw text to chunk.")
    path: str | None = Field(default=None, description="Path to a text file to chunk.")
    mode: str = Field(
        default="fixed",
        description="Chunking strategy: fixed, sliding, variable, paragraph, sentence, regex.",
    )
    unit: str = Field(
        default="chars",
        description=(
            "Unit for size and overlap in fixed/sliding/variable modes: "
            "chars, lines or words. Ignored by the structural modes."
        ),
    )
    size: int | None = Field(
        default=None,
        description=(
            "Chunk size for fixed/sliding modes (in units), or the number of "
            "items to group per chunk for paragraph/sentence/regex modes."
        ),
    )
    sizes: list[int] | None = Field(
        default=None,
        description="Ordered list of chunk sizes for variable mode.",
    )
    overlap: int = Field(
        default=0,
        description="Overlap between consecutive chunks (in units) for sliding mode.",
    )
    separator: str | None = Field(
        default=None,
        description="Regular expression to split on for regex mode.",
    )
    include_separator: bool = Field(
        default=False,
        description="Keep separators at the start of each chunk in regex mode.",
    )
    repeat_sizes: bool = Field(
        default=True,
        description=(
            "In variable mode, cycle through sizes when the input is longer than "
            "their sum. When false, the remainder becomes a final chunk."
        ),
    )
    max_chunks: int | None = Field(
        default=None,
        description="Override the configured maximum number of chunks.",
    )


class ChunkTextResult(BaseModel):
    chunks: list[str]
    count: int
    truncated: bool
    mode: str
    unit: str
    chunk_sizes: list[int]
    offsets: list[tuple[int, int]]
    input_bytes: int
    input_chars: int


class ChunkText(
    BaseTool[ChunkTextArgs, ChunkTextResult, ChunkTextConfig, ChunkTextState],
    ToolUIData[ChunkTextArgs, ChunkTextResult],
):
    description: ClassVar[str] = (
        "Chunk text into different shapes and sizes using multiple strategies "
        "(fixed, sliding, variable, paragraph, sentence, regex)."
    )

    async def run(self, args: ChunkTextArgs) -> ChunkTextResult:
        content = self._load_content(args)
        mode = args.mode.strip().lower()
        unit = args.unit.strip().lower()

        if not content:
            return ChunkTextResult(
                chunks=[],
                count=0,
                truncated=False,
                mode=mode,
                unit=STRUCTURAL_UNITS.get(mode, unit),
                chunk_sizes=[],
                offsets=[],
                input_bytes=0,
                input_chars=0,
            )

        chunks, effective_unit = compute_chunks(
            content,
            mode=args.mode,
            unit=args.unit,
            size=args.size,
            sizes=args.sizes,
            overlap=args.overlap,
            separator=args.separator,
            include_separator=args.include_separator,
            repeat_sizes=args.repeat_sizes,
        )

        max_chunks = (
            args.max_chunks if args.max_chunks is not None else self.config.max_chunks
        )
        if max_chunks <= 0:
            raise ToolError("max_chunks must be a positive integer.")

        truncated = len(chunks) > max_chunks
        if truncated:
            chunks = chunks[:max_chunks]

        return ChunkTextResult(
            chunks=[chunk.text for chunk in chunks],
            count=len(chunks),
            truncated=truncated,
            mode=mode,
            unit=effective_unit,
            chunk_sizes=[chunk.size for chunk in chunks],
            offsets=[(chunk.start, chunk.end) for chunk in chunks],
            input_bytes=len(content.encode("utf-8")),
            input_chars=len(content),
        )

    # ----------------------------------------------------------------- #
    # Input loading
    # ----------------------------------------------------------------- #

    def _load_content(self, args: ChunkTextArgs) -> str:
        if args.content is not None and args.path is not None:
            raise ToolError("Provide either content or path, not both.")
        if args.content is None and args.path is None:
            raise ToolError("Provide content or path.")

        if args.content is not None:
            self._validate_input_size(len(args.content.encode("utf-8")))
            return args.content

        path = self._resolve_path(args.path or "")
        try:
            self._validate_input_size(path.stat().st_size)
            return path.read_text("utf-8", errors="ignore")
        except OSError as exc:
            raise ToolError(f"Could not read file: {exc}") from exc

    def _validate_input_size(self, size: int) -> None:
        if size > self.config.max_input_bytes:
            raise ToolError(
                f"Input is {size} bytes, which exceeds the limit of "
                f"{self.config.max_input_bytes} bytes."
            )

    def _resolve_path(self, raw_path: str) -> Path:
        if not raw_path.strip():
            raise ToolError("Path cannot be empty.")

        path = Path(raw_path).expanduser()
        if not path.is_absolute():
            path = self.config.effective_workdir / path

        try:
            resolved = path.resolve()
        except (ValueError, OSError) as exc:
            raise ToolError("Could not resolve the provided path.") from exc

        if not resolved.exists():
            raise ToolError(f"File not found at: {resolved}")
        if resolved.is_dir():
            raise ToolError(f"Path is a directory, not a file: {resolved}")
        return resolved

    # ----------------------------------------------------------------- #
    # UI display
    # ----------------------------------------------------------------- #

    @classmethod
    def get_call_display(cls, event: ToolCallEvent) -> ToolCallDisplay:
        if not isinstance(event.args, ChunkTextArgs):
            return ToolCallDisplay(summary="chunk_text")

        return ToolCallDisplay(
            summary=f"chunk_text: {event.args.mode}",
            details={
                "mode": event.args.mode,
                "unit": event.args.unit,
                "size": event.args.size,
                "sizes": event.args.sizes,
                "overlap": event.args.overlap,
                "separator": event.args.separator,
                "include_separator": event.args.include_separator,
                "repeat_sizes": event.args.repeat_sizes,
                "max_chunks": event.args.max_chunks,
            },
        )

    @classmethod
    def get_result_display(cls, event: ToolResultEvent) -> ToolResultDisplay:
        if not isinstance(event.result, ChunkTextResult):
            return ToolResultDisplay(
                success=False,
                message=event.error or event.skip_reason or "No result",
            )

        result = event.result
        message = f"Created {result.count} chunks"
        warnings: list[str] = []
        if result.truncated:
            message += " (truncated)"
            warnings.append("Chunk list truncated by max_chunks limit")

        return ToolResultDisplay(
            success=True,
            message=message,
            warnings=warnings,
            details={
                "count": result.count,
                "truncated": result.truncated,
                "mode": result.mode,
                "unit": result.unit,
                "chunk_sizes": result.chunk_sizes,
                "offsets": result.offsets,
                "input_bytes": result.input_bytes,
                "input_chars": result.input_chars,
                "chunks": result.chunks,
            },
        )

    @classmethod
    def get_status_text(cls) -> str:
        return "Chunking text"
