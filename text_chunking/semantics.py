"""Semantic intelligence over chunks and documents.

Vector-space semantics without any external dependency:

* :class:`TfIdfVectorizer` -- sparse TF-IDF vectors over stemmed, stopword-
  filtered terms, with cosine similarity.
* :class:`SemanticIndex`   -- semantic search, chunk-to-chunk similarity,
  keyword extraction and related-chunk lookup, buildable straight from a
  :class:`~.text_structure_chunker.ChunkingResult`.
* :func:`semantic_boundaries` -- TextTiling-style topic-shift detection:
  finds sentence indices where lexical cohesion drops, i.e. semantically
  motivated chunk boundaries.
* :func:`cohesion` -- pairwise semantic cohesion score used by the
  meta-heuristic chunk optimizer.
"""

from __future__ import annotations

from collections import Counter
from dataclasses import dataclass
import math

from .syntax_tree import split_sentences
from .text_structure_chunker import ChunkingResult
from .tokenizer import words

__all__ = [
    "STOPWORDS",
    "TfIdfVectorizer",
    "SemanticIndex",
    "SearchHit",
    "cohesion",
    "semantic_boundaries",
    "terms",
]

STOPWORDS = frozenset(
    """a an the and or but if while of to in on at by for with from into over
    under about as is are was were be been being am has have had do does did
    this that these those it its i you he she we they them his her their our
    your not no nor yes can could may might must shall should will would there
    here when where which who whom what why how all any each every some such
    only own same so than too very just also then once more most other s t
    don now""".split()
)


def _stem(word: str) -> str:
    """Very light suffix stripping -- enough to conflate plural/verbal forms."""

    if len(word) > 4 and word.endswith("ies"):
        return word[:-3] + "y"
    if len(word) > 5 and word.endswith("sses"):
        return word[:-2]
    if len(word) > 3 and word.endswith("s") and not word.endswith(("ss", "us", "is")):
        word = word[:-1]
    if len(word) > 5 and word.endswith("ing"):
        return word[:-3]
    if len(word) > 4 and word.endswith("ed"):
        return word[:-2]
    return word


def terms(text: str) -> list[str]:
    """Stemmed, stopword-filtered content terms of ``text``."""

    return [
        _stem(w) for w in words(text) if w not in STOPWORDS and len(w) > 1
    ]


class TfIdfVectorizer:
    """Sparse TF-IDF vectors (dict term -> weight, L2-normalised)."""

    def __init__(self) -> None:
        self.idf: dict[str, float] = {}
        self.document_count = 0

    def fit(self, documents: list[str]) -> "TfIdfVectorizer":
        self.document_count = len(documents)
        frequencies = Counter()
        for document in documents:
            frequencies.update(set(terms(document)))
        self.idf = {
            term: math.log((1 + self.document_count) / (1 + df)) + 1.0
            for term, df in frequencies.items()
        }
        return self

    def transform(self, document: str) -> dict[str, float]:
        counts = Counter(terms(document))
        vector = {
            term: count * self.idf.get(term, 0.0) for term, count in counts.items()
        }
        norm = math.sqrt(sum(weight * weight for weight in vector.values()))
        if norm == 0:
            return {}
        return {term: weight / norm for term, weight in vector.items()}

    @staticmethod
    def cosine(a: dict[str, float], b: dict[str, float]) -> float:
        if len(b) < len(a):
            a, b = b, a
        return sum(weight * b.get(term, 0.0) for term, weight in a.items())


def cohesion(text_a: str, text_b: str) -> float:
    """Semantic cohesion between two texts in [0, 1]."""

    vectorizer = TfIdfVectorizer().fit([text_a, text_b])
    return TfIdfVectorizer.cosine(
        vectorizer.transform(text_a), vectorizer.transform(text_b)
    )


@dataclass(frozen=True)
class SearchHit:
    index: int
    score: float
    label: str


class SemanticIndex:
    """Semantic search + similarity over a set of texts (usually chunks)."""

    def __init__(self, texts: list[str], labels: list[str] | None = None) -> None:
        self.texts = texts
        self.labels = labels or [f"chunk {i + 1}" for i in range(len(texts))]
        self._vectorizer = TfIdfVectorizer().fit(texts) if texts else TfIdfVectorizer()
        self._vectors = [self._vectorizer.transform(t) for t in texts]

    @classmethod
    def from_result(cls, result: ChunkingResult) -> "SemanticIndex":
        texts = [chunk.content for chunk in result.chunks]
        labels = [
            chunk.heading_titles[0] if chunk.heading_titles else chunk.kind
            for chunk in result.chunks
        ]
        return cls(texts, labels)

    def __len__(self) -> int:
        return len(self.texts)

    def search(self, query: str, k: int = 3) -> list[SearchHit]:
        """Chunks ranked by cosine similarity to ``query``."""

        query_vector = self._vectorizer.transform(query)
        if not query_vector:
            return []
        scored = [
            SearchHit(index=i, score=TfIdfVectorizer.cosine(query_vector, v), label=self.labels[i])
            for i, v in enumerate(self._vectors)
        ]
        scored = [hit for hit in scored if hit.score > 0]
        scored.sort(key=lambda hit: (-hit.score, hit.index))
        return scored[:k]

    def similarity(self, i: int, j: int) -> float:
        return TfIdfVectorizer.cosine(self._vectors[i], self._vectors[j])

    def similarity_matrix(self) -> list[list[float]]:
        n = len(self._vectors)
        return [[self.similarity(i, j) for j in range(n)] for i in range(n)]

    def keywords(self, index: int, k: int = 8) -> list[str]:
        """Top TF-IDF terms of one chunk."""

        vector = self._vectors[index]
        return [t for t, _ in sorted(vector.items(), key=lambda kv: (-kv[1], kv[0]))[:k]]

    def top_terms(self, k: int = 10) -> list[str]:
        """Top TF-IDF terms across the whole corpus."""

        totals: Counter = Counter()
        for vector in self._vectors:
            totals.update(vector)
        return [t for t, _ in sorted(totals.items(), key=lambda kv: (-kv[1], kv[0]))[:k]]

    def related(self, index: int, k: int = 3) -> list[SearchHit]:
        """Chunks most similar to chunk ``index`` (excluding itself)."""

        scored = [
            SearchHit(index=j, score=self.similarity(index, j), label=self.labels[j])
            for j in range(len(self._vectors))
            if j != index
        ]
        scored.sort(key=lambda hit: (-hit.score, hit.index))
        return scored[:k]

    def coherence(self, text: str) -> float:
        """Mean adjacent-sentence similarity: how well a text hangs together."""

        sentences = split_sentences(text)
        if len(sentences) < 2:
            return 1.0
        vectorizer = TfIdfVectorizer().fit(sentences)
        vectors = [vectorizer.transform(s) for s in sentences]
        similarities = [
            TfIdfVectorizer.cosine(vectors[i], vectors[i + 1])
            for i in range(len(vectors) - 1)
        ]
        return sum(similarities) / len(similarities)


def _merge(vectors: list[dict[str, float]]) -> dict[str, float]:
    merged: Counter = Counter()
    for vector in vectors:
        merged.update(vector)
    norm = math.sqrt(sum(w * w for w in merged.values()))
    return {t: w / norm for t, w in merged.items()} if norm else {}


def semantic_boundaries(text: str, *, window: int = 2) -> list[int]:
    """Sentence indices where the topic shifts (TextTiling-style).

    Compares a sliding window of sentences on each side of every candidate
    gap; gaps whose cohesion falls below ``mean - std/2`` and that are local
    minima are reported as boundaries.  Returns indices ``i`` meaning "a new
    topic starts at sentence ``i``".
    """

    sentences = split_sentences(text)
    if len(sentences) < 2 * window:
        return []

    vectorizer = TfIdfVectorizer().fit(sentences)
    vectors = [vectorizer.transform(s) for s in sentences]

    gaps: list[tuple[int, float]] = []
    for i in range(window, len(sentences) - window + 1):
        left = _merge(vectors[i - window : i])
        right = _merge(vectors[i : i + window])
        gaps.append((i, TfIdfVectorizer.cosine(left, right)))

    scores = [score for _, score in gaps]
    mean = sum(scores) / len(scores)
    std = math.sqrt(sum((s - mean) ** 2 for s in scores) / len(scores))
    threshold = mean - std / 2

    boundaries: list[int] = []
    for position, (index, score) in enumerate(gaps):
        if score >= threshold:
            continue
        before = gaps[position - 1][1] if position > 0 else float("inf")
        after = gaps[position + 1][1] if position + 1 < len(gaps) else float("inf")
        if score <= before and score <= after:
            boundaries.append(index)
    return boundaries
