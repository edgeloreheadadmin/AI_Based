"""Structure-aware text chunking and document intelligence.

Dependency-free (stdlib only) modules:

* ``text_structure_chunker`` -- heading-based structural chunking engine.
* ``tokenizer``              -- lexical tokenization with positions.
* ``syntax_tree``            -- document AST, sentence/POS/phrase grammar.
* ``language_model``         -- n-gram next-token / next-word prediction.
* ``intellisense``           -- prefix/fuzzy completion engine with context.
* ``semantics``              -- TF-IDF semantic index, similarity, boundaries.
* ``chunk_predictor``        -- next-chunk-size forecasting.
* ``metaheuristics``         -- chunk-boundary optimization (hill climbing,
                                simulated annealing, genetic algorithm).
* ``pipeline``               -- the ``DocumentIntelligence`` facade.

``chunk_text_structures`` (the codex ``BaseTool`` adapter) is intentionally
NOT imported here because it requires ``pydantic`` and the ``codex``
framework; import it explicitly when embedding the tool.
"""

from .chunk_predictor import ChunkSizeForecast, ChunkSizePredictor
from .intellisense import CompletionEngine, Suggestion
from .language_model import NGramModel
from .metaheuristics import OptimizationResult, optimize_chunking
from .pipeline import DocumentIntelligence
from .semantics import SemanticIndex, TfIdfVectorizer, cohesion, semantic_boundaries
from .syntax_tree import parse_document, split_sentences, to_dict, to_sexpr, walk
from .text_structure_chunker import Chunk, ChunkingResult, Heading, chunk_text
from .tokenizer import Token, TokenType, lexical_tokens, tokenize, words

__all__ = [
    "Chunk",
    "ChunkingResult",
    "ChunkSizeForecast",
    "ChunkSizePredictor",
    "CompletionEngine",
    "DocumentIntelligence",
    "Heading",
    "NGramModel",
    "OptimizationResult",
    "SemanticIndex",
    "Suggestion",
    "TfIdfVectorizer",
    "Token",
    "TokenType",
    "chunk_text",
    "cohesion",
    "lexical_tokens",
    "optimize_chunking",
    "parse_document",
    "semantic_boundaries",
    "split_sentences",
    "to_dict",
    "to_sexpr",
    "tokenize",
    "walk",
    "words",
]
