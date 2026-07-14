"""Syntax trees and grammatical structures for documents.

Parses a text/Markdown document into a full abstract syntax tree::

    document
    ├── front-matter?
    ├── section (title, depth)          <- reuses the chunker's heading detection
    │   ├── paragraph
    │   │   └── sentence
    │   │       └── phrase (NP/VP/PP/O) <- grammatical structures
    │   │           └── word (POS tag)  <- tagged leaf tokens
    │   ├── code-block (language)
    │   └── list-block / list-item
    └── ...

Grammar layer (heuristic, stdlib-only):

* :func:`split_sentences`  -- abbreviation-aware sentence segmentation.
* :func:`tag_tokens`       -- lightweight POS tagging (closed-class word
  lists + morphological suffix rules).
* :func:`chunk_phrases`    -- shallow phrase chunking into noun phrases (NP),
  verb phrases (VP), prepositional phrases (PP) and other (O) runs.

Utilities: :func:`walk`, :func:`find_all`, :func:`to_dict`, :func:`to_sexpr`.
"""

from __future__ import annotations

from dataclasses import dataclass, field, fields
from typing import ClassVar, Iterator
import re

from .text_structure_chunker import (
    _FENCE_RE,
    _detect_headings,
    _front_matter_end,
    _normalise,
)
from .tokenizer import Token, TokenType, tokenize

__all__ = [
    "Node",
    "DocumentNode",
    "FrontMatterNode",
    "SectionNode",
    "ParagraphNode",
    "CodeBlockNode",
    "ListBlockNode",
    "ListItemNode",
    "SentenceNode",
    "PhraseNode",
    "WordNode",
    "parse_document",
    "parse_sentence",
    "split_sentences",
    "tag_tokens",
    "chunk_phrases",
    "walk",
    "find_all",
    "to_dict",
    "to_sexpr",
]


# --- AST nodes ---------------------------------------------------------------


@dataclass
class Node:
    kind: ClassVar[str] = "node"
    start_line: int = 0
    end_line: int = 0
    children: list["Node"] = field(default_factory=list)


@dataclass
class DocumentNode(Node):
    kind: ClassVar[str] = "document"


@dataclass
class FrontMatterNode(Node):
    kind: ClassVar[str] = "front-matter"
    text: str = ""


@dataclass
class SectionNode(Node):
    kind: ClassVar[str] = "section"
    title: str = ""
    depth: int = 1
    style: str = "atx"


@dataclass
class ParagraphNode(Node):
    kind: ClassVar[str] = "paragraph"
    text: str = ""


@dataclass
class CodeBlockNode(Node):
    kind: ClassVar[str] = "code-block"
    language: str = ""
    text: str = ""


@dataclass
class ListBlockNode(Node):
    kind: ClassVar[str] = "list-block"
    ordered: bool = False


@dataclass
class ListItemNode(Node):
    kind: ClassVar[str] = "list-item"
    text: str = ""


@dataclass
class SentenceNode(Node):
    kind: ClassVar[str] = "sentence"
    text: str = ""


@dataclass
class PhraseNode(Node):
    kind: ClassVar[str] = "phrase"
    label: str = "O"  # NP | VP | PP | O


@dataclass
class WordNode(Node):
    kind: ClassVar[str] = "word"
    text: str = ""
    tag: str = "NOUN"
    token_type: str = "word"


# --- Tree utilities ----------------------------------------------------------


def walk(node: Node) -> Iterator[Node]:
    """Depth-first pre-order traversal."""

    yield node
    for child in node.children:
        yield from walk(child)


def find_all(node: Node, kind: str) -> list[Node]:
    return [n for n in walk(node) if n.kind == kind]


def to_dict(node: Node) -> dict:
    """JSON-serialisable representation of the tree."""

    payload: dict = {"kind": node.kind}
    for f in fields(node):
        if f.name == "children":
            continue
        value = getattr(node, f.name)
        if value not in (0, "", None) or f.name in ("depth",):
            payload[f.name] = value
    if node.children:
        payload["children"] = [to_dict(c) for c in node.children]
    return payload


def to_sexpr(node: Node) -> str:
    """Compact S-expression rendering of the tree (single line)."""

    if isinstance(node, WordNode):
        return f"({node.tag} {node.text})"
    if isinstance(node, PhraseNode):
        head = node.label
    elif isinstance(node, SentenceNode):
        head = "S"
    elif isinstance(node, ParagraphNode):
        head = "P"
    elif isinstance(node, SectionNode):
        head = f'section:{node.depth} "{node.title}"'
    elif isinstance(node, CodeBlockNode):
        return f"(code {node.language or '_'})"
    elif isinstance(node, FrontMatterNode):
        return "(front-matter)"
    elif isinstance(node, ListBlockNode):
        head = "list"
    elif isinstance(node, ListItemNode):
        head = "item"
    else:
        head = node.kind
    inner = " ".join(to_sexpr(c) for c in node.children)
    return f"({head} {inner})" if inner else f"({head})"


# --- Sentence segmentation ---------------------------------------------------

_ABBREVIATIONS = {
    "mr", "mrs", "ms", "dr", "prof", "sr", "jr", "st", "vs", "etc",
    "eg", "ie", "cf", "fig", "no", "al", "inc", "ltd", "co", "dept",
    "est", "approx", "min", "max", "sec", "vol", "pp", "ca",
}

_BOUNDARY_RE = re.compile(r"[.!?]+[\"')\]]*\s+")
_LAST_WORD_RE = re.compile(r"[A-Za-z0-9]+$")


def split_sentences(text: str) -> list[str]:
    """Split ``text`` into sentences, tolerating common abbreviations.

    Whitespace is collapsed first, so multi-line paragraphs segment cleanly.
    A candidate boundary (``.!?`` + space) is rejected when the preceding word
    is a known abbreviation or a single capital initial ("J. Smith"), or when
    the next character does not look like a sentence opener.
    """

    text = " ".join(text.split())
    if not text:
        return []

    sentences: list[str] = []
    start = 0
    for match in _BOUNDARY_RE.finditer(text):
        nxt = text[match.end() : match.end() + 1]
        if nxt and not (nxt.isupper() or nxt.isdigit() or nxt in "\"'(["):
            continue
        before = _LAST_WORD_RE.search(text[: match.start()])
        if before:
            word = before.group(0)
            if word.lower() in _ABBREVIATIONS:
                continue
            if len(word) == 1 and word.isupper():
                continue  # initials such as "J. Smith"
        sentence = text[start : match.end()].strip()
        if sentence:
            sentences.append(sentence)
        start = match.end()
    tail = text[start:].strip()
    if tail:
        sentences.append(tail)
    return sentences


# --- POS-lite tagging --------------------------------------------------------

_CLOSED_CLASS = {
    "DET": {
        "a", "an", "the", "this", "that", "these", "those",
        "each", "every", "some", "any", "no", "either", "neither",
    },
    "PRON": {
        "i", "you", "he", "she", "it", "we", "they", "me", "him", "her",
        "us", "them", "who", "whom", "mine", "yours", "hers", "ours",
        "theirs", "my", "your", "his", "its", "our", "their", "itself",
        "himself", "herself", "themselves",
    },
    "PREP": {
        "in", "on", "at", "by", "for", "with", "from", "into", "onto",
        "over", "under", "of", "to", "through", "during", "between",
        "among", "about", "against", "within", "without", "across",
        "behind", "beyond", "near", "toward", "towards", "upon",
    },
    "CONJ": {
        "and", "or", "but", "nor", "so", "yet", "because", "although",
        "while", "if", "when", "than", "as", "unless", "whereas",
    },
    "MODAL": {"can", "could", "may", "might", "must", "shall", "should", "will", "would"},
}

_AUXILIARIES = {
    "is", "are", "was", "were", "be", "been", "being", "am",
    "has", "have", "had", "do", "does", "did",
}

_ADJ_SUFFIXES = ("ous", "ful", "ive", "able", "ible", "ical", "ish", "less", "ary")
_NOUN_SUFFIXES = (
    "tion", "sion", "ment", "ness", "ity", "ance", "ence", "ship", "ism", "ology", "ure",
)

_NOMINAL = {"NOUN", "PROPN", "PRON"}


def _tag_word(token: Token, first_in_sentence: bool) -> str:
    if token.type is TokenType.NUMBER:
        return "NUM"
    if token.type is not TokenType.WORD:
        return "PUNCT"
    low = token.value.lower()
    for tag, vocabulary in _CLOSED_CLASS.items():
        if low in vocabulary:
            return tag
    if low in _AUXILIARIES:
        return "VERB"
    if token.value[0].isupper() and not first_in_sentence:
        return "PROPN"
    if low.endswith("ly") and len(low) > 3:
        return "ADV"
    if len(low) >= 5 and low.endswith(("ing", "ed")):
        return "VERB"
    if low.endswith(_ADJ_SUFFIXES):
        return "ADJ"
    if low.endswith(_NOUN_SUFFIXES):
        return "NOUN"
    return "NOUN"


def tag_tokens(tokens: list[Token]) -> list[tuple[Token, str]]:
    """POS-tag a token sequence: ``[(token, tag), ...]``.

    A contextual pass follows the per-token rules: after a modal (skipping
    adverbs), an open-class word is a base-form verb ("should respond"),
    which the suffix rules alone cannot see.
    """

    tagged = [(tok, _tag_word(tok, idx == 0)) for idx, tok in enumerate(tokens)]
    expect_verb = False
    for idx, (tok, tag) in enumerate(tagged):
        if tag == "MODAL":
            expect_verb = True
            continue
        if expect_verb:
            if tag == "ADV":
                continue
            if tag == "NOUN" and tok.value.isalpha():
                tagged[idx] = (tok, "VERB")
            expect_verb = False
    return tagged


# --- Phrase chunking ---------------------------------------------------------


def _try_np(tags: list[str], i: int) -> int | None:
    """Return the end index (exclusive) of a noun phrase starting at ``i``."""

    j = i
    if j < len(tags) and tags[j] == "DET":
        j += 1
    while j < len(tags) and tags[j] in ("ADJ", "NUM"):
        j += 1
    k = j
    while k < len(tags) and tags[k] in _NOMINAL:
        k += 1
    return k if k > j else None


def _try_vp(tags: list[str], i: int) -> int | None:
    """Return the end index (exclusive) of a verb phrase starting at ``i``."""

    j = i
    if j < len(tags) and tags[j] == "MODAL":
        j += 1
    while j < len(tags) and tags[j] == "ADV":
        j += 1
    k = j
    while k < len(tags) and tags[k] == "VERB":
        k += 1
    if k == j:
        return None
    while k < len(tags) and tags[k] == "ADV":
        k += 1
    return k


def _word_nodes(tagged: list[tuple[Token, str]], start: int, end: int) -> list[WordNode]:
    return [
        WordNode(
            text=tok.value,
            tag=tag,
            token_type=tok.type.value,
            start_line=tok.line,
            end_line=tok.line,
        )
        for tok, tag in tagged[start:end]
    ]


def chunk_phrases(tagged: list[tuple[Token, str]]) -> list[PhraseNode]:
    """Group tagged tokens into shallow NP / VP / PP / O phrases."""

    tags = [tag for _, tag in tagged]
    phrases: list[PhraseNode] = []
    i = 0

    def starts_phrase(pos: int) -> bool:
        if pos >= len(tags):
            return False
        if tags[pos] == "PREP" and _try_np(tags, pos + 1):
            return True
        return bool(_try_np(tags, pos) or _try_vp(tags, pos))

    while i < len(tags):
        if tags[i] == "PREP" and (np_end := _try_np(tags, i + 1)):
            np = PhraseNode(label="NP", children=list(_word_nodes(tagged, i + 1, np_end)))
            prep = _word_nodes(tagged, i, i + 1)
            phrases.append(PhraseNode(label="PP", children=[*prep, np]))
            i = np_end
            continue
        if np_end := _try_np(tags, i):
            phrases.append(PhraseNode(label="NP", children=list(_word_nodes(tagged, i, np_end))))
            i = np_end
            continue
        if vp_end := _try_vp(tags, i):
            phrases.append(PhraseNode(label="VP", children=list(_word_nodes(tagged, i, vp_end))))
            i = vp_end
            continue
        # Collect a run of tokens that do not start any phrase.
        j = i + 1
        while j < len(tags) and not starts_phrase(j):
            j += 1
        phrases.append(PhraseNode(label="O", children=list(_word_nodes(tagged, i, j))))
        i = j

    return phrases


def parse_sentence(text: str, *, line: int = 0) -> SentenceNode:
    """Parse one sentence into a :class:`SentenceNode` of tagged phrases."""

    tokens = tokenize(text)
    node = SentenceNode(text=text, start_line=line, end_line=line)
    node.children = list(chunk_phrases(tag_tokens(tokens)))
    return node


def _sentence_nodes(text: str, line: int) -> list[SentenceNode]:
    return [parse_sentence(s, line=line) for s in split_sentences(text)]


# --- Document parsing --------------------------------------------------------

_LIST_RE = re.compile(r"^ {0,3}(?:([-*+])|(\d{1,3}[.)]))\s+(.+)$")


def parse_document(text: str) -> DocumentNode:
    """Parse a document into its full syntax tree.

    Heading detection is delegated to the structural chunking engine so the
    AST and the chunker always agree on the document outline.
    """

    document = DocumentNode(start_line=1, end_line=1)
    if not text:
        return document

    lines = _normalise(text)
    document.end_line = len(lines)
    front_end = _front_matter_end(lines)
    headings = {
        h.line: h
        for h in _detect_headings(
            lines, detect_atx=True, detect_setext=True, detect_numbered=True
        )
    }

    stack: list[SectionNode] = []

    def container() -> Node:
        return stack[-1] if stack else document

    def close_sections(depth: int, upto_line: int) -> None:
        while stack and stack[-1].depth >= depth:
            stack.pop().end_line = upto_line

    i = 0
    if front_end:
        document.children.append(
            FrontMatterNode(
                text="\n".join(lines[:front_end]), start_line=1, end_line=front_end
            )
        )
        i = front_end

    while i < len(lines):
        line_no = i + 1
        raw = lines[i]

        heading = headings.get(line_no)
        if heading is not None:
            close_sections(heading.depth, line_no - 1)
            section = SectionNode(
                title=heading.title,
                depth=heading.depth,
                style=heading.style,
                start_line=line_no,
                end_line=line_no,
            )
            container().children.append(section)
            stack.append(section)
            i += 2 if heading.style == "setext" else 1
            continue

        if not raw.strip():
            i += 1
            continue

        fence = _FENCE_RE.match(raw)
        if fence:
            language = raw[fence.end() :].strip()
            j = i + 1
            while j < len(lines) and not _FENCE_RE.match(lines[j]):
                j += 1
            container().children.append(
                CodeBlockNode(
                    language=language,
                    text="\n".join(lines[i + 1 : j]),
                    start_line=line_no,
                    end_line=min(j + 1, len(lines)),
                )
            )
            i = j + 1
            continue

        if match := _LIST_RE.match(raw):
            block = ListBlockNode(ordered=bool(match.group(2)), start_line=line_no)
            j = i
            while j < len(lines) and (item := _LIST_RE.match(lines[j])):
                item_text = item.group(3).strip()
                node = ListItemNode(text=item_text, start_line=j + 1, end_line=j + 1)
                node.children = list(_sentence_nodes(item_text, j + 1))
                block.children.append(node)
                j += 1
            block.end_line = j
            container().children.append(block)
            i = j
            continue

        # Paragraph: consume until a blank line or another block opener.
        j = i
        while j < len(lines):
            candidate = lines[j]
            if (
                not candidate.strip()
                or (j + 1) in headings
                or _FENCE_RE.match(candidate)
                or _LIST_RE.match(candidate)
            ):
                break
            j += 1
        paragraph_text = " ".join(part.strip() for part in lines[i:j])
        paragraph = ParagraphNode(text=paragraph_text, start_line=line_no, end_line=j)
        paragraph.children = list(_sentence_nodes(paragraph_text, line_no))
        container().children.append(paragraph)
        i = j

    close_sections(0, len(lines))
    return document
