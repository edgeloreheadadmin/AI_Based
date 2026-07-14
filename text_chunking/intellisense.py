"""IntelliSense-style completion for documents.

Editor-flavoured intelligence built from a document corpus:

* **prefix completion** from a frequency-weighted trie over the vocabulary,
* **context ranking**: candidates that actually follow the preceding word in
  the corpus (bigram evidence) are boosted, so ``complete("chu", "text")``
  prefers the word the author uses after "text",
* **symbol completion**: document heading titles are offered as symbols,
* **fuzzy fallback**: subsequence matching when prefix matches run out,
* **hover information**: frequency, first location and co-occurring terms.

Stdlib only.
"""

from __future__ import annotations

from collections import Counter, defaultdict
from dataclasses import dataclass, field

from .text_structure_chunker import chunk_text
from .tokenizer import TokenType, tokenize

__all__ = ["Suggestion", "CompletionEngine", "Trie"]

_CONTEXT_BOOST = 25.0
_SYMBOL_BASE = 40.0
_FUZZY_FACTOR = 0.3


@dataclass
class Suggestion:
    text: str
    score: float
    kind: str = "word"  # "word" | "symbol"
    detail: str = ""


class _TrieNode:
    __slots__ = ("children", "count")

    def __init__(self) -> None:
        self.children: dict[str, _TrieNode] = {}
        self.count = 0  # > 0 marks a complete word


class Trie:
    """Frequency-weighted prefix tree over lowercase words."""

    def __init__(self) -> None:
        self._root = _TrieNode()

    def insert(self, word: str, count: int = 1) -> None:
        node = self._root
        for char in word:
            node = node.children.setdefault(char, _TrieNode())
        node.count += count

    def complete(self, prefix: str) -> list[tuple[str, int]]:
        """All ``(word, count)`` entries under ``prefix``."""

        node = self._root
        for char in prefix:
            child = node.children.get(char)
            if child is None:
                return []
            node = child
        found: list[tuple[str, int]] = []
        stack = [(prefix, node)]
        while stack:
            word, current = stack.pop()
            if current.count:
                found.append((word, current.count))
            for char, child in current.children.items():
                stack.append((word + char, child))
        return found


def _is_subsequence(needle: str, haystack: str) -> bool:
    it = iter(haystack)
    return all(char in it for char in needle)


@dataclass
class _TermInfo:
    frequency: int = 0
    first_line: int = 0
    surfaces: Counter = field(default_factory=Counter)


class CompletionEngine:
    """Build completion intelligence from a document (or corpus) string."""

    def __init__(self, text: str) -> None:
        self._terms: dict[str, _TermInfo] = {}
        self._bigrams: Counter = Counter()
        self._follows: defaultdict[str, Counter] = defaultdict(Counter)
        self._trie = Trie()

        previous: str | None = None
        for token in tokenize(text):
            if token.type is not TokenType.WORD:
                previous = None
                continue
            low = token.value.lower()
            info = self._terms.setdefault(low, _TermInfo(first_line=token.line))
            info.frequency += 1
            info.surfaces[token.value] += 1
            if previous is not None:
                self._bigrams[(previous, low)] += 1
                self._follows[previous][low] += 1
            previous = low

        for word, info in self._terms.items():
            self._trie.insert(word, info.frequency)

        # Heading titles double as completable symbols.
        self.symbols: list[str] = [h.title for h in chunk_text(text).headings]

    # -- completion -------------------------------------------------------------

    def _surface(self, word: str) -> str:
        info = self._terms.get(word)
        if not info or not info.surfaces:
            return word
        return info.surfaces.most_common(1)[0][0]

    def complete(self, prefix: str, *, context: str = "", k: int = 8) -> list[Suggestion]:
        """Rank completions for ``prefix``, optionally biased by ``context``.

        ``context`` is the text before the prefix; its final word is used for
        bigram boosting.  Results mix vocabulary words and heading symbols.
        """

        prefix_low = prefix.lower()
        if not prefix_low:
            return []

        context_words = [
            t.value.lower() for t in tokenize(context) if t.type is TokenType.WORD
        ]
        previous = context_words[-1] if context_words else None

        suggestions: dict[str, Suggestion] = {}

        for word, frequency in self._trie.complete(prefix_low):
            if word == prefix_low and frequency == 0:
                continue
            score = float(frequency)
            detail = f"{frequency}x in document"
            if previous is not None:
                pair = self._bigrams.get((previous, word), 0)
                if pair:
                    score += _CONTEXT_BOOST * pair
                    detail += f", follows '{previous}' {pair}x"
            suggestions[word] = Suggestion(
                text=self._surface(word), score=score, kind="word", detail=detail
            )

        for title in self.symbols:
            low = title.lower()
            if low.startswith(prefix_low) or _is_subsequence(prefix_low, low):
                key = f"symbol:{low}"
                exact = low.startswith(prefix_low)
                suggestions[key] = Suggestion(
                    text=title,
                    score=_SYMBOL_BASE + (10.0 if exact else 0.0),
                    kind="symbol",
                    detail="document heading",
                )

        if len([s for s in suggestions.values() if s.kind == "word"]) < k:
            for word, info in self._terms.items():
                if word in suggestions or word.startswith(prefix_low):
                    continue
                if _is_subsequence(prefix_low, word):
                    suggestions[word] = Suggestion(
                        text=self._surface(word),
                        score=info.frequency * _FUZZY_FACTOR,
                        kind="word",
                        detail=f"fuzzy match, {info.frequency}x in document",
                    )

        ranked = sorted(suggestions.values(), key=lambda s: (-s.score, s.text.lower()))
        return ranked[:k]

    # -- hover ---------------------------------------------------------------------

    def hover(self, word: str, *, related_k: int = 5) -> dict | None:
        """Term info card: frequency, first line, and co-occurring words."""

        low = word.lower()
        info = self._terms.get(low)
        if info is None:
            return None
        related = Counter()
        related.update(self._follows.get(low, Counter()))
        for (first, second), count in self._bigrams.items():
            if second == low:
                related[first] += count
        related.pop(low, None)
        return {
            "term": self._surface(low),
            "frequency": info.frequency,
            "first_line": info.first_line,
            "related": [term for term, _ in related.most_common(related_k)],
        }
