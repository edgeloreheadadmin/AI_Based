"""Meta-heuristic optimization of chunk boundaries.

Where the structural engine chunks strictly by headings, this module treats
chunking as a combinatorial optimization problem: choose breakpoints between
atomic units (paragraph blocks) that jointly minimise

* **size deviation** -- squared relative error of each chunk against a target
  byte size,
* **semantic incoherence** -- ``1 - mean intra-chunk cohesion`` computed from
  TF-IDF vectors of the units, so semantically related units stay together,
* **structural misalignment** -- fraction of breakpoints that do NOT fall on
  a heading, so boundaries prefer the document's own structure.

Three interchangeable solvers, all deterministic under a seed:

* :func:`hill_climb`            -- greedy first-improvement local search,
* :func:`simulated_annealing`   -- escapes local optima via temperature,
* :func:`genetic_algorithm`     -- population search with crossover/mutation.

Entry point: :func:`optimize_chunking`.  The initial state is seeded from the
structural chunker's heading boundaries, so the meta-heuristics refine (never
ignore) the document's structure.  Stdlib only.
"""

from __future__ import annotations

from dataclasses import dataclass
from typing import Callable
import math
import random

from .semantics import TfIdfVectorizer
from .text_structure_chunker import _detect_headings, _normalise

__all__ = [
    "Unit",
    "CostWeights",
    "OptimizationResult",
    "optimize_chunking",
]

Breaks = tuple[int, ...]
CostFn = Callable[[Breaks], float]


# --- Problem construction ----------------------------------------------------


@dataclass(frozen=True)
class Unit:
    """An atomic block of the document (never split by the optimizer)."""

    text: str
    nbytes: int
    is_heading: bool
    start_line: int


@dataclass(frozen=True)
class CostWeights:
    size: float = 1.0
    cohesion: float = 0.35
    structure: float = 0.25


def _split_units(text: str) -> list[Unit]:
    """Split the document into paragraph blocks; headings open a new block."""

    lines = _normalise(text)
    heading_lines = {
        h.line
        for h in _detect_headings(
            lines, detect_atx=True, detect_setext=True, detect_numbered=True
        )
    }

    units: list[Unit] = []
    current: list[str] = []
    start = 1
    starts_with_heading = False

    def flush() -> None:
        nonlocal current, starts_with_heading
        if current:
            block = "\n".join(current)
            units.append(
                Unit(
                    text=block,
                    nbytes=len(block.encode("utf-8")),
                    is_heading=starts_with_heading,
                    start_line=start,
                )
            )
        current = []
        starts_with_heading = False

    for index, line in enumerate(lines, start=1):
        if not line.strip():
            flush()
            continue
        if index in heading_lines and current:
            flush()
        if not current:
            start = index
            starts_with_heading = index in heading_lines
        current.append(line)
    flush()
    return units


# --- Cost function -----------------------------------------------------------


def _spans(breaks: Breaks, n: int) -> list[tuple[int, int]]:
    edges = [0, *breaks, n]
    return [(edges[i], edges[i + 1]) for i in range(len(edges) - 1)]


def _make_cost(
    units: list[Unit],
    vectors: list[dict[str, float]],
    target_bytes: int,
    weights: CostWeights,
) -> CostFn:
    sizes = [u.nbytes for u in units]
    n = len(units)

    def chunk_bytes(start: int, end: int) -> int:
        return sum(sizes[start:end]) + 2 * max(end - start - 1, 0)  # "\n\n" joins

    def chunk_cohesion(start: int, end: int) -> float:
        if end - start <= 1:
            return 1.0
        similarities = [
            TfIdfVectorizer.cosine(vectors[i], vectors[i + 1])
            for i in range(start, end - 1)
        ]
        return sum(similarities) / len(similarities)

    def cost(breaks: Breaks) -> float:
        spans = _spans(breaks, n)
        size_term = sum(
            ((chunk_bytes(s, e) - target_bytes) / target_bytes) ** 2 for s, e in spans
        ) / len(spans)
        cohesion_term = 1.0 - sum(chunk_cohesion(s, e) for s, e in spans) / len(spans)
        if breaks:
            misaligned = sum(1 for b in breaks if not units[b].is_heading)
            structure_term = misaligned / len(breaks)
        else:
            structure_term = 0.0
        return (
            weights.size * size_term
            + weights.cohesion * cohesion_term
            + weights.structure * structure_term
        )

    return cost


# --- Neighbourhood -----------------------------------------------------------


def _neighbor(breaks: Breaks, n: int, rng: random.Random) -> Breaks:
    """One random edit: move a breakpoint, add one, or remove one."""

    current = list(breaks)
    operations = []
    if current:
        operations += ["move", "remove"]
    if len(current) < n - 1:
        operations.append("add")
    if not operations:
        return breaks

    operation = rng.choice(operations)
    if operation == "move":
        index = rng.randrange(len(current))
        moved = current[index] + rng.choice((-1, 1))
        if 1 <= moved <= n - 1 and moved not in current:
            current[index] = moved
    elif operation == "add":
        candidates = [b for b in range(1, n) if b not in current]
        current.append(rng.choice(candidates))
    else:  # remove
        current.pop(rng.randrange(len(current)))
    return tuple(sorted(current))


# --- Solvers -------------------------------------------------------------------


def hill_climb(
    cost_fn: CostFn, initial: Breaks, n: int, rng: random.Random, iterations: int = 400
) -> tuple[Breaks, list[float]]:
    best, best_cost = initial, cost_fn(initial)
    history = [best_cost]
    for _ in range(iterations):
        candidate = _neighbor(best, n, rng)
        candidate_cost = cost_fn(candidate)
        if candidate_cost < best_cost:
            best, best_cost = candidate, candidate_cost
            history.append(best_cost)
    return best, history


def simulated_annealing(
    cost_fn: CostFn,
    initial: Breaks,
    n: int,
    rng: random.Random,
    iterations: int = 1500,
    initial_temperature: float = 0.3,
    cooling: float = 0.995,
) -> tuple[Breaks, list[float]]:
    current, current_cost = initial, cost_fn(initial)
    best, best_cost = current, current_cost
    history = [best_cost]
    temperature = initial_temperature
    for _ in range(iterations):
        candidate = _neighbor(current, n, rng)
        candidate_cost = cost_fn(candidate)
        delta = candidate_cost - current_cost
        if delta < 0 or (temperature > 1e-9 and rng.random() < math.exp(-delta / temperature)):
            current, current_cost = candidate, candidate_cost
            if current_cost < best_cost:
                best, best_cost = current, current_cost
                history.append(best_cost)
        temperature *= cooling
    return best, history


def genetic_algorithm(
    cost_fn: CostFn,
    initial: Breaks,
    n: int,
    rng: random.Random,
    population_size: int = 24,
    generations: int = 40,
    mutation_rate: float = 0.4,
) -> tuple[Breaks, list[float]]:
    def random_individual() -> Breaks:
        k = rng.randrange(0, max(n // 2, 1))
        return tuple(sorted(rng.sample(range(1, n), min(k, n - 1)))) if n > 1 else ()

    def crossover(a: Breaks, b: Breaks) -> Breaks:
        union = sorted(set(a) | set(b))
        child = tuple(sorted(b for b in union if rng.random() < 0.5))
        return child

    population = [initial] + [random_individual() for _ in range(population_size - 1)]
    scored = sorted((cost_fn(ind), ind) for ind in population)
    history = [scored[0][0]]

    for _ in range(generations):
        def tournament() -> Breaks:
            picks = rng.sample(scored, min(3, len(scored)))
            return min(picks)[1]

        next_population = [scored[0][1]]  # elitism
        while len(next_population) < population_size:
            child = crossover(tournament(), tournament())
            if rng.random() < mutation_rate:
                child = _neighbor(child, n, rng)
            next_population.append(child)
        scored = sorted((cost_fn(ind), ind) for ind in next_population)
        history.append(scored[0][0])

    return scored[0][1], history


_SOLVERS = {
    "hill": hill_climb,
    "annealing": simulated_annealing,
    "genetic": genetic_algorithm,
}


# --- Entry point ---------------------------------------------------------------


@dataclass(frozen=True)
class OptimizationResult:
    method: str
    breaks: Breaks
    spans: list[tuple[int, int]]  # unit-index spans, end exclusive
    chunk_texts: list[str]
    cost: float
    initial_cost: float
    history: list[float]
    unit_count: int


def optimize_chunking(
    text: str,
    target_bytes: int = 800,
    *,
    method: str = "annealing",
    seed: int = 13,
    weights: CostWeights | None = None,
    **solver_options,
) -> OptimizationResult:
    """Optimize chunk boundaries for ``text`` with a meta-heuristic solver.

    ``method`` is one of ``"hill"``, ``"annealing"`` or ``"genetic"``.  The
    search starts from the structural (heading-based) boundaries and is fully
    deterministic for a given ``seed``.
    """

    if method not in _SOLVERS:
        raise ValueError(f"Unknown method {method!r}; choose from {sorted(_SOLVERS)}")
    if target_bytes <= 0:
        raise ValueError("target_bytes must be positive.")

    weights = weights or CostWeights()
    units = _split_units(text)
    n = len(units)
    if n <= 1:
        chunk = units[0].text if units else ""
        return OptimizationResult(
            method=method,
            breaks=(),
            spans=[(0, n)] if n else [],
            chunk_texts=[chunk] if n else [],
            cost=0.0,
            initial_cost=0.0,
            history=[0.0],
            unit_count=n,
        )

    vectorizer = TfIdfVectorizer().fit([u.text for u in units])
    vectors = [vectorizer.transform(u.text) for u in units]
    cost_fn = _make_cost(units, vectors, target_bytes, weights)

    structural = tuple(i for i in range(1, n) if units[i].is_heading)
    initial_cost = cost_fn(structural)

    rng = random.Random(seed)
    best, history = _SOLVERS[method](cost_fn, structural, n, rng, **solver_options)
    best_cost = cost_fn(best)

    if initial_cost < best_cost:  # never return something worse than the seed
        best, best_cost = structural, initial_cost

    spans = _spans(best, n)
    chunk_texts = ["\n\n".join(u.text for u in units[s:e]) for s, e in spans]
    return OptimizationResult(
        method=method,
        breaks=best,
        spans=spans,
        chunk_texts=chunk_texts,
        cost=best_cost,
        initial_cost=initial_cost,
        history=history,
        unit_count=n,
    )
