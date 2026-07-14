"""N-gram language model: next-token and next-word prediction.

A classic count-based model, trained on the document itself (or any corpus),
with:

* **stupid-backoff** candidate ranking for :meth:`NGramModel.predict_next`
  (longest matching context wins, shorter contexts back off at 0.4x),
* **interpolated add-one probabilities** for :meth:`NGramModel.probability`
  and :meth:`NGramModel.perplexity` (always finite, even for unseen words),
* deterministic, seedable :meth:`NGramModel.generate`.

Sentences are the training unit: each is padded with ``<s>``/``</s>`` markers
so the model learns sentence starts and ends.  Stdlib only.
"""

from __future__ import annotations

from collections import Counter, defaultdict
from typing import Iterable
import math
import random
import re

from .syntax_tree import split_sentences
from .tokenizer import lexical_tokens

__all__ = ["NGramModel", "BOS", "EOS"]

BOS = "<s>"
EOS = "</s>"

_WORDLIKE_RE = re.compile(r"[a-z]")
_BACKOFF = 0.4


class NGramModel:
    """Count-based n-gram model over lexical tokens (words, numbers, punct)."""

    def __init__(self, order: int = 3) -> None:
        if order < 1:
            raise ValueError("order must be >= 1")
        self.order = order
        # _contexts[L][ctx_tuple] -> Counter of next tokens, for L = 0..order-1
        self._contexts: list[defaultdict[tuple[str, ...], Counter]] = [
            defaultdict(Counter) for _ in range(order)
        ]
        self.vocabulary: set[str] = set()
        self.token_count = 0

    # -- training -------------------------------------------------------------

    def fit(self, corpus: str | Iterable[str]) -> "NGramModel":
        """Train on a text or an iterable of texts. Returns ``self``."""

        texts = [corpus] if isinstance(corpus, str) else list(corpus)
        for text in texts:
            for sentence in split_sentences(text):
                tokens = lexical_tokens(sentence)
                if not tokens:
                    continue
                padded = [BOS] * (self.order - 1) + tokens + [EOS]
                for i in range(self.order - 1, len(padded)):
                    token = padded[i]
                    self.vocabulary.add(token)
                    self.token_count += 1
                    for length in range(self.order):
                        context = tuple(padded[i - length : i])
                        self._contexts[length][context][token] += 1
        return self

    # -- prediction -------------------------------------------------------------

    def _context_tokens(self, context: str | Iterable[str]) -> tuple[str, ...]:
        tokens = lexical_tokens(context) if isinstance(context, str) else list(context)
        return tuple(tokens[-(self.order - 1) :]) if self.order > 1 else ()

    def _backoff_scores(self, context: tuple[str, ...]) -> dict[str, float]:
        scores: dict[str, float] = {}
        weight = 1.0
        for length in range(len(context), -1, -1):
            sub = context[len(context) - length :]
            counter = self._contexts[length].get(sub)
            if counter:
                total = sum(counter.values())
                for token, count in counter.items():
                    scores.setdefault(token, weight * count / total)
            weight *= _BACKOFF
        return scores

    def predict_next(
        self,
        context: str | Iterable[str] = "",
        k: int = 5,
        *,
        word_only: bool = False,
    ) -> list[tuple[str, float]]:
        """Top-``k`` next tokens after ``context``, as ``(token, score)`` pairs.

        Scores are stupid-backoff scores normalised over the returned
        candidates.  ``word_only=True`` restricts candidates to word-like
        tokens (this is "next word prediction"; the default is "next token
        prediction" and may return punctuation).
        """

        scores = self._backoff_scores(self._context_tokens(context))
        scores.pop(BOS, None)
        scores.pop(EOS, None)
        if word_only:
            scores = {t: s for t, s in scores.items() if _WORDLIKE_RE.search(t)}
        ranked = sorted(scores.items(), key=lambda kv: (-kv[1], kv[0]))[:k]
        total = sum(score for _, score in ranked)
        if total <= 0:
            return []
        return [(token, score / total) for token, score in ranked]

    def predict_next_word(
        self, context: str | Iterable[str] = "", k: int = 5
    ) -> list[tuple[str, float]]:
        """Convenience alias for word-only next-token prediction."""

        return self.predict_next(context, k, word_only=True)

    # -- probabilities ----------------------------------------------------------

    def probability(self, token: str, context: str | Iterable[str] = "") -> float:
        """Interpolated probability of ``token`` after ``context``.

        Mixes maximum-likelihood estimates of every available order with an
        add-one-smoothed unigram floor, so the result is always positive.
        """

        if self.token_count == 0:
            raise ValueError("model is not fitted")
        ctx = self._context_tokens(context)
        raw_weights = [2.0**length for length in range(len(ctx) + 1)]  # favour long ctx
        total_weight = sum(raw_weights)

        vocab_size = len(self.vocabulary) + 1  # +1 reserves mass for unseen tokens
        unigram = self._contexts[0][()]
        prob = 0.0
        for length, raw_weight in enumerate(raw_weights):
            weight = raw_weight / total_weight
            if length == 0:
                prob += weight * (unigram.get(token, 0) + 1) / (self.token_count + vocab_size)
                continue
            counter = self._contexts[length].get(ctx[len(ctx) - length :])
            if counter:
                total = sum(counter.values())
                prob += weight * counter.get(token, 0) / total
        return prob

    def perplexity(self, text: str) -> float:
        """Per-token perplexity of ``text`` under the model (lower is better)."""

        log_prob = 0.0
        count = 0
        for sentence in split_sentences(text):
            tokens = lexical_tokens(sentence)
            if not tokens:
                continue
            padded = [BOS] * (self.order - 1) + tokens + [EOS]
            for i in range(self.order - 1, len(padded)):
                log_prob += math.log(self.probability(padded[i], padded[:i]))
                count += 1
        if count == 0:
            return float("inf")
        return math.exp(-log_prob / count)

    # -- generation --------------------------------------------------------------

    def generate(self, seed_text: str = "", n: int = 20, *, seed: int = 0) -> str:
        """Sample up to ``n`` tokens continuing ``seed_text`` (deterministic)."""

        rng = random.Random(seed)
        tokens = [BOS] * (self.order - 1) + lexical_tokens(seed_text)
        generated: list[str] = []
        for _ in range(n):
            scores = self._backoff_scores(tuple(tokens[-(self.order - 1) :]) if self.order > 1 else ())
            scores.pop(BOS, None)
            if not scores:
                break
            candidates = sorted(scores.items())
            values = [token for token, _ in candidates]
            weights = [score for _, score in candidates]
            choice = rng.choices(values, weights=weights, k=1)[0]
            if choice == EOS:
                break
            generated.append(choice)
            tokens.append(choice)
        out = " ".join(generated)
        return re.sub(r"\s+([.,;:!?])", r"\1", out)
