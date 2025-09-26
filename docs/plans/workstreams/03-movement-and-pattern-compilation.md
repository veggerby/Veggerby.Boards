---
id: 3
slug: movement-and-pattern-compilation
name: "Movement & Pattern Compilation"
status: closed
last_updated: 2025-09-26
owner: core
flags:
  - EnableCompiledPatterns (graduated)
  - EnableCompiledPatternsAdjacencyCache (exp)
summary: >-
  Compiles supported movement patterns (Fixed, Direction, MultiDirection) into IR (Fixed/Ray/MultiRay) with
  adjacency cache support, parity suites, and edge-case semantics charter. Aggregate ≥5× throughput goal deferred.
acceptance:
  - Parity across archetype + randomized tests for supported kinds.
  - Scope guard tests for excluded patterns (Any, Null).
  - Edge-case semantics charter published.
open_followups:
  - Heuristic pruning for dense random queries.
  - Additional pattern kinds (conditional/leaper/wildcard) after charter update.
  - Large-sample latency improvement (current slight regression).
---

# Movement & Pattern Compilation

## Delivered

- IR & compiler for Fixed / Direction / MultiDirection
- Parity (archetypes + randomized 2000 sample harness)
- Scope guard tests (AnyPattern / NullPattern uncompiled)
- Edge semantics charter (blockers, captures, tie-breaking, zero-length)
- Adjacency cache flag scaffold
- Micro benchmarks per IR kind (~23% speed + ~23% fewer allocations vs visitor)
- Large-sample benchmark (allocation win, latency regression documented)

## Deferred Target

≥5× aggregate throughput deferred pending pruning heuristics & topology-based optimization.

## Risks

Heuristic overfitting may reduce determinism clarity; guard via expanded parity after each heuristic.

## Next Steps

See open_followups.
