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

## 2. Deterministic Randomness & State History

- [ ] Replay harness (external tool spec stub; internal test: seed + event sequence reproduces hash).
- [x] Undo/Redo zipper invariant tests (hash stability, idempotent redo after undo chain). (Active: tests enabled using deterministic rook oscillation path; hash + idempotent redo invariants validated.)
- [ ] RNG state serialization doc fragment (canonical field ordering confirmation).

## 3. Movement & Pattern Compilation

- [ ] Populate compiler for remaining pattern kinds (repeat expansions beyond current subset).
- [ ] 5× parity & perf gate: finalize benchmark measuring visitor vs compiled across 1k random samples.
- [ ] LINQ sweep legacy visitor (ensure no hot loop remains), add analyzer stub (optional).

## 4. Performance Data Layout & Hot Paths

- [ ] Sliding fast-path blocked/capture microbenchmarks (density tiers: empty, sparse, half, dense).
- [ ] Blocked/capture parity already green – add allocation assertions (fast-path hit must allocate 0).
- [ ] Per-piece occupancy masks (pruning heuristic) – behind new `EnablePerPieceMasks` flag.
- [ ] Bitboard128 prototype (only once a >64 tile board introduced – defer otherwise).
- [ ] Mobility heuristic prototype using bitboards (popcount span) – optional; feeds future evaluation.

## 5. Concurrency & Simulation

- [ ] Sequential Simulator basic playout loop (policy delegate, stop predicate) – feature-flagged.
- [ ] Parallel orchestrator (deterministic seeding scheme) – soak tests.
- [ ] Playout metrics (histogram P50/P95, branching factor capture, event counts).
- [ ] Cancellation & partial results contract tests.

## 6. Observability & Diagnostics

- [ ] CLI trace viewer (consume JSON trace file, colorize rule outcomes).
- [ ] Trace overhead benchmark gating (fail if >5% HandleEvent p50 delta).
- [ ] Observer callback batching adapter (reduce overhead in high-frequency scenarios).

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
