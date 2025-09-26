# Engine Configuration & Feature Flags

This document describes the internal runtime feature flags controlling experimental subsystems. All flags live in the internal static class `Internal.FeatureFlags` and are **not persisted** with game state; a resumed session must explicitly reapply any desired toggles prior to evaluation.

## Philosophy

- Determinism First: New performance paths ship behind flags until parity & invariants are proven.
- Explicit Opt-In: Except for features that pass full parity + benchmark acceptance (currently compiled patterns, bitboards + sliding fast-path for ≤64 tiles) defaults remain `false`.
- Transience: Flags are short‑lived migration tools. Mature subsystems graduate; their flags are removed in a subsequent minor version after two stable releases.

## Current Flags (2025-09-25)

| Flag | Default | Purpose | Graduation Criteria | Notes |
|------|---------|---------|---------------------|-------|
| EnableCompiledPatterns | true | DFA/IR based movement pattern resolution with legacy visitor fallback. | Sustained perf win & no unresolved parity gaps for two releases. | Visitor automatically used per-path when resolver misses. |
| EnableBitboards | true | Incremental bitboard occupancy + sliding attack generator foundation. | Net ≥15% improvement on sliding-heavy scenarios & parity across blocker/capture suites. | Auto-skipped for boards >64 tiles. |
| EnableStateHashing | false | Deterministic 64/128-bit (xxHash128) state hash each transition. | Downstream consumers (replay / transposition) validated for cost. | Observer `OnStateHashed` fired when on. |
| EnableTraceCapture | false | Capture last evaluation trace (phase enters, rule decisions, state hash). | Overhead ≤5% with tracing on; stable schema documented. | Trace cleared each event. |
| EnableTimelineZipper | false | Immutable undo/redo chain (past/present/future) for state history (now validated by active undo/redo invariant tests). | Replay / branching algorithms integrate; memory profile stable. | Not yet integrated with simulator. |
| EnableDecisionPlanGrouping | false | Evaluate identical predicate groups once (predicate hoisting). | Benchmark shows measurable reduction in predicate calls. | Depends on EnableDecisionPlan. |
| EnableDecisionPlanEventFiltering | false | Skip plan entries whose declared EventKind does not match current event. | Filtering coverage >80% of rules + stable perf win. | Depends on EnableDecisionPlan. |
| EnableCompiledPatternsAdjacencyCache | false | Precomputed (tile,direction)->neighbor cache for compiled resolver. | Confirmed micro-benchmark win (≥5%) w/out allocation regressions. | Mutually exclusive benefit with BoardShape fast path once enabled. |
| EnableDecisionPlanMasks | false | Short-circuit skip of mutually exclusive phases after first success. | Exclusive grouping correctness + perf proven. | Always evaluated within DecisionPlan (core path); flag only controls masking optimization. |
| EnableBoardShape | false | Prefer `BoardShape` O(1) neighbor lookups over relation scans / adjacency cache lookups. | Board topology heuristics integrated + microbench win. | Always built; flag controls exploitation. |
| EnableSlidingFastPath | true | Sliding (rook/bishop/queen) geometric fast-path using bitboards + attack generator + path reconstruction. | Already met: Parity V2 + benchmarks (≥4.6× empty, ≥2.4× quarter, ≥1.5× half vs compiled). | Requires EnableBitboards; metrics expose granular skip reasons. |

## Usage Pattern

### Typical Opt-In (Acceleration)

```csharp
// Compiled patterns are ON by default.
Internal.FeatureFlags.EnableBitboards = true;          // (default ON ≤64 tiles)
Internal.FeatureFlags.EnableSlidingFastPath = true;    // (default ON)
Internal.FeatureFlags.EnableBoardShape = true;         // Experimental topology exploitation
// DecisionPlan evaluator is always active (legacy traversal removed).
```

### DecisionPlan Optimizations (Optional Layers)

```csharp
Internal.FeatureFlags.EnableDecisionPlanGrouping = true;        // Predicate hoisting (optional)
Internal.FeatureFlags.EnableDecisionPlanEventFiltering = true;   // EventKind pre-filter (optional)
Internal.FeatureFlags.EnableDecisionPlanMasks = true;            // Exclusivity short-circuit (optional)
```

### Quick Disable (Bisect / Troubleshooting)

```csharp
Internal.FeatureFlags.EnableSlidingFastPath = false; // disable sliding acceleration
Internal.FeatureFlags.EnableBitboards = false;       // revert to compiled/legacy occupancy path
```

### Code Style Reminder

All acceleration code must follow repository style rules: file-scoped namespaces, explicit braces, 4-space indentation, no LINQ in hot loops (fast-path, attack generation, mutators), immutability of `GameState` snapshots, and deterministic branching only. Any deviation should be justified in code comments and slated for cleanup in backlog.

Disable individual toggles to isolate their contribution in benchmarks.

## Ordering & Dependencies

- DecisionPlan optimization flags (`Grouping`, `EventFiltering`, `Masks`) operate on the always-on evaluator (no base enable flag).
- Bitboard fast-path currently engages inside `GameProgress.ResolvePathCompiledFirst` when **all** of: bitboards enabled, sliding attack generator service present, piece map snapshot present, and board size ≤64. See also: [`board-vs-bitboard.md`](./board-vs-bitboard.md) for architectural division of responsibility.
- BoardShape is always constructed but only prioritized when `EnableBoardShape` is set (future resolvers will branch on this).

## Benchmarking Guidance

1. Establish a baseline: all experimental flags off except `EnableCompiledPatterns`.
2. Toggle one flag at a time; record mean/median + allocation deltas.
3. Use `FastPathMetrics.Snapshot()` (internal; test assembly accessible) to capture Attempt/Hit distribution and granular skip reasons (NoServices, NotSlider, AttackMiss, ReconstructFail) for sliding queries pre/post bitboards.
4. If a flag does not yield its acceptance threshold (see table) across three consecutive runs (different random seeds), defer graduation.

## Graduation Process

1. Expand parity tests to cover edge cases (blocked rays, captures, exclusivity overlaps).
2. Add microbenchmark proving sustained win (commit baseline artifact).
3. Flip default to true in a minor version; retain flag for one more minor.
4. Remove flag (and dead branches) in next minor after zero regressions.

## Anti-Patterns

- Do NOT change feature flags mid-event evaluation (nondeterministic behavior). Configure once at process start.
- Avoid relying on flags for long-term customization; they are transitional.
- Do not serialize flag states with saved games (restoring with different toggles is permitted and should still produce deterministic valid transitions from that state).

## Future Consolidation

Mature acceleration features (compiled patterns, board shape, bitboards) may be collapsed into a single `PerformanceProfile` enumeration (e.g., `Baseline`, `Accelerated`, `Max`) once their interactions stabilize, simplifying external configuration.

---

Last updated: 2025-09-26 (DecisionPlan graduated; legacy traversal & debug parity flag removed)
