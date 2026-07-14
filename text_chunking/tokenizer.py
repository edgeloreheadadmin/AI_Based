"""Lexical tokenization ("parsing tokenization") for the intelligence suite.

Turns raw text into a stream of typed :class:`Token` objects with exact
line/column/offset positions.  Every other layer -- the syntax tree, the
n-gram language model, the completion engine and the semantic index -- is
built on top of this single tokenizer so they all agree on what a "token" is.

Stdlib only; no external dependencies.
"""

from __future__ import annotations

from dataclasses import dataclass
from enum import Enum
import re

__all__ = ["TokenType", "Token", "tokenize", "words", "lexical_tokens"]


class TokenType(str, Enum):
    WORD = "word"
    NUMBER = "number"
    PUNCTUATION = "punctuation"
    SYMBOL = "symbol"
    SPACE = "space"
    NEWLINE = "newline"


# Every character of the input matches exactly one alternative, so the token
# stream is a lossless partition of the text.  Order matters: WORD outranks
# PUNCTUATION so hyphenated/apostrophised words ("well-known", "don't") stay
# whole, and NUMBER outranks WORD so "3.14" and "1.2.3" stay whole.
_TOKEN_RE = re.compile(
    r"""(?P<NEWLINE>\r\n|\r|\n)
      | (?P<SPACE>[^\S\r\n]+)
      | (?P<NUMBER>\d+(?:\.\d+)*)
      | (?P<WORD>[A-Za-z_]+(?:['’-][A-Za-z0-9_]+)*)
      | (?P<PUNCTUATION>[.,;:!?()\[\]{}"'`~*#>=|\\/<^&%$@+-])
      | (?P<SYMBOL>\S)
    """,
    re.VERBOSE,
)


@dataclass(frozen=True)
class Token:
    type: TokenType
    value: str
    line: int  # 1-based
    column: int  # 1-based
    offset: int  # 0-based character offset


def tokenize(text: str, *, include_whitespace: bool = False) -> list[Token]:
    """Tokenize ``text`` into typed tokens with positions.

    Whitespace (``SPACE``/``NEWLINE``) tokens are omitted unless
    ``include_whitespace`` is set; positions are tracked either way.
    """

    tokens: list[Token] = []
    line, column = 1, 1
    for match in _TOKEN_RE.finditer(text):
        kind = TokenType[match.lastgroup]  # group names mirror member names
        value = match.group()
        if include_whitespace or kind not in (TokenType.SPACE, TokenType.NEWLINE):
            tokens.append(
                Token(type=kind, value=value, line=line, column=column, offset=match.start())
            )
        if kind is TokenType.NEWLINE:
            line += 1
            column = 1
        else:
            column += len(value)
    return tokens


def words(text: str, *, lowercase: bool = True) -> list[str]:
    """The WORD tokens of ``text`` as plain strings (lowercased by default)."""

    return [
        t.value.lower() if lowercase else t.value
        for t in tokenize(text)
        if t.type is TokenType.WORD
    ]


def lexical_tokens(text: str) -> list[str]:
    """Word, number and punctuation values -- the language-model alphabet.

    Words are lowercased so the n-gram model is case-insensitive; numbers and
    punctuation are kept verbatim.
    """

    out: list[str] = []
    for t in tokenize(text):
        if t.type is TokenType.WORD:
            out.append(t.value.lower())
        elif t.type in (TokenType.NUMBER, TokenType.PUNCTUATION):
            out.append(t.value)
    return out
