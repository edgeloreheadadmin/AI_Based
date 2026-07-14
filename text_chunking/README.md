# `text_chunking` — structure-aware chunking + document intelligence

This package began as a redesign of the `chunk_text_structures` tool — **split
a text/Markdown document into hierarchical, heading-based chunks with depth
and density metrics** — and has grown into a full document-intelligence suite
layered on that engine. Everything is stdlib-only and unit-tested.

## Layout

| File | Responsibility |
| --- | --- |
| `text_structure_chunker.py` | Pure, dependency-free chunking engine. Owns heading detection, sectioning, the structure tree, and size-aware splitting. |
| `chunk_text_structures.py` | Thin `BaseTool` adapter: input loading, option resolution, size limits, and the tool's UI surface. Delegates all parsing to the engine. |
| `tokenizer.py` | Lexical tokenization: typed tokens (word/number/punctuation/symbol) with exact line/column/offset positions. Shared alphabet for every other layer. |
| `syntax_tree.py` | Document AST (**syntax trees**): sections/paragraphs/code/lists down to sentences, POS-tagged words, and shallow NP/VP/PP phrase structures (**grammatical structures**). S-expression and dict renderings. |
| `language_model.py` | N-gram model: **next-token / next-word prediction** (stupid backoff), interpolated probabilities, perplexity, deterministic generation. |
| `intellisense.py` | **IntelliSense**: trie prefix completion ranked by frequency, bigram context boosting, heading-symbol completion, fuzzy fallback, hover cards. |
| `semantics.py` | **Semantic intelligence**: TF-IDF vectors, cosine similarity, semantic search, keyword extraction, related-chunk lookup, TextTiling-style topic-boundary detection. |
| `chunk_predictor.py` | **Next-chunk-size prediction**: Holt level+trend smoothing with per-kind structural priors and Welford prediction intervals. |
| `metaheuristics.py` | **Meta-heuristics**: chunk-boundary optimization (size × cohesion × structure objective) via hill climbing, simulated annealing, and a genetic algorithm — seeded from the structural chunking. |
| `pipeline.py` | `DocumentIntelligence` facade: one object exposing outline, predictions, completions, search, forecasts, optimized plans and a JSON report. |
| `test_*.py` | 86 unit tests across all modules (`python -m unittest`). No third-party deps required. |

Only `chunk_text_structures.py` (the codex adapter) needs `pydantic` +
`codex`; the package `__init__` deliberately does not import it, so the whole
intelligence suite works with a bare Python install.

## Quick tour

```python
from text_chunking import DocumentIntelligence

doc = DocumentIntelligence(open("README.md").read())
doc.outline()                      # [(1, 'Title'), (2, 'Subsection'), ...]
doc.next_word("the")               # [('cat', 0.62), ...]   n-gram prediction
doc.complete("chu", context="text")# ranked IntelliSense suggestions
doc.search("database queries")     # semantic search over chunks
doc.predict_next_chunk_size()      # ChunkSizeForecast(bytes=882, ...)
doc.optimized_plan(target_bytes=800)  # annealed chunk boundaries
doc.report()                       # JSON-serialisable summary
```

Or run the demo directly: `python -m text_chunking.pipeline some_file.md`.

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

From the repository root:

```bash
python -m unittest discover -s text_chunking -t . -v
```
