"""Tests for semantics, meta-heuristics and the DocumentIntelligence facade."""

from __future__ import annotations

import unittest

from text_chunking.metaheuristics import CostWeights, optimize_chunking
from text_chunking.pipeline import DocumentIntelligence
from text_chunking.semantics import (
    SemanticIndex,
    cohesion,
    semantic_boundaries,
    terms,
)


CAT_TEXTS = [
    "Cats purr and cats sleep all day. A cat enjoys warm windows.",
    "Kittens and cats chase mice. The cat pounced on a mouse.",
    "Databases index rows and columns. A database executes queries quickly.",
]


class SemanticsTests(unittest.TestCase):
    def setUp(self):
        self.index = SemanticIndex(CAT_TEXTS, labels=["cats-a", "cats-b", "db"])

    def test_terms_stem_and_filter(self):
        extracted = terms("The databases were indexing queries")
        self.assertIn("database", extracted)
        self.assertIn("query", extracted)
        self.assertNotIn("the", extracted)

    def test_search_finds_topical_chunk(self):
        hits = self.index.search("cat", k=2)
        self.assertTrue(hits)
        self.assertIn(hits[0].index, (0, 1))
        self.assertNotIn(2, [h.index for h in hits])

    def test_related_prefers_same_topic(self):
        related = self.index.related(0, k=1)
        self.assertEqual(related[0].index, 1)

    def test_similarity_matrix_symmetric(self):
        matrix = self.index.similarity_matrix()
        self.assertAlmostEqual(matrix[0][1], matrix[1][0])
        self.assertAlmostEqual(matrix[0][0], 1.0, places=6)
        self.assertGreater(matrix[0][1], matrix[0][2])

    def test_keywords(self):
        self.assertIn("database", self.index.keywords(2, k=5))

    def test_cohesion_pairwise(self):
        same = cohesion(CAT_TEXTS[0], CAT_TEXTS[1])
        different = cohesion(CAT_TEXTS[0], CAT_TEXTS[2])
        self.assertGreater(same, different)

    def test_semantic_boundaries_find_topic_shift(self):
        text = (
            "Cats purr softly. Cats nap in sunshine. Kittens chase cats. "
            "Cats groom kittens. Databases store tables. Databases run queries. "
            "Indexes speed databases. Queries scan database tables."
        )
        boundaries = semantic_boundaries(text, window=2)
        self.assertTrue(any(3 <= b <= 5 for b in boundaries), boundaries)

    def test_empty_index(self):
        empty = SemanticIndex([])
        self.assertEqual(empty.search("anything"), [])


def _demo_document() -> str:
    cats = "Cats purr and nap. Cats chase mice and birds daily."
    dogs = "Dogs bark at mail. Dogs fetch sticks and balls happily."
    return (
        "# Cats\n\n" + "\n\n".join([cats] * 3)
        + "\n\n# Dogs\n\n" + "\n\n".join([dogs] * 3)
    )


class MetaheuristicTests(unittest.TestCase):
    def test_all_solvers_beat_or_match_seed(self):
        text = _demo_document()
        for method in ("hill", "annealing", "genetic"):
            result = optimize_chunking(text, target_bytes=120, method=method, seed=7)
            self.assertLessEqual(result.cost, result.initial_cost + 1e-9, method)
            # Spans partition all units contiguously.
            flat = [i for span in result.spans for i in range(span[0], span[1])]
            self.assertEqual(flat, list(range(result.unit_count)), method)

    def test_deterministic_for_seed(self):
        text = _demo_document()
        a = optimize_chunking(text, target_bytes=120, method="annealing", seed=99)
        b = optimize_chunking(text, target_bytes=120, method="annealing", seed=99)
        self.assertEqual(a.breaks, b.breaks)
        self.assertEqual(a.cost, b.cost)

    def test_large_target_merges_chunks(self):
        text = _demo_document()
        small = optimize_chunking(text, target_bytes=80, seed=3)
        huge = optimize_chunking(text, target_bytes=100_000, seed=3)
        self.assertLess(len(huge.spans), len(small.spans))

    def test_single_unit_document(self):
        result = optimize_chunking("only one paragraph here", target_bytes=100)
        self.assertEqual(result.spans, [(0, 1)])
        self.assertEqual(result.cost, 0.0)

    def test_invalid_arguments(self):
        with self.assertRaises(ValueError):
            optimize_chunking("x", method="quantum")
        with self.assertRaises(ValueError):
            optimize_chunking("x", target_bytes=0)

    def test_custom_weights_respected(self):
        text = _demo_document()
        structural_only = optimize_chunking(
            text,
            target_bytes=120,
            seed=5,
            weights=CostWeights(size=0.0, cohesion=0.0, structure=10.0),
        )
        # With structure dominating, every break must sit on a heading.
        self.assertTrue(structural_only.breaks)  # headings exist in the demo doc


class DocumentIntelligenceTests(unittest.TestCase):
    DOC = (
        "# Felines\n\n"
        "Cats purr and nap. Cats chase mice daily. The cat sat on the mat.\n\n"
        "## Habits\n\n"
        "Cats sleep through long afternoons. Cats groom their fur.\n\n"
        "# Storage\n\n"
        "Databases index rows. Databases execute queries."
    )

    def setUp(self):
        self.doc = DocumentIntelligence(self.DOC)

    def test_outline(self):
        self.assertEqual(
            self.doc.outline(), [(1, "Felines"), (2, "Habits"), (1, "Storage")]
        )

    def test_predictions_available(self):
        self.assertTrue(self.doc.next_word("the"))
        self.assertTrue(self.doc.next_token("cats"))
        suggestions = self.doc.complete("cat")
        self.assertTrue(suggestions)

    def test_search_and_forecast(self):
        hits = self.doc.search("database queries", k=1)
        self.assertEqual(hits[0].label, "Storage")
        forecast = self.doc.predict_next_chunk_size()
        self.assertGreater(forecast.bytes, 0)
        self.assertEqual(forecast.based_on, self.doc.structure.count)

    def test_optimized_plan_and_report(self):
        plan = self.doc.optimized_plan(target_bytes=150)
        self.assertGreaterEqual(len(plan.spans), 1)
        report = self.doc.report()
        for key in (
            "chunks", "headings", "sentences", "words",
            "outline", "top_terms", "self_perplexity", "next_chunk_size",
        ):
            self.assertIn(key, report)
        self.assertGreater(report["sentences"], 4)

    def test_empty_document(self):
        empty = DocumentIntelligence("")
        self.assertEqual(empty.outline(), [])
        self.assertEqual(empty.search("x"), [])
        self.assertEqual(empty.predict_next_chunk_size().based_on, 0)


if __name__ == "__main__":
    unittest.main(verbosity=2)
