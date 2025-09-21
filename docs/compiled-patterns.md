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

## Compilation Mapping (v1)

| Source Pattern            | Mapping Logic |
|---------------------------|---------------|
| `FixedPattern`            | `CompiledPattern.Fixed(steps)` |
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

## Parity Guarantees

Test coverage ensures:

- Fixed sequences (2+ steps) produce identical paths.
- Single and multi-direction repeatable and non-repeatable cases match legacy visitor.
- Unreachable sequences yield null in both systems.
- Null pattern yields null.
- Chess integration (pawn double advance) parity with feature flag on.

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

## Roadmap

- Add adjacency caching (tile index → outgoing relations by direction) to reduce dictionary lookups.
- Extend compiler to support composite patterns (future pattern types) or optional segments.
- Benchmark comparison: integrate into `PatternResolutionBaseline` once expanded to chess board dataset.
- Allocation profiling & potential `TilePath` builder optimization.
- Opportunistic inlining for hot loops in resolver once stable.

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
