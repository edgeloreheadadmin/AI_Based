"""Tests for the parsing layer: tokenizer, sentences, tagging, phrases, AST."""

from __future__ import annotations

import unittest

from text_chunking.syntax_tree import (
    chunk_phrases,
    find_all,
    parse_document,
    parse_sentence,
    split_sentences,
    tag_tokens,
    to_dict,
    to_sexpr,
    walk,
)
from text_chunking.tokenizer import TokenType, lexical_tokens, tokenize, words


class TokenizerTests(unittest.TestCase):
    def test_word_number_punctuation(self):
        tokens = tokenize("Pay 3.14 dollars, now!")
        types = [t.type for t in tokens]
        values = [t.value for t in tokens]
        self.assertEqual(values, ["Pay", "3.14", "dollars", ",", "now", "!"])
        self.assertEqual(types[1], TokenType.NUMBER)
        self.assertEqual(types[3], TokenType.PUNCTUATION)

    def test_hyphen_and_apostrophe_stay_in_words(self):
        values = [t.value for t in tokenize("don't split well-known words")]
        self.assertIn("don't", values)
        self.assertIn("well-known", values)

    def test_positions(self):
        tokens = tokenize("ab\ncd")
        self.assertEqual((tokens[0].line, tokens[0].column, tokens[0].offset), (1, 1, 0))
        self.assertEqual((tokens[1].line, tokens[1].column, tokens[1].offset), (2, 1, 3))

    def test_whitespace_excluded_by_default(self):
        self.assertTrue(all(t.type is not TokenType.SPACE for t in tokenize("a b")))
        with_ws = tokenize("a b", include_whitespace=True)
        self.assertIn(TokenType.SPACE, [t.type for t in with_ws])

    def test_words_and_lexical_tokens(self):
        self.assertEqual(words("The CAT sat"), ["the", "cat", "sat"])
        self.assertEqual(lexical_tokens("Go, 2 dogs!"), ["go", ",", "2", "dogs", "!"])


class SentenceSplitTests(unittest.TestCase):
    def test_basic_split(self):
        self.assertEqual(
            split_sentences("First one. Second one! Third one?"),
            ["First one.", "Second one!", "Third one?"],
        )

    def test_abbreviations_do_not_split(self):
        sentences = split_sentences("Dr. Smith went home. He slept.")
        self.assertEqual(sentences, ["Dr. Smith went home.", "He slept."])

    def test_initials_do_not_split(self):
        sentences = split_sentences("J. Smith wrote it. It was good.")
        self.assertEqual(len(sentences), 2)

    def test_multiline_paragraph(self):
        sentences = split_sentences("One line\nwraps here. Next sentence.")
        self.assertEqual(sentences[0], "One line wraps here.")


class TaggingTests(unittest.TestCase):
    def tags_for(self, text):
        return {tok.value: tag for tok, tag in tag_tokens(tokenize(text))}

    def test_closed_class_and_suffix_tags(self):
        tags = self.tags_for("she quickly walked the running path with 42 lights")
        self.assertEqual(tags["she"], "PRON")
        self.assertEqual(tags["quickly"], "ADV")
        self.assertEqual(tags["walked"], "VERB")
        self.assertEqual(tags["the"], "DET")
        self.assertEqual(tags["running"], "VERB")
        self.assertEqual(tags["with"], "PREP")
        self.assertEqual(tags["42"], "NUM")

    def test_proper_noun_mid_sentence(self):
        tags = self.tags_for("we visited Paris today")
        self.assertEqual(tags["Paris"], "PROPN")

    def test_sentence_initial_capital_not_propn(self):
        tags = self.tags_for("Walking is healthy")
        self.assertNotEqual(tags["Walking"], "PROPN")

    def test_adjective_and_noun_suffixes(self):
        tags = self.tags_for("a beautiful movement of brightness")
        self.assertEqual(tags["beautiful"], "ADJ")
        self.assertEqual(tags["movement"], "NOUN")
        self.assertEqual(tags["brightness"], "NOUN")


class PhraseTests(unittest.TestCase):
    def test_np_vp_pp_structure(self):
        node = parse_sentence("The quick brown fox jumped over the lazy dog.")
        labels = [p.label for p in node.children]
        self.assertEqual(labels, ["NP", "VP", "PP", "O"])
        np_words = [w.text for w in node.children[0].children]
        self.assertEqual(np_words, ["The", "quick", "brown", "fox"])

    def test_pp_contains_nested_np(self):
        node = parse_sentence("She slept in the barn.")
        pp = next(p for p in node.children if p.label == "PP")
        nested = [c for c in pp.children if getattr(c, "label", None) == "NP"]
        self.assertEqual(len(nested), 1)
        self.assertEqual([w.text for w in nested[0].children], ["the", "barn"])

    def test_modal_verb_phrase(self):
        tagged = tag_tokens(tokenize("systems should quickly respond"))
        phrases = chunk_phrases(tagged)
        vp = next(p for p in phrases if p.label == "VP")
        self.assertEqual([w.text for w in vp.children], ["should", "quickly", "respond"])


class SyntaxTreeTests(unittest.TestCase):
    DOC = (
        "---\ntitle: Demo\n---\n"
        "# Intro\n\n"
        "Hello world. This is a test.\n\n"
        "## Details\n\n"
        "- first item\n"
        "1. buy milk\n\n"
        "```python\nprint('# not a heading')\n```\n\n"
        "# Outro\n\nBye.\n"
    )

    def test_document_structure(self):
        tree = parse_document(self.DOC)
        self.assertEqual(tree.kind, "document")
        sections = find_all(tree, "section")
        self.assertEqual([s.title for s in sections], ["Intro", "Details", "Outro"])
        self.assertEqual([s.depth for s in sections], [1, 2, 1])

    def test_nesting(self):
        tree = parse_document(self.DOC)
        intro = next(s for s in find_all(tree, "section") if s.title == "Intro")
        self.assertIn("Details", [c.title for c in intro.children if c.kind == "section"])
        outro_parents = [
            n for n in walk(tree) if any(getattr(c, "title", "") == "Outro" for c in n.children)
        ]
        self.assertEqual(outro_parents[0].kind, "document")  # Outro is top-level

    def test_front_matter_code_and_list(self):
        tree = parse_document(self.DOC)
        self.assertEqual(len(find_all(tree, "front-matter")), 1)
        code = find_all(tree, "code-block")
        self.assertEqual(len(code), 1)
        self.assertEqual(code[0].language, "python")
        self.assertIn("# not a heading", code[0].text)
        items = find_all(tree, "list-item")
        self.assertEqual([i.text for i in items], ["first item", "buy milk"])

    def test_sentences_and_words_in_tree(self):
        tree = parse_document(self.DOC)
        sentences = find_all(tree, "sentence")
        texts = [s.text for s in sentences]
        self.assertIn("Hello world.", texts)
        self.assertIn("This is a test.", texts)
        self.assertGreater(len(find_all(tree, "word")), 5)

    def test_sexpr_and_dict(self):
        tree = parse_document("# T\n\nCats sleep.")
        sexpr = to_sexpr(tree)
        self.assertTrue(sexpr.startswith("(document"))
        self.assertIn('section:1 "T"', sexpr)
        self.assertIn("(S", sexpr)
        payload = to_dict(tree)
        self.assertEqual(payload["kind"], "document")
        self.assertTrue(payload["children"])

    def test_empty_document(self):
        tree = parse_document("")
        self.assertEqual(tree.children, [])


if __name__ == "__main__":
    unittest.main(verbosity=2)
