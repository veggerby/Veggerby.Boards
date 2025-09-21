# 2025-09-21 Strategic Architecture & DX Action Plan

> Revision Note (2025-09-21, branch `feat/architecture-and-dx`): Initial implementation landed for feature flags, DecisionPlan (parity mode), deterministic RNG scaffold, and documentation skeletons. Remaining items annotated below with status.

## Executive Summary

This plan operationalizes the 15+ architectural and developer experience upgrades outlined in `docs/plans/roadmap.md`. It groups them into coherent workstreams, defines phased delivery (Foundations → Acceleration → Observability & Tooling → Hardening & Ecosystem), and specifies deliverables, acceptance criteria, success metrics, risk mitigations, and migration safety for existing modules (Chess, Backgammon) and API consumers.

## Progress Status (2025-09-21 – branch `feat/architecture-and-dx`)

| Workstream | High-Level Status | Notes |
|------------|-------------------|-------|
| 1. Rule Evaluation Engine Modernization | PARTIAL | DecisionPlan parity path + flag merged; EventResult placeholder added; observer + perf targets pending |
| 2. Deterministic Randomness & State History | PARTIAL | `IRandomSource`, `XorShiftRandomSource`, `GameState.Random` implemented; timeline, hashing, BugReport not started |
| 3. Movement & Pattern Engine Compilation | NOT STARTED | No IR / compiler code yet |
| 4. Performance Data Layout & Hot Paths | NOT STARTED | No BoardShape / PieceMap / bitboards work begun |
| 5. Concurrency & Simulation | NOT STARTED | Simulator API not started |
| 6. Observability & Diagnostics | NOT STARTED | No observer interface / trace emitter yet |
| 7. Developer Experience & Quality Gates | PARTIAL | Baseline benchmark + property test scaffold; invariants & perf CI gate pending |
| 8. Module & API Versioning Strategy | NOT STARTED | No versioned DTOs yet |
| 9. Small Structural Refactors | NOT STARTED | Refactors/de-analyzers not started |

Legend: COMPLETED / PARTIAL / NOT STARTED (scope as defined in this plan).

## Guiding Principles

- Determinism and Immutability are non‑negotiable invariants.
- Performance improvements must be measurable (benchmarks) and opt‑in behind stable abstractions until proven.
- Backwards compatibility for public APIs and module builders (no breaking changes without versioned surfaces).
- Each phase ends with: Green tests, Updated docs, Benchmarks captured, Changelog entry.

## Workstream Overview

1. Rule Evaluation Engine Modernization (Items 1, 7, 9)
2. Deterministic Randomness & State History Evolution (Items 2, 3, 14)
3. Movement & Pattern Engine Compilation (Item 4, 10 partial)
4. Performance Data Layout & Hot Path Optimization (Items 5, small refactors, bitboards (10))
5. Concurrency & Simulation (Item 6)
6. Observability & Diagnostics (Items 9, 12, 13)
7. Developer Experience & Quality Gates (Items 11, 13, 15 + small refactors)
8. Module & API Versioning Strategy (Item 8)
9. Small Structural Refactors (LINQ removal, record structs, adjacency caches, no hidden globals)

## Phase Timeline

| Phase | Focus | Key Workstreams | Exit Criteria |
|-------|-------|-----------------|---------------|
| Phase 1 Foundations | Deterministic Core & Plan Skeleton | 1,2,3 (initial), 7 (typedef scaffolding) | DecisionPlan prototype; IRandomSource integrated; Zipper state model behind feature flag; tests green |
| Phase 2 Acceleration | Performance & Pattern Compilation | 3 (DFA complete), 4, 5 (sim proto), 10 (internal bitboards chess) | Benchmarks show >=5x faster path resolve; simulator runs parallel playouts deterministically |
| Phase 3 Observability & DX | Traces, Visualizer, Property Tests | 6, 9 (observer hooks), 11,12,13 | Trace UI/MVP; invariants passing 100+ FsCheck cases; perf guardrail CI failing on regression |
| Phase 4 Hardening & Ecosystem | Versioning & Stability | 8,15, refactors finalize | API v1 stable; extension point docs; migration guide published |

## Sequencing Refinements (Post Review)

The external architectural review recommends tightening early phase scope and pulling minimal observability forward. Adjusted high-level plan:

| Phase | Duration (target) | Added / Emphasized | Deferred from Earlier Draft |
|-------|-------------------|--------------------|-----------------------------|
| Phase 1 Foundations | 1–2 weeks | Minimal DecisionPlan, IRandomSource, no-op IEvaluationObserver, baseline benchmarks, FsCheck scaffold | Short-circuit masks, advanced predicate hoisting |
| Phase 2 Acceleration | 2–3 weeks | Pattern compiler (subset: sliders, knights, basic forward strides), SoA mirrors, one short-circuit optimization, optional chess bitboards (flag) | Full DFA for exotic patterns |
| Phase 3 Observability & Simulation | 1–2 weeks | JSON trace emitter, CLI viewer MVP, Simulator (sequential + parallel), Merkle hash + BugReport replay | Full web visualizer UI |
| Phase 4 Hardening & Ecosystem | ~1 week | Versioned DTOs, package split, analyzers for hidden globals, migration docs | Any experimental feature graduation pending metrics |

Rationale:

- Early instrumentation (observer) accelerates validation of subsequent optimizations.
- Narrow initial DFA scope reduces compiler complexity risk.
- Simulator deferred until after core speedups to avoid benchmarking unstable baselines.

## Scope Guardrails

To mitigate overreach and maintain momentum:

- DecisionPlan v1: fixed rule order, array iteration only (no multi-level mask layering yet).
- Pattern compiler v1: only (a) fixed-step, (b) repeat-until-blocked (sliders), (c) single jump (knight). Defer conditional / compound patterns.
- Bitboards: chess module internal adapter; abort if <15% net gain on attack generation after two profiling runs.
- Merkle hash: non-crypto xxHash128/BLAKE3-128 equivalent; stable canonical serialization (little-endian, sorted deterministic field ordering).
- Dual-engine parity: legacy evaluator retained only inside test compilation (compile symbol) for golden comparisons—never shipped.

## De-Scoped (Initial Delivery)

- Advanced DecisionPlan optimizations (predicate trees, multi-pass hoisting) until baseline perf metrics recorded.
- Full graphical/web visualizer—CLI text/ANSI output is sufficient initially.
- Cryptographic hashing—upgrade only if collision risk becomes material (e.g., transposition table adversarial inputs).
- Comprehensive pattern language features beyond existing module needs.

## Additional Acceptance Criteria

| Category | Criterion |
|----------|-----------|
| Determinism (Cross-Platform) | Same event stream + seed yields identical Merkle hash on Linux x64, Windows x64, and (when available) ARM64 CI agents. |
| Observer Transparency | Enabling observer must not alter any resulting state hash (dual-run assertion test). |
| BugReport Stability | Serialized BugReport round-trips (versioned schema) with semantic equality of: seed, event list, terminal state hash, decision plan version. |
| Performance Gate | Observer enabled adds ≤5% overhead in microbenchmarks. |
| Fallback Safety | Disabling compiled patterns or DecisionPlan (feature flag) reverts to legacy path with identical test outcomes. |

## Feature Toggles & Flags

| Flag | Purpose | Default | Removal Condition |
|------|---------|---------|--------------------|
| LEGACY_RULE_ENGINE (compile symbol) | Enables legacy evaluator in tests for parity checks | On (tests only) | After two successive minor versions with zero diffs |
| EnableCompiledPatterns | Switch between visitor and DFA engine | Off (Phase 1), On (post Phase 2 validation) | After parity + perf threshold met |
| EnableBitboards | Use chess bitboard adapter | Off | If sustained >15% attack gen speedup across 3 benchmarks |
| EnableStateHashing | Compute Merkle hash each transition | Off (Phase 1) | Always on after Phase 3 stability |
| EnableTraceCapture | Persist JSON trace of last evaluation | Off | Always on (behind observer presence) once overhead validated |

## Updated Metrics Targets (Consolidated)

| Metric | Phase 1 Target | Phase 2 Target | Phase 3 Target |
|--------|----------------|----------------|----------------|
| HandleEvent p50 Latency | -30% vs baseline | -60% vs baseline | Maintain -60% |
| Path Resolution Ops/sec | +2× | +5× | Maintain +5× |
| Allocations / HandleEvent | -20% | -70% | Maintain -70% |
| Deterministic Replay Divergence | 0 | 0 | 0 |
| Observer Overhead | ≤5% | ≤5% | ≤5% |
| Invariant Failure Rate | <0.5% (early) | <0.1% | <0.1% |

## Implementation Touch-Points

| Area | Files / Components (Indicative) | Change Summary |
|------|---------------------------------|----------------|
| Compilation Pipeline | `GameBuilder.Compile()` | Emit DecisionPlan, pattern DFAs, SoA mirrors, register flags |
| Event Handling | `GameEngine.HandleEvent(...)` | Replace dynamic rule evaluation with plan executor; adapt to EventResult |
| Pattern System | Pattern interfaces & visitors | Introduce IR + DFA compiler; retain existing visitors as fallback |
| RNG Integration | `GameState`, Backgammon dice mutators | Thread IRandomSource state snapshot through transitions |
| History Model | New `GameTimeline` | Zipper structure with undo/redo & hash capture (flagged) |
| Observability | New `IEvaluationObserver` | No-op default; conditional callbacks in hot paths |
| Simulation | New `Simulator` namespace | Parallel playout orchestration (Phase 3) |
| Hashing | New hashing utility | Canonical serialization + xxHash128/BLAKE3-128 wrapper |
| Benchmarks | New `/benchmarks` project | Baseline + comparative benchmarks for perf gates |
| Property Tests | New `/test/Veggerby.Boards.PropertyTests` | FsCheck invariants harness |

## Cross-Platform Determinism Strategy

1. Define canonical serialization order (artifact IDs ascending; piece positions by piece ID; dice states stable order; RNG state bytes appended last).
2. Use fixed little-endian encoding for integers; avoid `BitConverter` defaults without specifying endianness.
3. Integration test matrix comparing hashes across runners (GitHub Actions: ubuntu-latest, windows-latest; optional ARM64 self-host runner later).
4. Fail CI if any cross-platform drift detected (store baseline hash set for consensus).

## Parity Testing Approach

- Dual execution tests: run event sequence through legacy evaluator and DecisionPlan; assert identical successor state hashes + serialized piece placements.
- Same for pattern resolver: visitor vs compiled DFA across randomized legal move samples.
- Provide a diff helper that enumerates first divergent rule decision for rapid debugging.

## Review Feedback Summary (Traceability)

| Review Suggestion | Incorporated Section |
|-------------------|----------------------|
| Pull observer into Phase 1 | Sequencing Refinements / Phase 1 description |
| Limit initial DFA scope | Scope Guardrails (Pattern compiler v1) |
| Guard against complexity creep | Scope Guardrails / De-Scoped |
| Add cross-platform determinism | Additional Acceptance Criteria / Cross-Platform Determinism Strategy |
| Dual-engine parity only in tests | Scope Guardrails / Feature Toggles |
| Abort bitboards if low gain | Scope Guardrails (Bitboards) |
| Add schema stability for BugReport | Additional Acceptance Criteria |
| Provide clear feature flags | Feature Toggles & Flags |
| Expand metrics with phased targets | Updated Metrics Targets |

## Detailed Action Items

### 1. Rule Evaluation Engine Modernization

Deliverables (Status annotations in brackets):

- `DecisionPlan` immutable model: phases, rule table, pre-bound predicates, mutator delegate array. **[COMPLETED (parity subset – minimal model, no delegate table yet)]**
- Compiler: `DecisionPlanBuilder.Compile(GameBuilderContext ctx)`. **[COMPLETED (integrated into builder; context abstraction simplified)]**
- Execution path: `GameEngine.HandleEvent` uses precomputed plan. **[COMPLETED (flag-gated parity path)]**
- Typed result: `EventResult` discriminated union. **[COMPLETED (placeholder, not yet returned publicly)]**
- Observer integration points for rule evaluation events. **[NOT STARTED]**
Acceptance Criteria:
- No functional behavior change vs legacy path (golden test suite). **[PENDING – parity tests not yet added]**
- Benchmark: `HandleEvent` median latency reduced ≥30% on sample scenarios (Chess opening moves, Backgammon entry moves). **[PENDING – only baseline harness exists]**
- Trace includes rule index + failing predicate reason. **[NOT STARTED – tracing/observer missing]**
Risks & Mitigation:
- Complexity creep: keep plan structure minimal (arrays + bitsets). Stage features (start w/out short-circuit masks, add later).
- Debug difficulty: include verbose validator to cross-check results in tests.
Migration Strategy:
- Keep legacy evaluator for one release hidden behind `#if LEGACY_RULE_ENGINE` or configuration for A/B verification.

### 2. Deterministic Randomness & State History Evolution

Deliverables (Status annotations in brackets):

- `IRandomSource` + `XorShiftRandomSource` implementation (seed + serializable state). **[COMPLETED]**
- GameState includes RNG snapshot (struct) + `WithRandom(IRandomSource)` cloning. **[COMPLETED]**
- Persistent zipper model: `GameTimeline { ImmutableStack<GameState> Past; GameState Present; ImmutableStack<GameState> Future; }`. **[NOT STARTED]**
- Merkle hash: deterministic hash over artifact ids, piece positions, dice values, RNG state. **[NOT STARTED]**
- `BugReport` envelope capturing seed, event stream, decision plan version. **[NOT STARTED]**
Acceptance Criteria:
- Replaying same seed + events yields identical final hash and RNG state. **[PENDING – hashing & replay infra not implemented]**
- Undo/Redo operations O(1) and hash-stable. **[NOT STARTED]**
- BugReport replay test harness passes sample captured report. **[NOT STARTED]**
Risks:
- Hash collisions (mitigate with 128-bit hash like xxHash128 or Blake2b incremental).
- State size growth (dedupe identical state nodes via hash interning map optional in Phase 3).
Migration:
- Hash initially optional; enable via feature toggle in builder.

### 3. Movement & Pattern Engine Compilation

Deliverables (Status annotations in brackets):

- Pattern IR: normalized representation (directions, repeats, terminals). **[NOT STARTED]**
- Compiler: pattern set per piece → DFA/NFA structure (arrays of transitions, acceptance flags). **[NOT STARTED]**
- Runtime: path resolution via integer indices + bitsets, no interface dispatch in hot loop. **[NOT STARTED]**
- Fallback to existing visitor system for non-compiled patterns until parity confirmed. **[NOT STARTED]**
Acceptance Criteria:
- All existing movement tests green under compiled engine. **[NOT STARTED]**
- Benchmark: pattern resolution ≥5x faster on 1000 random moves sample. **[NOT STARTED]**
Risks:
- Over-optimizing rare patterns; limit scope to current patterns first.
Migration:
- `CompilePatterns()` invoked during game build; toggle to disable for troubleshooting.

### 4. Performance Data Layout & Hot Path Optimization

Deliverables (Status annotations in brackets):

- Internal `BoardShape` (tile adjacency arrays, directional lookup table). **[NOT STARTED]**
- `PieceMap` struct-of-arrays (ids, tile indices, owner indices). **[NOT STARTED]**
- Replace LINQ enumerations in path & rule evaluation with for loops. **[NOT STARTED]**
- Identify micro hotspots via BenchmarkDotNet baseline. **[PARTIAL – baseline harness exists, analysis pending]**
- Chess bitboard adapter (64-bit masks for occupancy, attacks) synced at evaluation entry. **[NOT STARTED]**
Acceptance Criteria:
- Benchmarks: resolve path & legal move generation 10–30× faster (target upper bound, accept ≥8× initial). **[NOT STARTED]**
- Allocation count in hot benchmarks < 5% of baseline. **[NOT STARTED]**
Risks:
- Bitboard sync overhead > savings (measure early).
Migration:
- Keep bitboard logic internal; no public exposure.

### 5. Concurrency & Simulation

Deliverables (Status annotations in brackets):

- `Simulator` API (pure playout loop) with cancellation token. **[NOT STARTED]**
- Policy delegate signature: `Func<GameState, IGameEvent?>`. **[NOT STARTED]**
- Parallel playout orchestrator using `Task` or `Parallel.ForEachAsync` without shared mutation. **[NOT STARTED]**
- Determinism guarantee doc (ordering independent). **[NOT STARTED]**
Acceptance Criteria:
- Running N playouts sequentially vs parallel yields identical multiset of final state hashes (when seeds derived deterministically per playout index). **[NOT STARTED]**
- Handles cancellation gracefully (partial results surfaced). **[NOT STARTED]**
Risks:
- Contention on shared resources (avoid globals, pre-clone plan/read-only data).
Migration:
- Ship as experimental namespace `Simulation` initially.

### 6. Observability & Diagnostics

Deliverables (Status annotations in brackets):

- `IEvaluationObserver` minimal v1 (RuleEvaluated, RuleApplied, EventIgnored) implemented + no-op default + builder injection. **[COMPLETED]**
- PhaseEnter callback emitted (legacy + DecisionPlan) **[COMPLETED (StateHashed still pending)]**
- StateHashed callback **[NOT STARTED]**
- Decision trace serializer (compact JSON) for last evaluation. **[NOT STARTED]**
- CLI or lightweight web visualizer (Phase 3) reading trace JSON. **[NOT STARTED]**
Acceptance Criteria:
- Observer adds ≤5% overhead when enabled in benchmark microtests. **[PENDING – instrumentation not benchmarked]**
- Trace includes minimal fields to reproduce reasoning (state hash, rule id, reason enum). **[NOT STARTED]**
Risks:
- Performance drag; mitigate with aggressive inlining and early branch-out when no observers.
Migration:
- Start with synchronous callbacks; consider batching later if needed.

### 7. Developer Experience & Quality Gates

Deliverables (Status annotations in brackets):

- Property tests (FsCheck) for listed invariants. **[PARTIAL – scaffold + minimal tests]**
- Benchmark suite (BenchmarkDotNet) in `/benchmarks` project. **[COMPLETED (baseline harness present)]**
- CI workflow: run benchmarks on PR; compare against stored baseline JSON; fail on >2% regression. **[NOT STARTED]**
- Documentation for extension points & stability guarantees. **[PARTIAL – new feature docs only]**
Acceptance Criteria:
- Invariants >99% pass rate across 10k generated cases each. **[NOT STARTED – limited invariants]**
- Regression workflow reliably detects synthetic slowdown. **[NOT STARTED]**
Risks:
- Flaky benchmarks (mitigate with statistical filtering, dedicated CI runner settings).
Migration:
- Property tests can be quarantined initially if unstable.

### 8. Module & API Versioning Strategy

Deliverables (Status annotations in brackets):

- Separate package metadata for Chess/Backgammon (csproj adjustments, semantic version scheme). **[NOT STARTED]**
- Versioned DTO namespaces: `Veggerby.Boards.Api.V1.Models`. **[NOT STARTED]**
- Changelog & migration guide for introducing v1. **[NOT STARTED]**
Acceptance Criteria:
- Existing clients (if any) compile unchanged; new versioned namespace exposed. **[NOT STARTED]**
- Pack step produces distinct nupkgs. **[NOT STARTED]**
Risks:
- Namespace churn confusion; emphasize docs.
Migration:
- Provide type forwarders temporarily if needed.

### 9. Small Structural Refactors

Deliverables (Status annotations in brackets):

- Replace LINQ in hot spots (profile-guided list). **[NOT STARTED]**
- Introduce `record struct` wrappers: `TileIndex`, `PlayerId` (implicit conversions avoided for clarity). **[NOT STARTED]**
- Adjacency cache keyed by board hash (dictionary or ConcurrentDictionary) with size cap. **[NOT STARTED]**
- Audit for hidden globals; enforce analyzer rule if necessary. **[NOT STARTED]**
Acceptance Criteria:
- Analyzer or code review checklist updated. **[NOT STARTED]**
- GC pressure reduced (verify via allocation benchmarks). **[NOT STARTED]**
Risks:
- Over-abstraction; keep wrappers tiny and well-documented.

## Cross-Cutting Concerns

Documentation: Update `docs/` per feature (new md pages for Decision Plan, RNG, Timeline/Zipper, Observer, Simulation, Benchmarks).
Testing: Golden master tests to ensure parity between legacy and new compiled engines until removal.
Security: No external dependencies for cryptographic hashing unless necessary (prefer managed Blake2b implementation or xxHash).
Versioning: Each phase increments minor version; breaking changes require major bump plus migration notes.

## Metrics Dashboard (Targets)

- HandleEvent p50 latency: -30% Phase 1, -60% Phase 2 vs baseline.
- Path resolution ops/sec: +5× Phase 2.
- Allocation per HandleEvent: -70% Phase 2.
- Deterministic replay divergence: 0 incidents in CI after Phase 2.
- Property test invariant failures: <0.1%.

## Risk Register (Top 5)

| Risk | Impact | Likelihood | Mitigation |
|------|--------|-----------|------------|
| Performance regressions from instrumentation | Medium | Medium | Feature flags, benchmarks in PR gate |
| DFA compiler complexity / bugs | High | Medium | Start with minimal subset, golden tests |
| Hash collision / incorrect equality assumptions | High | Low | 128-bit hash, include type tags |
| Parallel simulation nondeterminism | Medium | Medium | Strict seeding scheme, no shared mutable state |
| Versioning confusion for DTOs | Medium | Medium | Clear docs, type forwarders |

## Acceptance Checklist Per Feature

- Tests added/updated
- Benchmarks updated (if performance-related)
- XML docs + docs/ updated
- Changelog entry
- Feature toggle (if experimental) documented
- No style/analyzer warnings

## Sequencing Dependencies

- DecisionPlan before Observer (observer hooks rely on stable plan indexes).
- RNG integration before BugReport & Simulation.
- Pattern DFA before large perf refactors (so measurements reflect future model).
- Bitboards after pattern compilation (reuses adjacency baseline).

## Migration & Deprecation Policy

- Maintain legacy evaluators/pattern resolution for at least one minor version with dual-run validation in tests.
- Mark legacy APIs with `[Obsolete("Use new DecisionPlan engine")]` only after parity proven.

## Initial Backlog (Epics → Stories)

1. EPIC: DecisionPlan Engine
   - Spike: minimal plan with fixed rule list execution
   - Story: condition pre-binding
   - Story: mutator delegate table
   - Story: typed EventResult + reasons
   - Story: golden parity tests legacy vs plan
2. EPIC: Deterministic RNG & Timeline
   - Spike: IRandomSource interface + xoroshiro impl
   - Story: embed rng state in GameState
   - Story: zipper structure with undo/redo tests
   - Story: merkle hash function & tests
   - Story: bug report capture + replay
3. EPIC: Pattern Compilation
   - Spike: IR representation
   - Story: direction adjacency bitsets
   - Story: repeat pattern expansion
   - Story: DFA executor + benchmarks
   - Story: parity tests vs visitor
4. EPIC: Performance Layout
   - Spike: BoardShape arrays
   - Story: PieceMap structure
   - Story: replace LINQ in path evaluation
   - Story: chess bitboard adapter
5. EPIC: Simulation
   - Spike: basic sequential playout loop
   - Story: parallel orchestration
   - Story: deterministic seeding scheme
6. EPIC: Observability & Visualizer
   - Story: IEvaluationObserver interface
   - Story: JSON trace emitter
   - Story: CLI visualizer MVP
7. EPIC: Property Tests & Benchmarks
   - Story: FsCheck project setup
   - Story: invariants suite
   - Story: benchmark harness + CI gate
8. EPIC: Versioning & Packaging
   - Story: split module csproj packaging metadata
   - Story: API DTO v1 namespace
   - Story: publish pipeline updates
9. EPIC: Structural Refactors
   - Story: record structs adoption
   - Story: adjacency cache keyed by hash
   - Story: analyzer for hidden globals

## Deliverables Summary

This action plan yields: faster deterministic evaluation, richer diagnostics, replayable bug reports, a compiled pattern engine, parallel simulation scaffolding, stable extension contracts, and hardened performance guardrails.

## Next Immediate Steps

1. Capture current performance baselines (pre-DecisionPlan) in a new benchmark project.
2. Implement DecisionPlan spike with parity tests.
3. Introduce IRandomSource and integrate into GameState (no behavior change yet).

-- End of Plan --
