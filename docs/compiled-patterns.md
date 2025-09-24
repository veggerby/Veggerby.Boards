# Compiled Movement Patterns

This document describes the experimental compiled movement pattern subsystem. It converts high-level piece
patterns into a simplified immutable intermediate representation (IR) for faster path resolution while preserving
determinism and immutability guarantees.

## Goals

- Deterministic: identical state + requested (from,to,piece) yields identical path as legacy visitor.
- Faster resolution for common movement forms (fixed sequences, rays, multi-rays).
- Zero behavioral drift; feature flag (`EnableCompiledPatterns`) gates runtime usage.
- Minimal allocations during resolution (reuse small arrays; construct only resulting `TilePath`).

## Feature Flag

`Veggerby.Boards.Internal.FeatureFlags.EnableCompiledPatterns`

- Off (default): Legacy visitor exclusively used.
- On (build time via builder): Patterns compiled and resolver attached to `EngineServices`.
- Path resolution helpers (`Game.ResolvePathCompiledFirst` / `GameProgress.ResolvePathCompiledFirst`) prefer compiled patterns.

## Intermediate Representation

| Kind      | Description | Directions Payload | Repeatable |
|-----------|-------------|--------------------|------------|
| Fixed     | Exact ordered sequence of steps | All steps in order | false |
| Ray       | Single direction that may optionally repeat | 1 direction | flag |
| MultiRay  | Several alternative directions each treated as independent rays | N directions | flag |

`CompiledPattern` holds:

- `Kind` (`Fixed`, `Ray`, `MultiRay`)
- `Directions` (array of `Direction` references)
- `IsRepeatable` (applies to Ray/MultiRay)

## Compilation Mapping (v1.1)

| Source Pattern            | Mapping Logic |
|---------------------------|---------------|
| `FixedPattern`            | `CompiledPattern.Fixed(steps)` |
| `DirectionPattern`        | `CompiledPattern.Ray(direction, repeat)` |
| `MultiDirectionPattern` (1 direction) | `CompiledPattern.Ray(dir, repeat)` |
| `MultiDirectionPattern` (>1 directions) | `CompiledPattern.MultiRay(dirs, repeat)` |
| `NullPattern`             | Ignored |
| `AnyPattern`              | Ignored (insufficient semantic info) |

## Resolver Semantics

Resolution logic iterates compiled patterns for a piece and attempts to build a `TilePath` from the origin:

1. For `Fixed`: sequentially follow each direction; abort on first missing relation.
2. For `Ray`: follow direction accumulating relations until destination found or dead-end; stop after first segment if not repeatable.
3. For `MultiRay`: iterate each direction as an independent ray (repeat based on flag).
4. Choose the shortest successful path when multiple candidates (deterministic tie by discovery order).
5. Return `null` if no candidate reaches the target.

## Parity Guarantees (Expanded)

Test coverage ensures:

- Fixed sequences (2+ steps) produce identical paths (direct artifact construction parity test for clarity, no builder dependency).
- DirectionPattern repeatable and non-repeatable cases match legacy visitor (single and multi-step attempts).
- MultiDirectionPattern (single and multi-ray) repeatable and non-repeatable cases match legacy visitor.
- Unreachable sequences yield null in both systems.
- Null / Any patterns ignored consistently (legacy may iterate but resolver outputs null equivalently).
- Composite chess archetypes validated: rook (orthogonal slider), bishop (diagonal slider), queen (multi-ray selection / shortest path), knight (fixed L), pawn (single-step non-repeatable).
- Tie-breaking: when multiple rays can reach target with same length, first discovered (legacy visitor order) maintained.

## Usage

```csharp
FeatureFlags.EnableCompiledPatterns = true;
var progress = new ChessGameBuilder().Compile();
var piece = progress.Game.GetPiece("white-pawn-2");
var from = progress.Game.GetTile("e2");
var to = progress.Game.GetTile("e4");
var path = progress.ResolvePathCompiledFirst(piece, from, to); // compiled path
```

Fallback (flag off) uses legacy visitor automatically.

## Roadmap / Recent Additions

+- Added DirectionPattern compilation (Ray form, repeat flag preserved).
+- Added expanded parity test suite (`CompiledChessPatternParityTests`).
+- Added `PatternResolutionBenchmark` comparing legacy visitor vs compiled resolution across archetypes.
+- Adjacency caching scaffold behind `EnableCompiledPatternsAdjacencyCache` (not yet enabled by default).

Planned:

+- Extend compiler to support conditional / composite future pattern types.
+- Expand benchmark dataset to full 8x8 chess board with randomized target sampling.
+- Allocation profiling & potential `TilePath` builder optimization.
+- Opportunistic inlining for hot loops in resolver once stable.

## Invariants

- Compilation is pure: no mutation of artifacts.
- Resolver never mutates board or tiles; only allocates resulting `TilePath` objects.
- Turning flag off after build requires rebuild to detach compiled services (no dynamic unload yet).

## Testing Reference

See:

- `CompiledPatternParityTests`
- `CompiledPatternExtendedParityTests`
- `ChessCompiledIntegrationParityTests`

These collectively guard against behavioral drift.
