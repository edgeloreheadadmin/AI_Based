# `chunk_text_structures` — structure-aware text chunking

This is a redesign of the `chunk_text_structures` tool. Its goal is unchanged:
**split a text/Markdown document into hierarchical, heading-based chunks with
depth and density metrics**, suitable for RAG ingestion, summarisation, or
document navigation.

## Layout

| File | Responsibility |
| --- | --- |
| `text_structure_chunker.py` | Pure, dependency-free chunking engine. Owns heading detection, sectioning, the structure tree, and size-aware splitting. |
| `chunk_text_structures.py` | Thin `BaseTool` adapter: input loading, option resolution, size limits, and the tool's UI surface. Delegates all parsing to the engine. |
| `test_text_structure_chunker.py` | Unit tests for the engine (`python -m unittest`). No third-party deps required. |

The split exists so the parsing logic can be tested and reused in isolation —
the engine has **no `pydantic` or `codex` dependency**.

## What changed from the previous revision, and why

1. **Removed the `asunai_security` import shim.** The old file began with:

   ```python
   try:
       from asunai_security import bootstrap as _asunai_bootstrap
       _asunai_bootstrap.install()
   except Exception:
       pass
   ```

   This ran arbitrary code at import time, swallowed every exception, and had no
   relationship to chunking text. `asunai_security` is not referenced anywhere
   else and is not a declared dependency. It is unrelated to the document's
   intent and has been removed.

2. **Full coverage — the preamble is no longer dropped.** The old code only
   built chunks rooted at top-level headings, so any text before the first
   top-level heading vanished from the output. Every line now belongs to exactly
   one chunk; leading text becomes an explicit `preamble` chunk.

3. **Conservative numbered-heading detection.** The old regex matched any line
   starting with a number (`42 apples`, `2024 review`, `1. buy milk` all became
   "headings"). Detection now requires a real section marker (`1.2` or `1.` /
   `1)`) and rejects lower-case ordered-list items.

4. **Robust detection details.** ATX headings require the `#` to be followed by
   a space (so `#hashtag` is not a heading); setext underlines only apply to a
   single-line paragraph; YAML front matter and fenced code blocks are skipped
   (so a closing `---` is never mistaken for a heading).

5. **Size-aware instead of fail-fast.** A section larger than `max_chunk_bytes`
   is split into ordered `part`/`part_count` chunks rather than raising and
   aborting the whole call. Set `split_oversized_chunks=False` to restore the
   original hard-limit behaviour.

## Output shape

`run(...)` returns a `ChunkTextStructuresResult`:

- `chunks`: ordered, non-overlapping `TextStructureChunk`s. Each has `kind`
  (`section` / `preamble` / `document`), `heading_titles`, `heading_count`,
  `start_line`/`end_line`/`line_span`, `max_depth` (relative, root = 1),
  `density` and `density_per_100_lines`, `part`/`part_count`, and `content`.
- `headings`: a flat `HeadingNode` list scoped to emitted chunks, with
  `relative_depth`, `parent_index` (nearest emitted ancestor), `group_index`
  (owning chunk), and detection `style`.
- `root_depth`, `count`, `truncated`, and the effective `include_*` flags.

## Running the tests

```bash
cd text_chunking
python -m unittest test_text_structure_chunker -v
```
