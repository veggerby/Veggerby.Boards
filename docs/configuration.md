# Engine Configuration & Feature Flags

This document describes the internal runtime feature flags controlling experimental subsystems. All flags live in the internal static class `Internal.FeatureFlags` and are **not persisted** with game state; a resumed session must explicitly reapply any desired toggles prior to evaluation.

## Philosophy

- Determinism First: New performance paths ship behind flags until parity & invariants are proven.
- Explicit Opt-In: Except for features that pass full parity + benchmark acceptance (currently compiled movement patterns), defaults remain `false`.
- Transience: Flags are short‑lived migration tools. Mature subsystems graduate; their flags are removed in a subsequent minor version after two stable releases.

## Current Flags (2025-09-24)

| Flag | Default | Purpose | Graduation Criteria | Notes |
|------|---------|---------|---------------------|-------|
| EnableDecisionPlan | false | Replace legacy rule traversal with precompiled leaf phase list (parity + optional optimizations). | All optimization stages (grouping, filtering, masks) validated + stable perf gate. | Debug parity & grouping/masks individually flag-gated. |
| EnableCompiledPatterns | true | DFA/IR based movement pattern resolution with legacy visitor fallback. | Sustained perf win & no unresolved parity gaps for two releases. | Visitor automatically used per-path when resolver misses. |
| EnableBitboards | false | Incremental bitboard occupancy + sliding attack generator & path fast‑path. | Net ≥15% improvement on sliding-heavy scenarios & parity across blocker/capture suites. | Guarded by board tile count ≤64. |
| EnableStateHashing | false | Deterministic 64/128-bit (xxHash128) state hash each transition. | Downstream consumers (replay / transposition) validated for cost. | Observer `OnStateHashed` fired when on. |
| EnableTraceCapture | false | Capture last evaluation trace (phase enters, rule decisions, state hash). | Overhead ≤5% with tracing on; stable schema documented. | Trace cleared each event. |
| EnableTimelineZipper | false | Immutable undo/redo chain (past/present/future) for state history. | Replay / branching algorithms integrate; memory profile stable. | Not yet integrated with simulator. |
| EnableDecisionPlanGrouping | false | Evaluate identical predicate groups once (predicate hoisting). | Benchmark shows measurable reduction in predicate calls. | Depends on EnableDecisionPlan. |
| EnableDecisionPlanEventFiltering | false | Skip plan entries whose declared EventKind does not match current event. | Filtering coverage >80% of rules + stable perf win. | Depends on EnableDecisionPlan. |
| EnableDecisionPlanDebugParity | false | Dual-run legacy evaluator for divergence detection. | Zero mismatches across full suite for ≥2 releases. | High overhead; testing only. |
| EnableCompiledPatternsAdjacencyCache | false | Precomputed (tile,direction)->neighbor cache for compiled resolver. | Confirmed micro-benchmark win (≥5%) w/out allocation regressions. | Mutually exclusive benefit with BoardShape fast path once enabled. |
| EnableDecisionPlanMasks | false | Short-circuit skip of mutually exclusive phases after first success. | Exclusive grouping correctness + perf proven. | Depends on EnableDecisionPlan. |
| EnableBoardShape | false | Prefer `BoardShape` O(1) neighbor lookups over relation scans / adjacency cache lookups. | Board topology heuristics integrated + microbench win. | Always built; flag controls exploitation. |
| EnableSlidingFastPath | false | Sliding (rook/bishop/queen) geometric fast-path using bitboards + attack generator + path reconstruction. | Parity V2 (blocked/capture matrix) + benchmark gains published; default ON for ≤64 tiles thereafter. | Requires EnableBitboards; metrics expose granular skip reasons. |

## Usage Pattern

Typical opt-in flow for experimental performance runs:

```csharp
// Enable compiled patterns already ON by default.
Internal.FeatureFlags.EnableBitboards = true;                 // sliding attacks + fast-path
Internal.FeatureFlags.EnableBoardShape = true;                // adjacency lookups
Internal.FeatureFlags.EnableDecisionPlan = true;              // plan executor
Internal.FeatureFlags.EnableDecisionPlanGrouping = true;      // predicate gate hoisting
Internal.FeatureFlags.EnableDecisionPlanEventFiltering = true;// EventKind pre-filter
Internal.FeatureFlags.EnableDecisionPlanMasks = true;         // exclusivity short-circuit
```

Disable individual toggles to isolate their contribution in benchmarks.

## Ordering & Dependencies

- DecisionPlan optimization flags (`Grouping`, `EventFiltering`, `Masks`, `DebugParity`) require `EnableDecisionPlan`.
- Bitboard fast-path currently engages inside `GameProgress.ResolvePathCompiledFirst` when **all** of: bitboards enabled, sliding attack generator service present, piece map snapshot present, and board size ≤64.
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

Last updated: 2025-09-24
