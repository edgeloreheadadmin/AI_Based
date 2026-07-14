"""``DocumentIntelligence`` -- one facade over the whole suite.

Feeds a document once through every layer:

structural chunking -> syntax tree -> semantic index -> n-gram language model
-> completion engine -> chunk-size predictor -> meta-heuristic chunk plan

and exposes the interesting queries (outline, next word, completions,
semantic search, size forecasts, optimized chunking) from one object.
"""

from __future__ import annotations

from .chunk_predictor import ChunkSizeForecast, ChunkSizePredictor
from .intellisense import CompletionEngine, Suggestion
from .language_model import NGramModel
from .metaheuristics import OptimizationResult, optimize_chunking
from .semantics import SearchHit, SemanticIndex
from .syntax_tree import DocumentNode, parse_document, walk
from .text_structure_chunker import ChunkingResult, chunk_text

__all__ = ["DocumentIntelligence"]


class DocumentIntelligence:
    """Analyse a document once; query it many ways."""

    def __init__(self, text: str, *, ngram_order: int = 3, seed: int = 13) -> None:
        self.text = text
        self.seed = seed

        self.structure: ChunkingResult = chunk_text(text)
        self.tree: DocumentNode = parse_document(text)
        self.semantics: SemanticIndex = (
            SemanticIndex.from_result(self.structure)
            if self.structure.chunks
            else SemanticIndex([])
        )
        self.language_model: NGramModel = NGramModel(order=ngram_order).fit(text)
        self.completions: CompletionEngine = CompletionEngine(text)
        self.size_predictor: ChunkSizePredictor = ChunkSizePredictor().fit_result(
            self.structure
        )

    # -- structure ---------------------------------------------------------------

    def outline(self) -> list[tuple[int, str]]:
        """The document outline as ``(relative_depth, title)`` pairs."""

        return [(h.relative_depth, h.title) for h in self.structure.headings]

    # -- prediction ----------------------------------------------------------------

    def next_token(self, context: str, k: int = 5) -> list[tuple[str, float]]:
        return self.language_model.predict_next(context, k)

    def next_word(self, context: str, k: int = 5) -> list[tuple[str, float]]:
        return self.language_model.predict_next_word(context, k)

    def complete(self, prefix: str, *, context: str = "", k: int = 8) -> list[Suggestion]:
        return self.completions.complete(prefix, context=context, k=k)

    def predict_next_chunk_size(self, kind: str | None = None) -> ChunkSizeForecast:
        return self.size_predictor.predict(kind)

    # -- semantics ---------------------------------------------------------------

    def search(self, query: str, k: int = 3) -> list[SearchHit]:
        return self.semantics.search(query, k)

    def keywords(self, chunk_index: int, k: int = 8) -> list[str]:
        return self.semantics.keywords(chunk_index, k)

    # -- optimization ---------------------------------------------------------------

    def optimized_plan(
        self, target_bytes: int = 800, *, method: str = "annealing"
    ) -> OptimizationResult:
        return optimize_chunking(
            self.text, target_bytes, method=method, seed=self.seed
        )

    # -- reporting ----------------------------------------------------------------

    def report(self) -> dict:
        """A JSON-serialisable summary of everything the suite extracted."""

        sentence_count = sum(1 for n in walk(self.tree) if n.kind == "sentence")
        word_count = sum(1 for n in walk(self.tree) if n.kind == "word")
        forecast = self.predict_next_chunk_size()
        return {
            "chunks": self.structure.count,
            "headings": len(self.structure.headings),
            "sentences": sentence_count,
            "words": word_count,
            "outline": self.outline()[:12],
            "top_terms": self.semantics.top_terms(10),
            "self_perplexity": round(self.language_model.perplexity(self.text), 2),
            "next_chunk_size": {
                "bytes": forecast.bytes,
                "range": [forecast.low, forecast.high],
                "confidence": forecast.confidence,
            },
        }


if __name__ == "__main__":  # pragma: no cover - manual smoke run
    import json
    import sys
    from pathlib import Path

    source = Path(sys.argv[1]) if len(sys.argv) > 1 else None
    text = source.read_text("utf-8", errors="ignore") if source else (
        "# Demo\n\nThe quick brown fox jumps over the lazy dog. "
        "The quick fox runs.\n\n## Details\n\nFoxes are quick animals."
    )
    intelligence = DocumentIntelligence(text)
    print(json.dumps(intelligence.report(), indent=2))
    plan = intelligence.optimized_plan(target_bytes=1200)
    print(
        f"\noptimized plan ({plan.method}): {len(plan.spans)} chunks from "
        f"{plan.unit_count} units, cost {plan.initial_cost:.4f} -> {plan.cost:.4f}"
    )
