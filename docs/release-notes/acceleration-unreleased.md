# Acceleration Rollout (Unreleased)

## Summary

Bitboards (occupancy) and sliding fast-path (ray-based attack + path reconstruction) are now ENABLED BY DEFAULT for boards with ≤64 tiles. This follows completion of Parity V2 coverage (447 tests) and benchmark validation demonstrating ≥4.6× speedup for empty sliding path resolution versus legacy visitor.

## What Changed

- `EnableBitboards` default: false → true (≤64 tiles)
- `EnableSlidingFastPath` default: false → true (requires bitboards + sliding attack service)
- Configuration doc updated with quick disable snippet and style reminder
- Action plan updated; follow-up items moved to Next section
- Added detailed fast-path metrics (attempts, hits, skip reasons)
- Added `docs/perf/bitboard128-design.md` (future >64 support design)

## Rationale

Benchmarks show substantial latency reduction in path reconstruction for sliding pieces (rook, bishop, queen) on both empty and semi-blocked scenarios while preserving determinism and parity with compiled resolver results.

## Safety & Determinism

- Parity tests ensure identical geometric reachability + occupancy semantics (friendly block stop, enemy capture terminal, no pass-through) across fast-path and fallback.
- Fast-path only engages when all prerequisites present (services + slider pattern). Otherwise it defers to compiled + legacy cascade.
- Metrics expose skip reasons: NoServices, NotSlider, AttackMiss, ReconstructFail.

## Limitations

- Guarded to boards with at most 64 tiles (no Bitboard128 yet).
- No topology-aware pruning or mobility heuristics integrated yet (planned follow-ups).
- Fast-path currently limited to repeatable directional (sliding) patterns; non-sliders take compiled path.

## Disabling (Temporary)

```csharp
var flags = new FeatureFlags
{
    EnableBitboards = false,
    EnableSlidingFastPath = false,
};
```

## Future / Next Items

- Bitboard128 support (boards up to 128 tiles) — design note drafted.
- Typed mask operations / possible interface consolidation.
- Board topology pruning in DecisionPlan grouping phase.
- Mobility heuristic (popcount-based) experiment.
- LINQ sweep: remove residual allocations in hot mutators.

## Code Style Reaffirmation

All acceleration code adheres to repository style:

- File-scoped namespaces
- Explicit braces everywhere
- 4-space indentation
- No LINQ in hot loops (fast-path, mutators, attack generation)
- Immutable `GameState` snapshots and deterministic branching

Deviations must be justified inline and queued for cleanup.

## Acceptance Criteria Met

- Parity V2 suite green (447 sliding tests + legacy/compiled alignment)
- Benchmarks show ≥4.6× improvement (empty rook paths) with no correctness regression
- Clear fallback behavior + metrics instrumentation

## Pending Validation

- >64 tile guard test to be added (ensures graceful skip)

Last updated: 2025-09-24
