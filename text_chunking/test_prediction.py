"""Tests for the predictive layer: n-gram LM, intellisense, chunk-size."""

from __future__ import annotations

import unittest

from text_chunking.chunk_predictor import ChunkSizePredictor
from text_chunking.intellisense import CompletionEngine
from text_chunking.language_model import NGramModel


CORPUS = (
    "The cat sat on the mat. The cat ran away. "
    "The dog sat on the rug. The dog barked loudly."
)


class NGramModelTests(unittest.TestCase):
    def setUp(self):
        self.model = NGramModel(order=3).fit(CORPUS)

    def test_next_token_prefers_frequent_follower(self):
        top = self.model.predict_next("the", k=3)
        # "the cat" and "the dog" both occur twice; deterministic tie-break
        # is alphabetical, and both outrank "the mat"/"the rug" (once each).
        self.assertEqual(top[0][0], "cat")
        self.assertTrue(all(0 < p <= 1 for _, p in top))

    def test_longer_context_wins(self):
        top = [t for t, _ in self.model.predict_next("sat on the", k=2)]
        self.assertEqual(set(top), {"mat", "rug"})

    def test_next_word_filters_punctuation(self):
        candidates = self.model.predict_next_word("on the mat", k=5)
        self.assertTrue(candidates)
        for token, _ in candidates:
            self.assertNotIn(token, {".", ",", "</s>", "<s>"})

    def test_scores_normalised(self):
        scores = [p for _, p in self.model.predict_next("the", k=5)]
        self.assertAlmostEqual(sum(scores), 1.0, places=6)

    def test_perplexity_orders_texts(self):
        familiar = self.model.perplexity("The cat sat on the mat.")
        alien = self.model.perplexity("Zebra quantum flux nebula recursion.")
        self.assertLess(familiar, alien)

    def test_generate_deterministic(self):
        first = self.model.generate("the", n=6, seed=42)
        second = self.model.generate("the", n=6, seed=42)
        self.assertEqual(first, second)
        self.assertTrue(first)

    def test_unfitted_probability_raises(self):
        with self.assertRaises(ValueError):
            NGramModel().probability("cat")

    def test_invalid_order(self):
        with self.assertRaises(ValueError):
            NGramModel(order=0)


class IntelliSenseTests(unittest.TestCase):
    TEXT = (
        "# Chunk Planning\n\n"
        "chunks chunks chunks text chunking. "
        "chunky prose is chunky. checkpoint reached."
    )

    def setUp(self):
        self.engine = CompletionEngine(self.TEXT)

    def test_prefix_completion_by_frequency(self):
        names = [s.text for s in self.engine.complete("chu", k=3) if s.kind == "word"]
        self.assertEqual(names[0], "chunks")  # 3 occurrences beats the rest

    def test_context_bigram_boost(self):
        suggestions = self.engine.complete("chu", context="some text", k=3)
        word_names = [s.text for s in suggestions if s.kind == "word"]
        self.assertEqual(word_names[0], "chunking")  # follows "text" in corpus

    def test_symbol_completion_from_headings(self):
        suggestions = self.engine.complete("chunk", k=8)
        symbols = [s for s in suggestions if s.kind == "symbol"]
        self.assertEqual(len(symbols), 1)
        self.assertEqual(symbols[0].text, "Chunk Planning")

    def test_fuzzy_fallback(self):
        suggestions = self.engine.complete("ckpt", k=8)
        self.assertIn("checkpoint", [s.text for s in suggestions])

    def test_hover(self):
        card = self.engine.hover("chunks")
        self.assertEqual(card["frequency"], 3)
        self.assertGreaterEqual(card["first_line"], 1)
        self.assertIsNone(self.engine.hover("missing"))


class ChunkSizePredictorTests(unittest.TestCase):
    def test_constant_series(self):
        predictor = ChunkSizePredictor()
        for _ in range(10):
            predictor.observe(100, 5, "section")
        forecast = predictor.predict()
        self.assertAlmostEqual(forecast.bytes, 100, delta=5)
        self.assertLessEqual(forecast.low, 100)
        self.assertGreaterEqual(forecast.high, 100)
        self.assertGreater(forecast.confidence, 0.8)

    def test_trending_series_extrapolates(self):
        predictor = ChunkSizePredictor()
        for size in range(100, 200, 10):
            predictor.observe(size)
        self.assertGreater(predictor.predict().bytes, 170)

    def test_kind_prior_blends(self):
        predictor = ChunkSizePredictor()
        for _ in range(5):
            predictor.observe(100, kind="section")
        for _ in range(5):
            predictor.observe(10, kind="preamble")
        with_kind = predictor.predict(kind="section").bytes
        without = predictor.predict().bytes
        self.assertGreater(with_kind, without)

    def test_empty_predictor(self):
        forecast = ChunkSizePredictor().predict()
        self.assertEqual((forecast.bytes, forecast.based_on), (0, 0))

    def test_fit_result(self):
        from text_chunking.text_structure_chunker import chunk_text

        result = chunk_text("# A\nbody\n# B\nbody\n# C\nbody")
        predictor = ChunkSizePredictor().fit_result(result)
        self.assertEqual(predictor.observations, result.count)
        self.assertGreater(predictor.predict().bytes, 0)


if __name__ == "__main__":
    unittest.main(verbosity=2)
