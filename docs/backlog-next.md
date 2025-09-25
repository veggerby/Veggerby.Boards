# Backlog (Next Focus Slice)

Status Date: 2025-09-25
Origin: Extracted from `plans/action-plan.md` + `backlog-extra.md` to keep the strategic plan lean.
Scope: Only items not yet delivered and required before graduating DecisionPlan + Sliding Fast-Path + Compiled Patterns to default stable status.

## Legend

COMPLETED (in main plan) items removed. This file tracks ACTIVE + PENDING work only.

## 1. Rule Evaluation Engine Modernization (FINALIZED)

- [x] Static exclusivity inference (attribute-driven) feeding mask table (M2) – implemented with precedence (builder > phase definition > attribute). Flag-gated under `EnableDecisionPlanMasks`; parity unaffected.
- [x] Grouping invalid-gate negative tests (ensure skipped entries unaffected). (Implemented: `DecisionPlanGroupingTests.GivenGroupedFalseGate_WhenGroupingEnabled_ThenOnlyNonGroupedConditionEvaluatesIndependently` – semantics clarified.)
- [x] EventKind filtering benchmark variants (heterogeneous inert state event + 50/50, 80/20, 20/80 distributions + evaluation counts). Allocation commentary captured; style charter reaffirmed.
- [x] EventKind filtering allocation & distribution follow-up (metrics captured 2025-09-25; perf note updated).
- [x] Debug parity overhead microbenchmark – `DebugParityOverheadBenchmark` measuring dual-run cost.
- [x] Observer skip reason taxonomy (OnRuleSkipped + RuleSkipReason: EventKindFiltered, ExclusivityMasked, GroupGateFailed). Follow-up (classification of invalid vs ignored vs masked nuance + composite skip capture tests) deferred to Observability backlog (migrated below) – core modernization considered complete.

Outcome: Rule evaluation modernization feature set (grouping, event kind filtering, masking, static exclusivity inference, debug parity, observer skip taxonomy) is feature-complete for current release objectives. Any additional observer granularity or masking refinements treated as incremental diagnostics enhancements, not gating engine graduation.

## 2. Deterministic Randomness & State History (FINALIZED)

- [x] Replay determinism internal acceptance test (seed + event sequence reproduces 64/128-bit hash & seed) – `ReplayDeterminismTests` added.
- [x] Undo/Redo zipper invariant tests (hash stability, idempotent redo after undo chain). (Active: tests enabled using deterministic rook oscillation path; hash + idempotent redo invariants validated.)
- [x] RNG state serialization doc fragment (canonical field ordering: Seed, Peek[0], Peek[1]) added to `rng-and-timeline.md`.

Outcome: Workstream objectives met for milestone (deterministic RNG, dual hashing, zipper invariants, replay acceptance, documented serialization ordering). Deferred enhancements (external reproduction envelope tooling, hash interning map, timeline diff utilities) tracked outside active slice; no further blocking tasks here.

## 3. Movement & Pattern Compilation (CLOSED – Deferred Enhancements Only)

- [ ] Populate compiler for remaining pattern kinds (conditional/leaper/wildcard) – requires semantics charter update.
- [ ] Throughput improvement target (≥5×) – deferred; revisit with topology pruning + heuristic gating.
- [ ] LINQ sweep legacy visitor (ensure no hot loop remains), add analyzer stub (optional).
- [x] Micro-benchmarks per pattern kind (Fixed, Ray, MultiRay) vs visitor (latency + allocations). (Added `CompiledPatternKindsBenchmark` 2025-09-25.)
- [x] Edge-case semantics charter (blocked capture variants, repeat limit rationale) prior to broadening kinds. (Added detailed section to `movement-semantics.md` 2025-09-25.)
- [x] Large-sample randomized parity benchmark & test harness (`PatternResolutionLargeSampleBenchmark`, `RandomizedCompiledPatternParityTests`) – allocation reduction confirmed; latency regression noted and documented.
- [ ] Investigate conditional compilation heuristics (board size / topology) via capability seam (no inline flags).

## 4. Performance Data Layout & Hot Paths

Initiation Emphasis (2025-09-25): Prioritize sliding fast-path blocked/capture microbenchmarks and allocation verification before pursuing new heuristics (per-piece masks, topology pruning). Maintain strict style charter: no LINQ in fast-path loops, allocation-free hit path, explicit braces, deterministic ordering.

- [x] Sliding fast-path microbenchmarks (empty, blocked, capture, off-ray) with allocation probe variant (`FastPathAllocationProbeEmpty`).
- [x] Allocation verification seam (internal `FastPathAllocationProbe`) – benchmark-level GC collection delta guard (soft signal; no hot-path instrumentation).
- [x] Per-piece occupancy masks (pruning heuristic) – behind new `EnablePerPieceMasks` flag (implemented: `BitboardLayout` exposes ordered `Pieces[]`; `BitboardOccupancyIndex` maintains optional mask array rebuilt on snapshot bind; flag default off pending overhead benchmark).
- [ ] Bitboard128 prototype (only once a >64 tile board introduced – defer otherwise).
- [ ] Mobility heuristic prototype using bitboards (popcount span) – optional; feeds future evaluation.
- [x] Mobility heuristic prototype using bitboards (popcount span) – internal `MobilityEvaluator` added (per-player sliding mobility counts via attack rays + occupancy). Deterministic, allocation-light; test added. Future: integrate leaper/pawn semantics, weighting, pruning with per-piece masks.

## 5. Concurrency & Simulation

- [x] Sequential Simulator basic playout loop (policy delegate, stop predicate) – feature-flagged (`EnableSimulation`); added `SequentialSimulator.Run` with deterministic policy loop + safety cap + guard tests. Style charter reaffirmed (explicit braces, no LINQ in loop, immutable state usage).
- [x] Parallel orchestrator (initial) – `ParallelSimulator.RunManyAsync` with bounded degree-of-parallelism, ordered results, cancellation throwing semantics (future partial result enhancement pending).
- [x] Cancellation test (exception path) – validates cancel token triggers `OperationCanceledException` / `TaskCanceledException`.
- [x] Partial results cancellation contract – `ParallelSimulator.RunManyPartialAsync` added (returns subset + flag, no exception path).
- [x] Metrics enrichment phase 1 – `PlayoutMetrics`, `PlayoutBatchDetailedResult`, `SequentialSimulator.RunDetailed`, `ParallelSimulator.RunManyDetailedAsync` (AppliedEvents, PolicyCalls, placeholder RejectedEvents=0, coarse branching factor).
- [x] Placeholder legacy duplication removal (`PlayoutResults.cs`) – file deleted; CHANGELOG entry added.
- [ ] Deterministic multiset equivalence test (parallel vs sequential) – expand to 32-playout hash multiset comparison (current test small sample only). (Post-finalization enhancement)
- [ ] Depth histogram & percentile metrics (P50/P95) – allocate-free buckets in detailed runs only. (Post-finalization enhancement)
- [ ] Accurate rejection counting – extend policy surface or introduce instrumentation wrapper. (Post-finalization enhancement)
- [ ] Refined branching factor metric (policyCalls / appliedEvents; expose both coarse & refined forms). (Post-finalization enhancement)
- [ ] Policy helper expansion (struct-enumerator single-step, seeded randomized variant, composite heuristic chaining). (Post-finalization enhancement)
- [ ] Simulation documentation expansion (`docs/simulation.md`): metrics field glossary, partial cancellation semantics table, determinism/seeding guidance, style charter block. (Post-finalization enhancement)
- [ ] Simulation benchmark harness (baseline vs metrics vs partial) – validate metrics overhead <3% on 256 short playouts. (Post-finalization enhancement)

Finalization Note: Core simulation deliverables (sequential/parallel, phase 1 metrics, partial cancellation) are complete and reflected in the Action Plan (Workstream 5 marked FINALIZED). Remaining items above are incremental, non-blocking enhancements subject to the same style charter (no LINQ in inner loops, immutable state, explicit braces, deterministic execution). Any deviation requires `// STYLE-DEVIATION:` annotation + CHANGELOG entry.

## 6. Observability & Diagnostics

- [ ] CLI trace viewer (consume JSON trace file, colorize rule outcomes). **(DEFERRED – scope reduced this increment)**
- [x] Trace overhead benchmark gating (fail if >5% HandleEvent p50 delta). **(COMPLETED – `TraceObserverBatchOverheadBenchmark` added; provides multi-scenario data to enforce ≤5% small-plan delta)**
- [x] Observer callback batching adapter (reduce overhead in high-frequency scenarios). **(COMPLETED – feature flag `EnableObserverBatching`, benchmark variant + ordering test added)**

## 7. Developer Experience & Quality Gates

- [ ] Property test expansion (captures, blocked moves, multi-step paths, dice modules).
- [ ] CI benchmark regression gate (JSON baseline compare, threshold config).
- [ ] Roslyn analyzers: forbidden `System.Random`, mutable state mutation, FeatureFlags usage outside scopes.
- [ ] Analyzer tests (happy/diagnostic/fix). (Fixers optional initially.)

## 8. Public API Facade (Deferred)

Removed from active scope. Re-imagining (HTTP transport, DTO versioning policy) will occur under a future dedicated plan; no tasks tracked here.

## 9. Structural Refactors

- [ ] Replace residual LINQ in performance-sensitive paths (post profiling report).
- [ ] Record struct wrappers (`TileIndex`, `PlayerId`) – minimal; ensure no boxing.
- [ ] Hidden global analyzer (ensure only FeatureFlags used under controlled scope helper).

## Exit Gates (Before Legacy Removal)

- DecisionPlan: Static exclusivity inference + overhead benchmarks complete; parity + performance acceptable.
- Sliding Fast-Path: Blocked/capture microbenchmarks show ≥1.5× at half density; allocation-free guarantee enforced.
- Compiled Patterns: 5× resolution throughput vs visitor achieved & measured.
- Observer: Overhead ≤5% with callback batching OFF (baseline) and documented.
- Hashing: 128-bit path validated in parity suites (randomized + curated) cross-platform.

## Hygiene Tasks

- [ ] Normalize markdown heading spacing (lint clean pass after doc additions).
- [x] Update CHANGELOG per milestone merges (grouping test rename, event kind benchmark repair + mixed evaluation counts, timeline undo/redo invariants now ACTIVE, exclusivity inference, observer taxonomy, parity overhead benchmark).
- [x] Validate all new feature flags documented in configuration doc. (Completed 2025-09-25 – `configuration.md` updated with Timeline zipper validation note.)

---
Style Charter Reiteration (COMPLETED EMPHASIS): All new tests & benchmarks must honor core rules (file-scoped namespaces, explicit braces, deterministic outcomes, no LINQ in hot loops/benchmarks critical path, immutability). Any deviation requires justification inline. Style emphasis block propagated to `backlog-extra.md` and `action-plan.md`. Source of authority: `.github/copilot-instructions.md` (sections 2–5, 13) – changes must not contradict those invariants.

---
This backlog intentionally lean. Items graduating into "Completed" will be pruned here and reflected only in the historical action plan revision notes.
