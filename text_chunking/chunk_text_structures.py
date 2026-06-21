"""``chunk_text_structures`` tool.

Splits a text document into hierarchical, heading-based chunks with depth and
density metrics.  This module is a thin adapter over the dependency-free
:mod:`text_structure_chunker` engine: it handles input loading, option
resolution, size limits and the tool's UI surface, while the engine owns all of
the parsing and chunking logic (and carries the unit tests).

Notable changes from the previous revision of this file:

* The opaque ``asunai_security`` import-time ``bootstrap.install()`` shim has
  been removed.  It executed code on import, swallowed every exception, and had
  nothing to do with chunking text; it is not part of this tool's purpose.
* Text appearing before the first top-level heading is no longer discarded -- it
  is returned as a ``preamble`` chunk, so the chunks now cover the whole input.
* Numbered-heading detection is conservative, so ordinary numeric lines and
  lower-case ordered lists are no longer mistaken for structure.
* An oversized section is split into ordered parts instead of failing the whole
  call (unless ``split_oversized_chunks`` is disabled, which restores the old
  hard-limit behaviour).
"""

from __future__ import annotations

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

from .text_structure_chunker import (
    DEFAULT_MAX_CHUNK_BYTES,
    DEFAULT_MAX_CHUNKS,
    Chunk,
    Heading,
    chunk_text,
)

if TYPE_CHECKING:
    from codex.core.types import ToolCallEvent, ToolResultEvent


# --- Configuration ----------------------------------------------------------


class ChunkTextStructuresConfig(BaseToolConfig):
    permission: ToolPermission = ToolPermission.ALWAYS
    max_input_bytes: int = Field(
        default=5_000_000, description="Maximum input size in bytes."
    )
    max_chunk_bytes: int = Field(
        default=DEFAULT_MAX_CHUNK_BYTES,
        description="Maximum structure chunk size in bytes.",
    )
    max_chunks: int = Field(
        default=DEFAULT_MAX_CHUNKS,
        description="Maximum number of structure chunks to return.",
    )
    split_oversized_chunks: bool = Field(
        default=True,
        description=(
            "Split a section larger than max_chunk_bytes into ordered parts "
            "instead of raising an error."
        ),
    )
    include_nested: bool = Field(
        default=True, description="Include nested headings in results."
    )
    include_singletons: bool = Field(
        default=True,
        description="Include sections with only a single heading (or none).",
    )
    include_markdown_headings: bool = Field(
        default=True, description="Detect Markdown # style (ATX) headings."
    )
    include_setext_headings: bool = Field(
        default=True, description="Detect setext-style headings (---/===)."
    )
    include_numbered_headings: bool = Field(
        default=True, description="Detect numbered headings (e.g., 1.2 Title)."
    )


class ChunkTextStructuresState(BaseToolState):
    pass


# --- Arguments --------------------------------------------------------------


class ChunkTextStructuresArgs(BaseModel):
    content: str | None = Field(default=None, description="Raw text to chunk.")
    path: str | None = Field(default=None, description="Path to a text file.")
    include_nested: bool | None = Field(
        default=None, description="Include nested headings."
    )
    include_singletons: bool | None = Field(
        default=None, description="Include single-heading sections."
    )
    include_markdown_headings: bool | None = Field(
        default=None, description="Detect Markdown # (ATX) headings."
    )
    include_setext_headings: bool | None = Field(
        default=None, description="Detect setext headings."
    )
    include_numbered_headings: bool | None = Field(
        default=None, description="Detect numbered headings."
    )
    split_oversized_chunks: bool | None = Field(
        default=None, description="Split oversized sections into parts."
    )
    max_chunks: int | None = Field(
        default=None, description="Override the configured max chunks limit."
    )


# --- Result models ----------------------------------------------------------


class HeadingNode(BaseModel):
    index: int
    title: str
    depth: int
    relative_depth: int
    start_line: int
    end_line: int
    parent_index: int | None
    group_index: int
    style: str


class TextStructureChunk(BaseModel):
    index: int
    kind: str
    heading_titles: list[str]
    heading_count: int
    start_line: int
    end_line: int
    line_span: int
    max_depth: int
    density: float
    density_per_100_lines: float
    part: int
    part_count: int
    content: str


class ChunkTextStructuresResult(BaseModel):
    include_nested: bool
    include_singletons: bool
    root_depth: int | None
    chunks: list[TextStructureChunk]
    headings: list[HeadingNode]
    count: int
    truncated: bool


# --- Tool -------------------------------------------------------------------


class ChunkTextStructures(
    BaseTool[
        ChunkTextStructuresArgs,
        ChunkTextStructuresResult,
        ChunkTextStructuresConfig,
        ChunkTextStructuresState,
    ],
    ToolUIData[ChunkTextStructuresArgs, ChunkTextStructuresResult],
):
    description: ClassVar[str] = (
        "Chunk text by heading structure with depth and density metrics."
    )

    async def run(self, args: ChunkTextStructuresArgs) -> ChunkTextStructuresResult:
        content = self._load_content(args)

        include_nested = self._resolve(args.include_nested, self.config.include_nested)
        include_singletons = self._resolve(
            args.include_singletons, self.config.include_singletons
        )
        detect_atx = self._resolve(
            args.include_markdown_headings, self.config.include_markdown_headings
        )
        detect_setext = self._resolve(
            args.include_setext_headings, self.config.include_setext_headings
        )
        detect_numbered = self._resolve(
            args.include_numbered_headings, self.config.include_numbered_headings
        )
        split_oversized = self._resolve(
            args.split_oversized_chunks, self.config.split_oversized_chunks
        )
        max_chunks = self._resolve(args.max_chunks, self.config.max_chunks)

        if max_chunks <= 0:
            raise ToolError("max_chunks must be a positive integer.")

        try:
            result = chunk_text(
                content,
                include_nested=include_nested,
                include_singletons=include_singletons,
                detect_atx=detect_atx,
                detect_setext=detect_setext,
                detect_numbered=detect_numbered,
                max_chunks=max_chunks,
                max_chunk_bytes=self.config.max_chunk_bytes,
                split_oversized=split_oversized,
            )
        except ValueError as exc:  # defensive: engine validates its own inputs
            raise ToolError(str(exc)) from exc

        chunks = [self._to_chunk_model(chunk) for chunk in result.chunks]
        if not split_oversized:
            self._validate_chunk_sizes(chunks)

        return ChunkTextStructuresResult(
            include_nested=include_nested,
            include_singletons=include_singletons,
            root_depth=result.root_depth,
            chunks=chunks,
            headings=[self._to_heading_model(node) for node in result.headings],
            count=len(chunks),
            truncated=result.truncated,
        )

    # -- option helpers ------------------------------------------------------

    @staticmethod
    def _resolve(override, default):
        return override if override is not None else default

    # -- input loading -------------------------------------------------------

    def _load_content(self, args: ChunkTextStructuresArgs) -> str:
        if args.content is not None and args.path is not None:
            raise ToolError("Provide content or path, not both.")
        if args.content is None and args.path is None:
            raise ToolError("Provide content or path.")

        if args.content is not None:
            self._validate_input_size(len(args.content.encode("utf-8")))
            return args.content

        path = self._resolve_path(args.path or "")
        self._validate_input_size(path.stat().st_size)
        return path.read_text("utf-8", errors="ignore")

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
        except OSError as exc:
            raise ToolError(f"Failed to resolve path: {exc}") from exc

        if not resolved.exists():
            raise ToolError(f"Path not found: {resolved}")
        if resolved.is_dir():
            raise ToolError(f"Path is a directory, not a file: {resolved}")
        return resolved

    def _validate_chunk_sizes(self, chunks: list[TextStructureChunk]) -> None:
        max_bytes = self.config.max_chunk_bytes
        for chunk in chunks:
            size = len(chunk.content.encode("utf-8"))
            if size > max_bytes:
                raise ToolError(
                    f"Chunk {chunk.index} exceeds max_chunk_bytes "
                    f"({size} > {max_bytes}). Enable split_oversized_chunks to "
                    f"split large sections automatically."
                )

    # -- engine -> pydantic mapping ------------------------------------------

    @staticmethod
    def _to_chunk_model(chunk: Chunk) -> TextStructureChunk:
        return TextStructureChunk(
            index=chunk.index,
            kind=chunk.kind,
            heading_titles=chunk.heading_titles,
            heading_count=chunk.heading_count,
            start_line=chunk.start_line,
            end_line=chunk.end_line,
            line_span=chunk.line_span,
            max_depth=chunk.max_depth,
            density=chunk.density,
            density_per_100_lines=chunk.density_per_100_lines,
            part=chunk.part,
            part_count=chunk.part_count,
            content=chunk.content,
        )

    @staticmethod
    def _to_heading_model(node: Heading) -> HeadingNode:
        return HeadingNode(
            index=node.index,
            title=node.title,
            depth=node.depth,
            relative_depth=node.relative_depth,
            start_line=node.line,
            end_line=node.end_line,
            parent_index=node.parent_index,
            group_index=node.chunk_index or 0,
            style=node.style,
        )

    # -- UI ------------------------------------------------------------------

    @classmethod
    def get_call_display(cls, event: ToolCallEvent) -> ToolCallDisplay:
        if not isinstance(event.args, ChunkTextStructuresArgs):
            return ToolCallDisplay(summary="chunk_text_structures")

        return ToolCallDisplay(
            summary="chunk_text_structures",
            details={
                "path": event.args.path,
                "include_nested": event.args.include_nested,
                "include_singletons": event.args.include_singletons,
                "include_markdown_headings": event.args.include_markdown_headings,
                "include_setext_headings": event.args.include_setext_headings,
                "include_numbered_headings": event.args.include_numbered_headings,
                "split_oversized_chunks": event.args.split_oversized_chunks,
                "max_chunks": event.args.max_chunks,
            },
        )

    @classmethod
    def get_result_display(cls, event: ToolResultEvent) -> ToolResultDisplay:
        if not isinstance(event.result, ChunkTextStructuresResult):
            return ToolResultDisplay(
                success=False, message=event.error or event.skip_reason or "No result"
            )

        result = event.result
        message = f"Found {result.count} text structure chunk(s)"
        warnings: list[str] = []
        if result.truncated:
            warnings.append("Chunk list truncated by max_chunks limit")
        if any(chunk.part_count > 1 for chunk in result.chunks):
            warnings.append("Some sections were split to respect max_chunk_bytes")

        return ToolResultDisplay(
            success=True,
            message=message,
            warnings=warnings,
            details={
                "include_nested": result.include_nested,
                "include_singletons": result.include_singletons,
                "root_depth": result.root_depth,
                "count": result.count,
                "truncated": result.truncated,
                "chunks": result.chunks,
                "headings": result.headings,
            },
        )

    @classmethod
    def get_status_text(cls) -> str:
        return "Chunking text structures"
