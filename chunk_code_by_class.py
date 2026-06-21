"""Chunk a single source document into one chunk per class definition.

This module implements the ``chunk_code_by_class`` tool for the codex agent
framework. Given a single piece of code -- supplied inline or read from a single
file -- it locates every class-like definition (``class``/``struct``/
``interface``/``record`` by default, configurable) and returns one chunk per
definition containing that class's full source span.

Two boundary-detection strategies are supported:

``indent``
    For indentation-delimited languages (Python, YAML). A class ends when a
    later significant line is indented at or below the class's own indentation.
``brace``
    For brace-delimited languages (C/C++, C#, Java, JavaScript, Go, Rust, ...).
    A class ends at the ``}`` that closes the ``{`` opening its body.

``auto`` (the default) picks a strategy from the language hint or file
extension. A shared, comment- and string-aware line scanner keeps keywords and
braces that appear inside comments or string/`docstring` literals from being
mistaken for real declarations.

The tool always operates on a *single* document; there is no directory walking
or multi-file batching by design.
"""

from __future__ import annotations

import re
from dataclasses import dataclass
from pathlib import Path
from typing import TYPE_CHECKING, ClassVar

from pydantic import BaseModel, Field

from codex.core.tools.base import (
    BaseTool,
    BaseToolConfig,
    BaseToolState,
    ToolError,
    ToolPermission,
)
from codex.core.tools.ui import ToolCallDisplay, ToolResultDisplay, ToolUIData

if TYPE_CHECKING:
    from codex.core.types import ToolCallEvent, ToolResultEvent


# --------------------------------------------------------------------------- #
# Language detection and detection vocabulary
# --------------------------------------------------------------------------- #

INDENT_EXTS = {".py", ".pyi", ".pyw", ".pyx", ".yml", ".yaml"}
INDENT_LANGS = {"python", "py", "yaml", "yml"}
DEFAULT_CLASS_KEYWORDS = ("class", "struct", "interface", "record")
NAME_RE = r"[A-Za-z_][A-Za-z0-9_]*"


@dataclass
class _ScanState:
    """Carried across lines so multi-line strings and comments are tracked."""

    in_block_comment: bool = False
    in_string: str | None = None  # active delimiter: ' " ` or """ '''


@dataclass
class _ClassEntry:
    """A class declaration that is currently open while scanning."""

    name: str
    kind: str
    start_line: int
    indent: int
    depth: int
    brace_depth: int | None = None  # brace depth at which the body opened


@dataclass
class _Span:
    """A resolved class chunk as line offsets into the source (1-based)."""

    name: str
    kind: str
    start_line: int
    end_line: int
    depth: int


# --------------------------------------------------------------------------- #
# Pure scanning / detection helpers (no framework or I/O dependency)
# --------------------------------------------------------------------------- #


def build_class_pattern(keywords: list[str]) -> re.Pattern[str]:
    """Compile a class-detection regex for the given *keywords*.

    Building the pattern from the resolved keywords (rather than a hardcoded
    list) is what makes the ``class_keywords`` override actually take effect for
    custom keywords such as ``enum``, ``trait`` or ``object``.
    """
    alternation = "|".join(re.escape(keyword) for keyword in keywords)
    return re.compile(rf"\b(?:{alternation})\b\s+({NAME_RE})")


def detect_class(
    line: str, pattern: re.Pattern[str], keywords: set[str]
) -> tuple[str, str] | None:
    """Return ``(kind, name)`` for the first class declaration on *line*."""
    match = pattern.search(line)
    if not match:
        return None
    # group(0) starts with the keyword; recover it for the reported kind.
    kind = match.group(0).split(None, 1)[0].lower()
    if kind not in keywords:
        return None
    return kind, match.group(1)


def scan_line(
    line: str, state: _ScanState, *, hash_comments: bool
) -> tuple[str, _ScanState]:
    """Strip comments and string literals from *line*, updating *state*.

    Returns the code-only remainder of the line. Block comments and string
    literals (including Python ``\"\"\"``/``'''`` docstrings and back-tick
    template literals) are tracked across lines via *state*, so keywords and
    braces inside them never reach the detectors.
    """
    out: list[str] = []
    i = 0
    length = len(line)
    while i < length:
        ch = line[i]
        pair = line[i : i + 2]
        triple = line[i : i + 3]

        if state.in_block_comment:
            if pair == "*/":
                state.in_block_comment = False
                i += 2
            else:
                i += 1
            continue

        if state.in_string is not None:
            delimiter = state.in_string
            if ch == "\\":  # escaped character inside a string literal
                i += 2
                continue
            if len(delimiter) == 3:
                if line[i : i + 3] == delimiter:
                    state.in_string = None
                    i += 3
                    continue
                i += 1
                continue
            if ch == delimiter:
                state.in_string = None
            i += 1
            continue

        if triple in ('"""', "'''"):
            state.in_string = triple
            i += 3
            continue
        if pair == "/*":
            state.in_block_comment = True
            i += 2
            continue
        if pair == "//":
            break
        if hash_comments and ch == "#":
            break
        if ch in ("'", '"', "`"):
            state.in_string = ch
            i += 1
            continue

        out.append(ch)
        i += 1

    return "".join(out), state


def count_indent(line: str) -> int:
    """Width of *line*'s leading whitespace (tabs count as four columns)."""
    count = 0
    for ch in line:
        if ch == " ":
            count += 1
        elif ch == "\t":
            count += 4
        else:
            break
    return count


def _finalize(
    entry: _ClassEntry,
    lines: list[str],
    end_line: int,
    include_nested: bool,
) -> _Span | None:
    """Close an open class into a :class:`_Span`, or drop it if filtered out."""
    if not include_nested and entry.depth != 1:
        return None
    end_line = max(end_line, entry.start_line)
    # Trim trailing blank lines so end_line points at real content.
    while end_line > entry.start_line and not lines[end_line - 1].strip():
        end_line -= 1
    return _Span(entry.name, entry.kind, entry.start_line, end_line, entry.depth)


def find_classes_indent(
    content: str,
    pattern: re.Pattern[str],
    keywords: set[str],
    include_nested: bool,
) -> list[_Span]:
    """Locate classes in an indentation-delimited document."""
    lines = content.splitlines()
    state = _ScanState()
    stack: list[_ClassEntry] = []
    spans: list[_Span | None] = []

    for idx, line in enumerate(lines, start=1):
        sanitized, state = scan_line(line, state, hash_comments=True)
        stripped = sanitized.strip()
        if not stripped:
            continue

        indent = count_indent(line)
        while stack and indent <= stack[-1].indent:
            spans.append(_finalize(stack.pop(), lines, idx - 1, include_nested))

        detected = detect_class(stripped, pattern, keywords)
        if detected:
            kind, name = detected
            stack.append(
                _ClassEntry(name, kind, idx, indent, depth=len(stack) + 1)
            )

    while stack:
        spans.append(_finalize(stack.pop(), lines, len(lines), include_nested))

    return [span for span in spans if span is not None]


def find_classes_brace(
    content: str,
    pattern: re.Pattern[str],
    keywords: set[str],
    include_nested: bool,
) -> list[_Span]:
    """Locate classes in a brace-delimited document."""
    lines = content.splitlines()
    state = _ScanState()
    stack: list[_ClassEntry] = []
    pending: _ClassEntry | None = None
    brace_depth = 0
    spans: list[_Span | None] = []

    for idx, line in enumerate(lines, start=1):
        sanitized, state = scan_line(line, state, hash_comments=False)
        stripped = sanitized.strip()

        if stripped and pending is None:
            detected = detect_class(stripped, pattern, keywords)
            if detected:
                kind, name = detected
                pending = _ClassEntry(name, kind, idx, 0, depth=len(stack) + 1)

        for ch in sanitized:
            if ch == "{":
                brace_depth += 1
                if pending is not None:
                    pending.brace_depth = brace_depth
                    stack.append(pending)
                    pending = None
            elif ch == "}":
                brace_depth = max(brace_depth - 1, 0)
                while (
                    stack
                    and stack[-1].brace_depth is not None
                    and brace_depth < stack[-1].brace_depth
                ):
                    spans.append(
                        _finalize(stack.pop(), lines, idx, include_nested)
                    )
            elif ch == ";" and pending is not None:
                # e.g. a forward declaration `class Foo;` with no body.
                pending = None

    while stack:
        spans.append(_finalize(stack.pop(), lines, len(lines), include_nested))

    return [span for span in spans if span is not None]


# --------------------------------------------------------------------------- #
# Tool definition
# --------------------------------------------------------------------------- #


class ChunkCodeByClassConfig(BaseToolConfig):
    permission: ToolPermission = ToolPermission.ALWAYS
    max_input_bytes: int = Field(
        default=5_000_000, description="Maximum input size in bytes."
    )
    max_chunk_bytes: int = Field(
        default=500_000, description="Maximum class chunk size in bytes."
    )
    max_chunks: int = Field(
        default=200, description="Maximum number of class chunks to return."
    )
    default_mode: str = Field(default="auto", description="auto, indent, or brace.")
    include_nested: bool = Field(
        default=True, description="Include nested classes in results."
    )
    default_class_keywords: list[str] = Field(
        default=list(DEFAULT_CLASS_KEYWORDS),
        description="Keywords treated as class declarations.",
    )


class ChunkCodeByClassState(BaseToolState):
    pass


class ChunkCodeByClassArgs(BaseModel):
    content: str | None = Field(default=None, description="Raw code to chunk.")
    path: str | None = Field(default=None, description="Path to a single code file.")
    language: str | None = Field(default=None, description="Language hint.")
    mode: str | None = Field(default=None, description="auto, indent, or brace.")
    include_nested: bool | None = Field(
        default=None, description="Include nested classes."
    )
    class_keywords: list[str] | None = Field(
        default=None, description="Override class keywords to detect."
    )
    max_chunks: int | None = Field(
        default=None, description="Override the configured max chunks limit."
    )


class ClassChunk(BaseModel):
    index: int
    name: str
    kind: str
    depth: int
    start_line: int
    end_line: int
    content: str


class ChunkCodeByClassResult(BaseModel):
    mode: str
    include_nested: bool
    chunks: list[ClassChunk]
    count: int
    truncated: bool


class ChunkCodeByClass(
    BaseTool[
        ChunkCodeByClassArgs,
        ChunkCodeByClassResult,
        ChunkCodeByClassConfig,
        ChunkCodeByClassState,
    ],
    ToolUIData[ChunkCodeByClassArgs, ChunkCodeByClassResult],
):
    description: ClassVar[str] = "Chunk a single script by each class definition."

    async def run(self, args: ChunkCodeByClassArgs) -> ChunkCodeByClassResult:
        content, source_path = self._load_content(args)
        mode = self._resolve_mode(args, source_path)
        include_nested = (
            args.include_nested
            if args.include_nested is not None
            else self.config.include_nested
        )

        if not content:
            return ChunkCodeByClassResult(
                mode=mode,
                include_nested=include_nested,
                chunks=[],
                count=0,
                truncated=False,
            )

        keywords = self._resolve_keywords(args)
        pattern = build_class_pattern(keywords)
        keyword_set = set(keywords)

        max_chunks = (
            args.max_chunks if args.max_chunks is not None else self.config.max_chunks
        )
        if max_chunks <= 0:
            raise ToolError("max_chunks must be a positive integer.")

        if mode == "indent":
            spans = find_classes_indent(content, pattern, keyword_set, include_nested)
        elif mode == "brace":
            spans = find_classes_brace(content, pattern, keyword_set, include_nested)
        else:
            raise ToolError("mode must be auto, indent, or brace.")

        spans.sort(key=lambda span: (span.start_line, span.end_line))

        truncated = len(spans) > max_chunks
        if truncated:
            spans = spans[:max_chunks]

        lines = content.splitlines()
        chunks = [
            self._build_chunk(index, span, lines)
            for index, span in enumerate(spans, start=1)
        ]
        self._validate_chunk_sizes(chunks)

        return ChunkCodeByClassResult(
            mode=mode,
            include_nested=include_nested,
            chunks=chunks,
            count=len(chunks),
            truncated=truncated,
        )

    # ----------------------------------------------------------------- #
    # Input loading
    # ----------------------------------------------------------------- #

    def _load_content(self, args: ChunkCodeByClassArgs) -> tuple[str, Path | None]:
        if args.content is not None and args.path is not None:
            raise ToolError("Provide either content or path, not both.")
        if args.content is None and args.path is None:
            raise ToolError("Provide content or path.")

        if args.content is not None:
            self._validate_input_size(len(args.content.encode("utf-8")))
            return args.content, None

        path = self._resolve_path(args.path or "")
        try:
            self._validate_input_size(path.stat().st_size)
            text = path.read_text("utf-8", errors="ignore")
        except OSError as exc:
            raise ToolError(f"Could not read file: {exc}") from exc
        return text, path

    def _validate_input_size(self, size: int) -> None:
        if size > self.config.max_input_bytes:
            raise ToolError(
                f"Input is {size} bytes, which exceeds max_input_bytes "
                f"({self.config.max_input_bytes})."
            )

    def _resolve_path(self, raw_path: str) -> Path:
        if not raw_path.strip():
            raise ToolError("Path cannot be empty.")

        path = Path(raw_path).expanduser()
        if not path.is_absolute():
            path = self.config.effective_workdir / path

        try:
            resolved = path.resolve()
        except (OSError, ValueError) as exc:
            raise ToolError(f"Failed to resolve path: {exc}") from exc

        if not resolved.exists():
            raise ToolError(f"Path not found: {resolved}")
        if resolved.is_dir():
            raise ToolError(f"Path is a directory, not a file: {resolved}")
        return resolved

    # ----------------------------------------------------------------- #
    # Argument resolution
    # ----------------------------------------------------------------- #

    def _resolve_mode(
        self, args: ChunkCodeByClassArgs, source_path: Path | None
    ) -> str:
        mode = (args.mode or self.config.default_mode).strip().lower()
        if mode != "auto":
            return mode

        language = (args.language or "").strip().lower()
        if language in INDENT_LANGS:
            return "indent"
        if source_path and source_path.suffix.lower() in INDENT_EXTS:
            return "indent"
        return "brace"

    def _resolve_keywords(self, args: ChunkCodeByClassArgs) -> list[str]:
        raw = (
            args.class_keywords
            if args.class_keywords is not None
            else self.config.default_class_keywords
        )
        keywords = {value.strip().lower() for value in raw if value and value.strip()}
        if not keywords:
            raise ToolError("class_keywords must not be empty.")
        return sorted(keywords)

    # ----------------------------------------------------------------- #
    # Chunk assembly
    # ----------------------------------------------------------------- #

    def _build_chunk(self, index: int, span: _Span, lines: list[str]) -> ClassChunk:
        body = "\n".join(lines[span.start_line - 1 : span.end_line])
        return ClassChunk(
            index=index,
            name=span.name,
            kind=span.kind,
            depth=span.depth,
            start_line=span.start_line,
            end_line=span.end_line,
            content=body,
        )

    def _validate_chunk_sizes(self, chunks: list[ClassChunk]) -> None:
        max_bytes = self.config.max_chunk_bytes
        for chunk in chunks:
            size = len(chunk.content.encode("utf-8"))
            if size > max_bytes:
                raise ToolError(
                    f"Chunk '{chunk.name}' exceeds max_chunk_bytes "
                    f"({size} > {max_bytes})."
                )

    # ----------------------------------------------------------------- #
    # UI display
    # ----------------------------------------------------------------- #

    @classmethod
    def get_call_display(cls, event: ToolCallEvent) -> ToolCallDisplay:
        if not isinstance(event.args, ChunkCodeByClassArgs):
            return ToolCallDisplay(summary="chunk_code_by_class")

        return ToolCallDisplay(
            summary="chunk_code_by_class",
            details={
                "path": event.args.path,
                "language": event.args.language,
                "mode": event.args.mode,
                "include_nested": event.args.include_nested,
                "class_keywords": event.args.class_keywords,
                "max_chunks": event.args.max_chunks,
            },
        )

    @classmethod
    def get_result_display(cls, event: ToolResultEvent) -> ToolResultDisplay:
        if not isinstance(event.result, ChunkCodeByClassResult):
            return ToolResultDisplay(
                success=False,
                message=event.error or event.skip_reason or "No result",
            )

        result = event.result
        warnings: list[str] = []
        if result.truncated:
            warnings.append("Chunk list truncated by max_chunks limit")

        return ToolResultDisplay(
            success=True,
            message=f"Found {result.count} class chunk(s)",
            warnings=warnings,
            details={
                "mode": result.mode,
                "include_nested": result.include_nested,
                "count": result.count,
                "truncated": result.truncated,
                "chunks": result.chunks,
            },
        )

    @classmethod
    def get_status_text(cls) -> str:
        return "Chunking classes"
