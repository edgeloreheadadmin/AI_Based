"""Unit tests for the dependency-free :mod:`text_structure_chunker` engine.

Run from the repository root with::

    python -m unittest discover -s text_chunking -t .
"""

from __future__ import annotations

import unittest

from text_chunking.text_structure_chunker import chunk_text


def kinds(result):
    return [c.kind for c in result.chunks]


def titles(result):
    return [c.heading_titles for c in result.chunks]


def covered_lines(result):
    """Every line index covered by the union of chunk spans (1-based)."""

    covered = set()
    for c in result.chunks:
        covered.update(range(c.start_line, c.end_line + 1))
    return covered


class HeadingDetectionTests(unittest.TestCase):
    def test_atx_headings(self):
        result = chunk_text("# One\nbody\n# Two\nbody")
        self.assertEqual(kinds(result), ["section", "section"])
        self.assertEqual(titles(result), [["One"], ["Two"]])

    def test_atx_requires_space(self):
        # "#hashtag" is not a heading -> whole thing is one unstructured doc.
        result = chunk_text("#hashtag is not a heading")
        self.assertEqual(kinds(result), ["document"])

    def test_atx_closing_hashes_stripped(self):
        result = chunk_text("## Title ##\nbody")
        self.assertEqual(result.chunks[0].heading_titles, ["Title"])

    def test_setext_headings(self):
        text = "Title One\n=========\nbody\n\nTitle Two\n---------\nbody"
        result = chunk_text(text)
        # '=' -> h1, '-' -> nested h2, so a single top-level section lists both.
        self.assertEqual(titles(result), [["Title One", "Title Two"]])
        self.assertEqual(result.headings[0].depth, 1)
        self.assertEqual(result.headings[1].depth, 2)

    def test_setext_not_triggered_mid_paragraph(self):
        # A '---' directly under a multi-line paragraph is not a heading.
        text = "first line of paragraph\nsecond line of paragraph\n---"
        result = chunk_text(text)
        self.assertEqual(kinds(result), ["document"])

    def test_numbered_headings(self):
        text = "1. Introduction\nbody\n1.1 Background\nmore\n2. Methods\nbody"
        result = chunk_text(text)
        self.assertEqual(titles(result), [["Introduction", "Background"], ["Methods"]])

    def test_numbered_rejects_plain_numeric_lines(self):
        for line in ("42 apples on the table", "2024 was a good year"):
            result = chunk_text(line)
            self.assertEqual(kinds(result), ["document"], line)

    def test_numbered_rejects_lowercase_todo_list(self):
        text = "1. buy milk\n2. walk dog\n3. write code"
        result = chunk_text(text)
        self.assertEqual(kinds(result), ["document"])

    def test_headings_inside_code_fence_ignored(self):
        text = "# Real\nbody\n```\n# not a heading\n```\nmore"
        result = chunk_text(text)
        self.assertEqual(titles(result), [["Real"]])

    def test_yaml_front_matter_ignored(self):
        text = "---\ntitle: Doc\ntags: [a, b]\n---\n# Real Heading\nbody"
        result = chunk_text(text)
        # Front matter becomes preamble content; the closing '---' must not be
        # read as a setext heading, so "Real Heading" is the only heading.
        self.assertEqual([h.title for h in result.headings], ["Real Heading"])
        self.assertEqual(result.chunks[0].kind, "preamble")
        self.assertEqual(result.chunks[-1].heading_titles, ["Real Heading"])


class CoverageTests(unittest.TestCase):
    def test_preamble_is_captured(self):
        text = "preamble line one\npreamble line two\n# Heading\nbody"
        result = chunk_text(text)
        self.assertEqual(result.chunks[0].kind, "preamble")
        self.assertIn("preamble line one", result.chunks[0].content)

    def test_full_non_overlapping_coverage(self):
        text = "pre\n# A\na1\n## A1\na2\n# B\nb1\n"
        result = chunk_text(text)
        total_lines = len(text.replace("\r\n", "\n").split("\n"))
        self.assertEqual(covered_lines(result), set(range(1, total_lines + 1)))
        # Non-overlapping: spans are contiguous and ordered.
        spans = [(c.start_line, c.end_line) for c in result.chunks]
        for (_, prev_end), (next_start, _) in zip(spans, spans[1:]):
            self.assertEqual(next_start, prev_end + 1)

    def test_no_headings_returns_whole_document(self):
        text = "just some text\nover two lines"
        result = chunk_text(text)
        self.assertEqual(kinds(result), ["document"])
        self.assertEqual(result.chunks[0].content, text)


class StructureTests(unittest.TestCase):
    def test_nested_titles_and_depth(self):
        text = "# Root\n## Child\n### Grandchild\nbody"
        result = chunk_text(text)
        self.assertEqual(result.chunks[0].heading_titles, ["Root", "Child", "Grandchild"])
        self.assertEqual(result.chunks[0].max_depth, 3)

    def test_include_nested_false_lists_only_roots(self):
        text = "# Root\n## Child\nbody\n# Root2\nbody"
        result = chunk_text(text, include_nested=False)
        self.assertEqual(titles(result), [["Root"], ["Root2"]])
        self.assertTrue(all(c.max_depth in (0, 1) for c in result.chunks))

    def test_parent_indices(self):
        text = "# Root\n## Child\n### Grandchild\nbody"
        result = chunk_text(text)
        by_title = {h.title: h for h in result.headings}
        self.assertIsNone(by_title["Root"].parent_index)
        self.assertEqual(by_title["Child"].parent_index, by_title["Root"].index)
        self.assertEqual(by_title["Grandchild"].parent_index, by_title["Child"].index)

    def test_headings_scoped_to_emitted_chunks(self):
        text = "# Alone\nbody\n# Pair\n## Sub\nbody"
        result = chunk_text(text, include_singletons=False)
        # The single-heading "Alone" section is filtered out.
        self.assertEqual(titles(result), [["Pair", "Sub"]])
        self.assertNotIn("Alone", [h.title for h in result.headings])

    def test_mixed_root_depth_uses_minimum(self):
        # Document starts at depth 2 then introduces depth 1.
        text = "## Early\nbody\n# Later\nbody"
        result = chunk_text(text)
        self.assertEqual(result.root_depth, 1)
        # "## Early" precedes the first root heading -> it is preamble content.
        self.assertEqual(result.chunks[0].kind, "preamble")
        self.assertEqual(result.chunks[1].kind, "section")
        self.assertEqual(result.chunks[1].heading_titles[0], "Later")


class FilterTests(unittest.TestCase):
    def test_include_singletons_false_drops_single_heading_sections(self):
        text = "# Solo\nbody only"
        result = chunk_text(text, include_singletons=False)
        self.assertEqual(result.chunks, [])

    def test_max_chunks_truncates(self):
        text = "# A\n# B\n# C\n# D"
        result = chunk_text(text, max_chunks=2)
        self.assertTrue(result.truncated)
        self.assertEqual(result.count, 2)

    def test_max_chunks_must_be_positive(self):
        with self.assertRaises(ValueError):
            chunk_text("# A", max_chunks=0)


class SizeSplittingTests(unittest.TestCase):
    def test_oversized_section_is_split_into_parts(self):
        body = "\n".join(f"line {i} with some filler text" for i in range(200))
        text = f"# Big\n{body}"
        result = chunk_text(text, max_chunk_bytes=200)
        self.assertGreater(result.count, 1)
        self.assertTrue(all(c.kind == "section" for c in result.chunks))
        # Every emitted chunk respects the byte budget.
        for c in result.chunks:
            self.assertLessEqual(len(c.content.encode("utf-8")), 200)
        # Parts are numbered and share a part_count.
        self.assertEqual([c.part for c in result.chunks], list(range(1, result.count + 1)))
        self.assertTrue(all(c.part_count == result.count for c in result.chunks))

    def test_single_huge_line_is_force_split(self):
        text = "# H\n" + ("x" * 1000)
        result = chunk_text(text, max_chunk_bytes=100)
        for c in result.chunks:
            self.assertLessEqual(len(c.content.encode("utf-8")), 100)

    def test_split_disabled_keeps_one_chunk(self):
        body = "\n".join(f"line {i}" for i in range(100))
        text = f"# Big\n{body}"
        result = chunk_text(text, max_chunk_bytes=50, split_oversized=False)
        self.assertEqual(result.count, 1)

    def test_density_metric(self):
        text = "# A\n## B\n## C\nbody line\nbody line"
        result = chunk_text(text)
        chunk = result.chunks[0]
        self.assertEqual(chunk.heading_count, 3)
        self.assertAlmostEqual(chunk.density, 3 / chunk.line_span)
        self.assertAlmostEqual(chunk.density_per_100_lines, chunk.density * 100)


class EdgeCaseTests(unittest.TestCase):
    def test_empty_input(self):
        result = chunk_text("")
        self.assertEqual(result.count, 0)
        self.assertEqual(result.chunks, [])

    def test_crlf_normalisation(self):
        result = chunk_text("# A\r\nbody\r\n# B\r\nbody")
        self.assertEqual(titles(result), [["A"], ["B"]])
        self.assertNotIn("\r", result.chunks[0].content)


if __name__ == "__main__":
    unittest.main(verbosity=2)
