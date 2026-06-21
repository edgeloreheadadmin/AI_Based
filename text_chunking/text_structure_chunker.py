"""Structure-aware text chunking engine.

The goal of this module is the one the original ``chunk_text_structures`` tool
set out to achieve: take a plain-text or Markdown-ish document and split it into
**hierarchical, heading-based chunks** that are useful for downstream work such
as retrieval-augmented generation, summarisation, or document navigation.

Design principles (and the ways this differs from the original script):

* **Complete coverage.** Every line of the input belongs to exactly one chunk.
  The original silently dropped any preamble that appeared before the first
  top-level heading; here that text becomes an explicit ``preamble`` chunk.
* **Non-overlapping sections.** Each chunk is a top-level section: a root
  heading plus everything beneath it, up to the next root heading.
* **Robust heading detection.** ATX (``# Title``), setext (``Title`` / ``====``)
  and numbered (``1.2 Title``) headings are recognised, code fences and YAML
  front matter are ignored, and the numbered-heading matcher is conservative so
  ordinary numeric lines ("42 apples", "1. buy milk") are not misread as
  structure.
* **Size aware.** A section larger than ``max_chunk_bytes`` is split into
  ordered parts instead of raising an error, so a single huge section can never
  break the whole operation.

This engine has **no third-party dependencies** so it can be tested and reused
in isolation. The :class:`ChunkTextStructures` tool is a thin adapter on top of
it.
"""

from __future__ import annotations

from dataclasses import dataclass, field
import re

__all__ = [
    "Heading",
    "Chunk",
    "ChunkingResult",
    "chunk_text",
    "DEFAULT_MAX_CHUNKS",
    "DEFAULT_MAX_CHUNK_BYTES",
]

DEFAULT_MAX_CHUNKS = 200
DEFAULT_MAX_CHUNK_BYTES = 500_000

# --- Heading / structure patterns -------------------------------------------

# ATX heading: up to three leading spaces, 1-6 '#', a required space, a title,
# and an optional closing run of '#'.  Requiring the space means "#hashtag" is
# not mistaken for a heading (matching CommonMark).
_ATX_RE = re.compile(r"^ {0,3}(#{1,6})\s+(.+?)\s*#*\s*$")

# Setext underline: a line made up solely of '=' (h1) or '-' (h2).
_SETEXT_RE = re.compile(r"^ {0,3}(=+|-+)\s*$")

# Numbered heading: "1", "1.2", "1.2.3" optionally followed by '.' or ')'.
# Acceptance is tightened in ``_match_numbered`` to avoid false positives.
_NUMBERED_RE = re.compile(r"^ {0,3}(\d{1,3}(?:\.\d{1,3}){0,5})([.)])?\s+(\S.*)$")

# Fenced code block delimiter: 3+ backticks or tildes.
_FENCE_RE = re.compile(r"^ {0,3}(`{3,}|~{3,})")

# Front-matter delimiters (only honoured at the very top of the document).
_FRONT_MATTER_FENCE = "---"
_FRONT_MATTER_CLOSE = ("---", "...")
_FRONT_MATTER_MAX_LINES = 100


# --- Public data structures -------------------------------------------------


@dataclass
class Heading:
    """A single detected heading and its place in the structure tree."""

    index: int  # 1-based, stable order by line; only set for emitted headings
    title: str
    depth: int  # absolute depth (1 = shallowest level present is not assumed)
    relative_depth: int  # depth normalised so the document root is 1
    line: int  # 1-based line number where the heading text lives
    end_line: int  # last line covered by this heading's subsection
    style: str  # "atx" | "setext" | "numbered"
    parent_index: int | None = None  # 1-based index of nearest emitted ancestor
    chunk_index: int | None = None  # 1-based index of the chunk it belongs to


@dataclass
class Chunk:
    """A contiguous, structure-aligned slice of the document."""

    index: int  # 1-based position in the result
    kind: str  # "section" | "preamble" | "document"
    heading_titles: list[str]
    heading_count: int
    start_line: int
    end_line: int
    line_span: int
    max_depth: int  # deepest relative heading level inside the chunk (root = 1)
    density: float  # headings per line
    density_per_100_lines: float  # headings per 100 lines
    part: int = 1  # 1-based part number when an oversized section is split
    part_count: int = 1  # total parts the originating section was split into
    content: str = ""


@dataclass
class ChunkingResult:
    chunks: list[Chunk] = field(default_factory=list)
    headings: list[Heading] = field(default_factory=list)
    count: int = 0
    truncated: bool = False
    root_depth: int | None = None


# --- Internal working structures --------------------------------------------


@dataclass
class _RawHeading:
    title: str
    depth: int
    line: int  # 1-based
    style: str
    end_line: int = 0
    parent: int | None = None  # index into the raw heading list


@dataclass
class _Section:
    kind: str
    start_line: int  # 1-based, inclusive
    end_line: int  # 1-based, inclusive
    heading_idxs: list[int]  # indices into the raw heading list (listed only)


# --- Heading detection ------------------------------------------------------


def _normalise(text: str) -> list[str]:
    """Split into lines with stable 1-based numbering and uniform newlines."""

    return text.replace("\r\n", "\n").replace("\r", "\n").split("\n")


def _front_matter_end(lines: list[str]) -> int:
    """Return the (1-based) last line of a leading YAML front-matter block.

    Returns 0 when the document does not start with a front-matter block.  The
    block is only recognised at the very top of the file so a stray ``---`` in
    the body is never treated as front matter.
    """

    if not lines or lines[0].strip() != _FRONT_MATTER_FENCE:
        return 0
    limit = min(len(lines), _FRONT_MATTER_MAX_LINES)
    for idx in range(1, limit):
        if lines[idx].strip() in _FRONT_MATTER_CLOSE:
            return idx + 1  # 1-based, inclusive of the closing fence
    return 0


def _mark_skip_lines(lines: list[str], front_matter_end: int) -> list[bool]:
    """Flag lines that must not be scanned for headings (fences, front matter)."""

    skip = [False] * len(lines)
    for idx in range(front_matter_end):
        skip[idx] = True

    in_fence = False
    fence_char: str | None = None
    for idx in range(front_matter_end, len(lines)):
        match = _FENCE_RE.match(lines[idx])
        if match:
            skip[idx] = True
            marker = match.group(1)[0]
            if not in_fence:
                in_fence, fence_char = True, marker
            elif fence_char == marker:
                in_fence, fence_char = False, None
            continue
        if in_fence:
            skip[idx] = True
    return skip


def _match_numbered(line: str) -> tuple[int, str] | None:
    """Return ``(depth, title)`` for a numbered heading, else ``None``.

    A bare ``"1 apple"`` is rejected; structure markers need either a multi-part
    number (``1.2``) or a trailing separator (``1.`` / ``1)``).  Single-level
    numbers additionally require a title that does not start lower-case, which
    filters most ordered to-do lists ("1. buy milk") while keeping section
    headings ("1. Introduction").
    """

    match = _NUMBERED_RE.match(line)
    if not match:
        return None
    number, separator, title = match.group(1), match.group(2), match.group(3)
    title = title.strip()
    if not title or not any(ch.isalpha() for ch in title):
        return None
    has_dot = "." in number
    if not has_dot and not separator:
        return None
    if not has_dot and title[:1].islower():
        return None
    depth = number.count(".") + 1
    return depth, title


def _detect_setext(lines: list[str], skip: list[bool]) -> dict[int, int]:
    """Map a 0-based title-line index to its depth for setext headings.

    The line above an ``===``/``---`` underline is treated as a heading only
    when it is a single-line paragraph (the line before it is blank, a heading,
    or the start of the document), which avoids turning the tail of an ordinary
    paragraph or a thematic break into a heading.
    """

    titles: dict[int, int] = {}
    for idx in range(1, len(lines)):
        if skip[idx]:
            continue
        underline = _SETEXT_RE.match(lines[idx])
        if not underline:
            continue
        title_idx = idx - 1
        if skip[title_idx]:
            continue
        if not lines[title_idx].strip():
            continue
        # Require the title to be a single-line paragraph.
        above = lines[title_idx - 1].strip() if title_idx - 1 >= 0 else ""
        if above and not (
            _ATX_RE.match(lines[title_idx - 1]) or _SETEXT_RE.match(lines[title_idx - 1])
        ):
            continue
        titles[title_idx] = 1 if underline.group(1).startswith("=") else 2
    return titles


def _detect_headings(
    lines: list[str],
    *,
    detect_atx: bool,
    detect_setext: bool,
    detect_numbered: bool,
) -> list[_RawHeading]:
    front_matter_end = _front_matter_end(lines)
    skip = _mark_skip_lines(lines, front_matter_end)

    setext_titles = _detect_setext(lines, skip) if detect_setext else {}
    setext_title_lines = set(setext_titles)

    headings: list[_RawHeading] = []
    for idx, raw in enumerate(lines):
        if skip[idx]:
            continue

        if idx in setext_title_lines:
            headings.append(
                _RawHeading(
                    title=lines[idx].strip(),
                    depth=setext_titles[idx],
                    line=idx + 1,
                    style="setext",
                )
            )
            continue

        # The underline of a setext heading must not be re-read as anything else.
        if detect_setext and _SETEXT_RE.match(raw) and (idx - 1) in setext_title_lines:
            continue

        if detect_atx:
            atx = _ATX_RE.match(raw)
            if atx:
                title = atx.group(2).strip()
                if title:
                    headings.append(
                        _RawHeading(
                            title=title,
                            depth=len(atx.group(1)),
                            line=idx + 1,
                            style="atx",
                        )
                    )
                continue

        if detect_numbered:
            numbered = _match_numbered(raw)
            if numbered:
                depth, title = numbered
                headings.append(
                    _RawHeading(title=title, depth=depth, line=idx + 1, style="numbered")
                )

    headings.sort(key=lambda h: h.line)
    return headings


def _assign_tree(headings: list[_RawHeading], last_line: int) -> None:
    """Populate ``end_line`` and ``parent`` for every heading via a depth stack."""

    stack: list[int] = []
    for idx, heading in enumerate(headings):
        while stack and heading.depth <= headings[stack[-1]].depth:
            headings[stack.pop()].end_line = heading.line - 1
        heading.parent = stack[-1] if stack else None
        stack.append(idx)
    for idx in stack:
        headings[idx].end_line = last_line


# --- Sectioning -------------------------------------------------------------


def _build_sections(
    headings: list[_RawHeading],
    root_depth: int,
    last_line: int,
    *,
    include_nested: bool,
) -> list[_Section]:
    """Partition the document into a preamble plus one section per root heading."""

    def listed(start: int, end: int) -> list[int]:
        return [
            i
            for i, h in enumerate(headings)
            if start <= h.line <= end and (include_nested or h.depth == root_depth)
        ]

    root_idxs = [i for i, h in enumerate(headings) if h.depth == root_depth]
    sections: list[_Section] = []

    first_root_line = headings[root_idxs[0]].line
    if first_root_line > 1:
        sections.append(
            _Section("preamble", 1, first_root_line - 1, listed(1, first_root_line - 1))
        )

    for pos, root_idx in enumerate(root_idxs):
        start = headings[root_idx].line
        if pos + 1 < len(root_idxs):
            end = headings[root_idxs[pos + 1]].line - 1
        else:
            end = last_line
        sections.append(_Section("section", start, end, listed(start, end)))

    return sections


# --- Size-aware splitting ---------------------------------------------------


def _split_long_line(line: str, max_bytes: int) -> list[str]:
    """Break a single oversized line into <= ``max_bytes`` UTF-8 pieces."""

    pieces: list[str] = []
    current = ""
    current_bytes = 0
    for ch in line:
        cb = len(ch.encode("utf-8"))
        if current and current_bytes + cb > max_bytes:
            pieces.append(current)
            current, current_bytes = "", 0
        current += ch
        current_bytes += cb
    pieces.append(current)
    return pieces


def _split_span_by_bytes(
    lines: list[str], start: int, end: int, max_bytes: int | None
) -> list[tuple[int, int, str]]:
    """Split a line span into parts whose content fits in ``max_bytes``.

    Returns a list of ``(start_line, end_line, content)`` tuples (line numbers
    are 1-based and inclusive).  When ``max_bytes`` is falsy the whole span is
    returned as a single part.  A single line that exceeds the budget on its own
    is force-split into sub-line pieces that share its line number.
    """

    if not max_bytes or max_bytes <= 0:
        return [(start, end, "\n".join(lines[start - 1 : end]))]

    parts: list[tuple[int, int, str]] = []
    cur_lines: list[str] = []
    cur_start = start
    cur_bytes = 0
    for ln in range(start, end + 1):
        line = lines[ln - 1]
        line_bytes = len(line.encode("utf-8"))
        sep = 1 if cur_lines else 0

        if line_bytes > max_bytes:
            if cur_lines:
                parts.append((cur_start, ln - 1, "\n".join(cur_lines)))
                cur_lines, cur_bytes = [], 0
            for piece in _split_long_line(line, max_bytes):
                parts.append((ln, ln, piece))
            cur_start = ln + 1
            continue

        if cur_lines and cur_bytes + sep + line_bytes > max_bytes:
            parts.append((cur_start, ln - 1, "\n".join(cur_lines)))
            cur_lines, cur_bytes, cur_start = [line], line_bytes, ln
        else:
            cur_lines.append(line)
            cur_bytes += sep + line_bytes

    if cur_lines:
        parts.append((cur_start, end, "\n".join(cur_lines)))
    return parts or [(start, end, "\n".join(lines[start - 1 : end]))]


# --- Chunk assembly ---------------------------------------------------------


def _make_chunk(
    headings: list[_RawHeading],
    *,
    kind: str,
    start: int,
    end: int,
    content: str,
    root_depth: int,
    include_nested: bool,
    part: int,
    part_count: int,
) -> Chunk:
    listed = [
        h
        for h in headings
        if start <= h.line <= end and (include_nested or h.depth == root_depth)
    ]
    line_span = max(end - start + 1, 1)
    heading_count = len(listed)
    if listed:
        max_depth = max(h.depth - root_depth + 1 for h in listed)
    else:
        max_depth = 0
    density = heading_count / line_span
    return Chunk(
        index=0,  # assigned later
        kind=kind,
        heading_titles=[h.title for h in listed],
        heading_count=heading_count,
        start_line=start,
        end_line=end,
        line_span=line_span,
        max_depth=max_depth,
        density=density,
        density_per_100_lines=density * 100.0,
        part=part,
        part_count=part_count,
        content=content,
    )


def _emitted_headings(
    headings: list[_RawHeading], chunks: list[Chunk], root_depth: int, include_nested: bool
) -> list[Heading]:
    """Build the flat heading list, scoped to headings that survived chunking."""

    def chunk_for(line: int) -> int | None:
        for chunk in chunks:
            if chunk.start_line <= line <= chunk.end_line:
                return chunk.index
        return None

    emitted: list[tuple[int, Heading]] = []
    raw_to_node: dict[int, int] = {}
    next_index = 1
    for raw_idx, heading in enumerate(headings):
        if not include_nested and heading.depth != root_depth:
            continue
        chunk_index = chunk_for(heading.line)
        if chunk_index is None:
            continue
        raw_to_node[raw_idx] = next_index
        emitted.append(
            (
                raw_idx,
                Heading(
                    index=next_index,
                    title=heading.title,
                    depth=heading.depth,
                    relative_depth=heading.depth - root_depth + 1,
                    line=heading.line,
                    end_line=heading.end_line or heading.line,
                    style=heading.style,
                    chunk_index=chunk_index,
                ),
            )
        )
        next_index += 1

    # Resolve parents to the nearest *emitted* ancestor.
    for raw_idx, node in emitted:
        parent = headings[raw_idx].parent
        while parent is not None and parent not in raw_to_node:
            parent = headings[parent].parent
        node.parent_index = raw_to_node.get(parent) if parent is not None else None

    return [node for _, node in emitted]


# --- Entry point ------------------------------------------------------------


def chunk_text(
    text: str,
    *,
    include_nested: bool = True,
    include_singletons: bool = True,
    detect_atx: bool = True,
    detect_setext: bool = True,
    detect_numbered: bool = True,
    max_chunks: int = DEFAULT_MAX_CHUNKS,
    max_chunk_bytes: int | None = DEFAULT_MAX_CHUNK_BYTES,
    split_oversized: bool = True,
) -> ChunkingResult:
    """Chunk ``text`` by its heading structure.

    Args:
        text: The document to chunk.
        include_nested: List sub-headings inside their section's chunk. When
            ``False`` only root-level headings are reported (and ``max_depth``
            collapses to 1).
        include_singletons: Emit chunks that contain fewer than two listed
            headings (the preamble and single-heading sections).  Set ``False``
            to keep only multi-heading sections; note this can drop content by
            design.
        detect_atx / detect_setext / detect_numbered: Toggle each heading style.
        max_chunks: Cap on the number of chunks returned; extras are dropped and
            ``truncated`` is set.
        max_chunk_bytes: Soft cap on a chunk's UTF-8 size.
        split_oversized: When ``True`` a section larger than ``max_chunk_bytes``
            is split into ordered parts; when ``False`` the size limit is left
            for the caller to enforce.

    Returns:
        A :class:`ChunkingResult` with full, non-overlapping coverage of the
        input (subject to the ``include_singletons`` filter).
    """

    if max_chunks <= 0:
        raise ValueError("max_chunks must be a positive integer.")

    if not text:
        return ChunkingResult()

    lines = _normalise(text)
    last_line = len(lines)
    byte_limit = max_chunk_bytes if split_oversized else None

    headings = _detect_headings(
        lines,
        detect_atx=detect_atx,
        detect_setext=detect_setext,
        detect_numbered=detect_numbered,
    )

    # Unstructured document: emit the whole thing as one chunk (when allowed).
    if not headings:
        if not include_singletons:
            return ChunkingResult()
        chunks: list[Chunk] = []
        for part_no, (start, end, content) in enumerate(
            _split_span_by_bytes(lines, 1, last_line, byte_limit), start=1
        ):
            chunks.append(
                _make_chunk(
                    [],
                    kind="document",
                    start=start,
                    end=end,
                    content=content,
                    root_depth=1,
                    include_nested=include_nested,
                    part=part_no,
                    part_count=0,  # fixed up below
                )
            )
        return _finalise(chunks, [], max_chunks, root_depth=None)

    _assign_tree(headings, last_line)
    root_depth = min(h.depth for h in headings)
    sections = _build_sections(
        headings, root_depth, last_line, include_nested=include_nested
    )

    chunks = []
    for section in sections:
        if not include_singletons and len(section.heading_idxs) < 2:
            continue
        parts = _split_span_by_bytes(lines, section.start_line, section.end_line, byte_limit)
        for part_no, (start, end, content) in enumerate(parts, start=1):
            chunks.append(
                _make_chunk(
                    headings,
                    kind=section.kind,
                    start=start,
                    end=end,
                    content=content,
                    root_depth=root_depth,
                    include_nested=include_nested,
                    part=part_no,
                    part_count=len(parts),
                )
            )

    return _finalise(chunks, headings, max_chunks, root_depth, include_nested)


def _finalise(
    chunks: list[Chunk],
    headings: list[_RawHeading],
    max_chunks: int,
    root_depth: int | None,
    include_nested: bool = True,
) -> ChunkingResult:
    chunks.sort(key=lambda c: (c.start_line, c.part))

    truncated = len(chunks) > max_chunks
    if truncated:
        chunks = chunks[:max_chunks]

    for new_index, chunk in enumerate(chunks, start=1):
        chunk.index = new_index
        if chunk.part_count == 0:  # the unstructured single-document path
            chunk.part_count = len([c for c in chunks if c.kind == "document"])

    emitted_headings = (
        _emitted_headings(headings, chunks, root_depth, include_nested)
        if headings and root_depth is not None
        else []
    )

    return ChunkingResult(
        chunks=chunks,
        headings=emitted_headings,
        count=len(chunks),
        truncated=truncated,
        root_depth=root_depth,
    )


if __name__ == "__main__":  # pragma: no cover - tiny manual smoke test
    import json

    sample = """Intro paragraph before any heading.

# Title One
Body of section one.

## Sub A
More text.

# Title Two
Final section.
"""
    result = chunk_text(sample)
    print(f"root_depth={result.root_depth} count={result.count} truncated={result.truncated}")
    for c in result.chunks:
        print(
            json.dumps(
                {
                    "index": c.index,
                    "kind": c.kind,
                    "lines": [c.start_line, c.end_line],
                    "headings": c.heading_titles,
                    "max_depth": c.max_depth,
                }
            )
        )
