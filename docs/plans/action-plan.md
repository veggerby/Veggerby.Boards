# 2025-09-21 Strategic Architecture & DX Action Plan

> Revision Addendum (2025-09-25, branch `feat/architecture-and-dx`): Completed migration to reentrant thread-safe `Infrastructure.FeatureFlagScope`; legacy non-thread-safe scope removed; guard test added to prevent reintroduction; documentation & changelog updated; style charter (file-scoped namespaces, explicit braces, no LINQ in hot paths, immutability) re-emphasized across new infrastructure and parity code.
>
> Follow-Up Addendum (2025-09-25 later):
>
> - Added grouping invalid-gate negative test (`DecisionPlanGroupingTests.GivenGroupedFalseGate_WhenGroupingEnabled_ThenOnlyNonGroupedConditionEvaluatesIndependently`) clarifying semantics: grouping suppresses duplicate identical-condition re-evaluation only; unrelated subsequent conditions still evaluate.
> - Introduced `EventKindFilteringBenchmark` scaffold (Move burst scenario) – pending allocation counters & mixed hit/miss distributions.
> - Timeline undo/redo invariants tests IMPLEMENTED (deterministic one-step pawn path helper added) validating hash + reference stability across multi-cycle undo/redo.
> - Propagated style charter emphasis blocks to `backlog-next.md` and `backlog-extra.md`; this action plan will carry a consolidated style appendix below for consistency.
> - CHANGELOG updated accordingly (test rename, benchmark scaffold, style emphasis, timeline test pending).
>
> Consolidation (2025-09-25 later): Core Copilot operating guidance extracted to `docs/wip/copilot-action-plan.md` (augmenting `.github/copilot-instructions.md`). This document now defers granular backlog grooming to `backlog-next.md` (new) for remaining forward-looking items; completed or deprecated entries trimmed here to reduce noise.
>
> Addendum (2025-09-25 later still): Heterogeneous EventKind filtering benchmark variants implemented (50/50, 80/20, 20/80 with inert state event) + evaluation count observer integration validated; allocation results captured (see `docs/perf/eventkind-filtering.md`). Style charter reconfirmed—no LINQ introduced in new hot loops; rook oscillation strategy replaced invalid pawn reverse path sequence to maintain deterministic legality.
>
> Backgammon Flow Addendum (2025-09-25 latest): Replaced previously experimental `SelectActivePlayerRule` wrapper with a direct post-roll `SelectStartingPlayerStateMutator` in the opening phase. This assigns active/inactive players immediately after a distinct opening roll without extra rule indirection, simplifying evaluation and reducing allocations. Property tests updated (first-valid doubling + same-turn redouble invariant). CHANGELOG and backlog adjusted; style charter reaffirmed (file-scoped namespaces, explicit braces, no LINQ in hot loops, immutable state transitions). Any further doubling cube multi-turn semantics (ownership-based redoubling gating) tracked under Section 7 granular tasks.
>
> Developer Experience Workstream 7 Addendum (2025-09-25): Added Backgammon negative doubling invariants (pre-active-player attempt no-op, immediate same-turn redouble no-op) and `EventRejectionReason` exhaustiveness guard test. Backlog Section 7 cleaned (duplicate entries removed, progress annotated). CHANGELOG updated with new property test bullet. Style charter re-emphasized: file-scoped namespaces, explicit braces, 4-space indentation, no LINQ in hot loops, immutable and deterministic state transitions. Remaining tasks: chess castling blocked invariant, capture sequence invariant, property test acceptance criteria & feature flag isolation documentation, benchmark JSON schema draft, style enforcement stub, CONTRIBUTING update. These will finalize Workstream 7 prior to marking COMPLETED.
>
> Workstream 7 FINALIZATION (2025-09-25 latest): All non-deferred Developer Experience & Quality Gates granular tasks completed: chess castling blocked + capture invariants, property test acceptance criteria doc, feature flag isolation pattern doc, benchmark JSON schema draft, style enforcement stub, CONTRIBUTING cross-reference, event rejection guard, backlog grooming (monotonic wording removal). Deferred (non-blocking) items: multi-turn doubling sequence (awaiting richer turn semantics), deterministic chess opening helper, remaining path helper adoption sweep. Acceptance criteria met; Section 7 marked FINALIZED in `backlog-next.md`. Style charter reiterated (file-scoped namespaces, explicit braces, 4-space indentation, no LINQ in hot loops, immutable state, deterministic transitions). Any future DX enhancements require explicit CHANGELOG entries and adherence to deviation annotation policy (`// STYLE-DEVIATION:`) if exceptions arise.
>
> Follow-Up (2025-09-25 later): Deterministic chess opening helper implemented (`DeterministicChessOpening` – minimal Ruy Lopez sequence) closing the optional deferred DX item; backlog and changelog updated; style charter reaffirmed (file-scoped namespaces, explicit braces, no hot-path LINQ, immutability, deterministic ordering). Remaining deferred DX items: multi-turn Backgammon doubling (dependent on TurnState), path helper adoption sweep.
>
> Turn Sequencing Scaffolding Addendum (2025-09-25 latest): Introduced shadow-mode turn timeline primitives – `TurnSegment` enum (Start/Main/End), `TurnArtifact` (singleton), and `TurnState` (TurnNumber=1, Segment=Start) injected at compile time for every game. No behavioral change yet (active player logic unchanged). Added `TurnStateScaffoldingTests` asserting presence, and updated `GameBuilderTests` expectations (artifact/state counts). Deterministic chess opening helper now consumed by `DeterministicOpeningTests` (ensuring helper usage). Backlog Section 10 to be updated with Phase 0 completion and upcoming Phase 1 tasks (advancement mutator, segment gating). Style charter reaffirmed: file-scoped namespaces, explicit braces, 4-space indentation, no LINQ in hot paths, immutable state, deterministic transitions. Any future sequencing deviations require inline `// STYLE-DEVIATION:` plus CHANGELOG entry.
>
> Turn Sequencing Phase 1 Addendum (2025-09-25 later): Implemented flag-gated advancement mechanics: `EnableTurnSequencing` feature flag, default `TurnProfile` (Start→Main→End), `BeginTurnSegmentEvent` / `EndTurnSegmentEvent`, `EndTurnSegmentCondition`, and `TurnAdvanceStateMutator` handling both intra-turn segment progression and terminal segment turn advancement (increment TurnNumber, reset segment). Added `TurnSequencingCoreTests` validating inert flag-off behavior, Start→Main, Main→End, End→Turn increment + reset, and invalid segment mismatch path. Rule wiring minimal (condition + mutator invoked) pending future automatic DecisionPlan integration. Active player rotation & pass/commit semantics deferred to Phase 2. Style charter re‑emphasized (file‑scoped namespaces, explicit braces, no LINQ in mutator, immutability, deterministic transitions). Any future deviation mandates `// STYLE-DEVIATION:` + CHANGELOG entry.
>
> Backgammon Multi-Turn Redouble Addendum (2025-09-25 latest): Implemented multi-turn doubling cube progression (2→4→8) requiring distinct TurnNumbers and owner-on-roll gating. `DoublingDiceStateMutator` now enforces: (a) same-turn redouble blocked via `DoublingDiceState.LastDoubledTurn`, (b) only current owner (active player) may redouble, (c) ownership transfers to opponent after each accepted redouble. `DoublingDiceWithActivePlayerGameEventCondition` updated to validate owner activity and turn advancement (presence of `TurnState` + TurnNumber comparison) while remaining sequencing-agnostic beyond TurnState availability. Property test `GivenDistinctTurns_WhenOpponentsAlternateRedoubles_ThenValueProgresses2xEachTurn` added. Backlog deferred item closed; CHANGELOG updated. Style charter reaffirmed (file-scoped namespaces, explicit braces, no hot-path LINQ, immutability). Any future segment-specific doubling (e.g., Roll-only gating) will extend TurnProfile & condition logic with unchanged core mutator semantics.
>
> Turn Pass Addendum (2025-09-25 latest): Added explicit `TurnPassEvent` and `TurnPassStateMutator` enabling active player to terminate a turn early (advances TurnNumber, resets Segment to Start, rotates active player) under `EnableTurnSequencing`. Provides structural groundwork for future pass/commit semantics (Go consecutive pass termination, bidding/auction commit cycles) without requiring segment exhaustion. Inert when sequencing disabled. Test added (`GivenAnySegment_WhenTurnPassEventApplied_ThenTurnAdvancesAndActivePlayerRotates`). Style charter upheld (file-scoped namespaces, explicit braces, no LINQ in hot mutator path, immutable state transition). Backlog Workstream 10 updated to mark pass primitive delivered.
>
> Turn Commit Addendum (2025-09-25 latest): Introduced `TurnCommitEvent` + `TurnCommitStateMutator` enabling an explicit Main→End segment transition without advancing `TurnNumber` or rotating the active player. This models commit/finalization intent distinct from full turn termination (pass) and sets the stage for richer End / Resolution segment semantics (e.g., cleanup, deferred effect batching, simultaneous order reveal). Behavior: active when `EnableTurnSequencing` is ON and current `TurnState.Segment == Main`; ignored otherwise (state unchanged). Tests added (`GivenMainSegment_WhenTurnCommitEventApplied_ThenSegmentTransitionsToEnd`, `GivenStartSegment_WhenTurnCommitEventApplied_ThenStateUnchanged`). CHANGELOG & backlog updated. Style charter reaffirmed (file-scoped namespaces, explicit braces, no LINQ in mutator hot path, immutable transitions). Future extension: introduce optional Commit/Resolution segments in TurnProfile; current implementation remains minimal to avoid premature abstraction.
>
> Style Re‑Emphasis (contextual to addendum): All benchmark modifications adhere to repository charter:
>
> 1. File-scoped namespaces only
> 2. Explicit braces for all control flow
> 3. No LINQ in hot loops (event generation & application)
> 4. Immutable state transitions (no in-place mutation)
> 5. Deterministic paths (rook oscillation chosen for reversible legality)
> 6. Allocation awareness (no per-iteration allocations beyond fixed arrays)
>
> Revision Note (2025-09-21, branch `feat/architecture-and-dx`): Initial implementation landed for feature flags, DecisionPlan (parity mode), deterministic RNG scaffold, and documentation skeletons. Remaining items annotated below with status.
>
> Revision Addendum (2025-09-21 later, same branch): Added timeline zipper (flagged), dual state hashing (64-bit FNV-1a + 128-bit xxHash128), extended `IEvaluationObserver` with `PhaseEnter`, `StateHashed`, and trace capture scaffold (in‑memory last-evaluation trace). Added hashing + observer overhead benchmarks and documentation updates. (Previous internal BugReport scaffold removed; future external reproduction tooling deferred to roadmap item 14.)

## Executive Summary

This plan operationalizes the 15+ architectural and developer experience upgrades outlined in `docs/plans/roadmap.md`. It groups them into coherent workstreams, defines phased delivery (Foundations → Acceleration → Observability & Tooling → Hardening & Ecosystem), and specifies deliverables, acceptance criteria, success metrics, risk mitigations, and migration safety for existing modules (Chess, Backgammon) and API consumers.

## Progress Status (2025-09-21 – branch `feat/architecture-and-dx`)

| Workstream | High-Level Status | Notes |
|------------|-------------------|-------|
| 1. Rule Evaluation Engine Modernization | PARTIAL | DecisionPlan parity path + grouping + EventKind filtering (Move/Roll/State/Phase with tests) + exclusivity metadata scaffold + masking runtime + debug parity dual-run + deterministic & randomized parity harnesses; typed EventResult + rejection reasons + `HandleEventResult` extension landed (non-breaking); observer perf targets pending |
| 2. Deterministic Randomness & State History | PARTIAL | RNG + dual state hashing (64/128-bit) + timeline zipper + GameBuilder.WithSeed deterministic seeding API + undo/redo invariant tests (external reproduction envelope deferred – see roadmap item 14) |
| 3. Movement & Pattern Engine Compilation | PARTIAL | IR + resolver scaffold; flag + services wired; compiler populated (Fixed + MultiDirection + Direction); adjacency cache scaffold + flag; parity tests added (Fixed/MultiDirection/Direction + chess archetype) + integration parity (pawn single + unreachable double) |
| 4. Performance Data Layout & Hot Paths | PARTIAL → NEARING COMPLETE | BoardShape (built), PieceMap incremental (integrated), Bitboard incremental snapshot (global + per-player), SlidingAttackGenerator (ray precompute), sliding path fast-path (pre-compiled resolver), occupancy semantics charter, Parity V2 (blockers/captures/multi-block/edge/non-slider) tests, benchmarks published (FastPath default ON). Remaining: typed per-piece masks, Bitboard128 design & implementation (>64 tiles), LINQ legacy sweep, mobility heuristic, topology-based pruning heuristics. |
| 5. Concurrency & Simulation | PARTIAL | Core Simulator API (single, parallel playouts), batch metrics (histogram/variance/percentiles), randomized + composite policies, observer hooks, early-stop sequential playout; legal move helper policy added. Pending: parallel early-stop, branching factor metrics doc, advanced policy heuristics. |
| 6. Observability & Diagnostics | PARTIAL | Observer + PhaseEnter + StateHashed + in-memory trace capture + JSON trace exporter; visualizer pending |
| 7. Developer Experience & Quality Gates | PARTIAL | Baseline benchmark + property test scaffold; invariants & perf CI gate pending |
| 8. Public API Facade (Deferred) | DEFERRED | Removed – to be re-imagined later |
| 9. Small Structural Refactors | NOT STARTED | Refactors/de-analyzers not started |

Legend: COMPLETED / PARTIAL / NOT STARTED (scope as defined in this plan).

### Intermediate Stage Tracking

| Stage | Theme | Status | Notes |
|-------|-------|--------|-------|
| 2.5 | EventKind taxonomy activation | COMPLETED | Concrete `SelectActivePlayerGameEvent` (State) + rule; marker interfaces public; roll phase refactored to emit state event; filtering tests cover Move/Roll/State/Phase (test-only phase control event). Benchmarks deferred until final perf pass. |

## Guiding Principles

- Determinism and Immutability are non‑negotiable invariants.
- Performance improvements must be measurable (benchmarks) and opt‑in behind stable abstractions until proven.
- Backwards compatibility for public APIs and module builders (no breaking changes without versioned surfaces).
- Each phase ends with: Green tests, Updated docs, Benchmarks captured, Changelog entry.

## Workstream Overview

1. Rule Evaluation Engine Modernization (Items 1, 7, 9)
2. Deterministic Randomness & State History Evolution (Items 2, 3, 14)
3. Movement & Pattern Engine Compilation (Item 4, 10 partial – DirectionPattern now supported)
4. Performance Data Layout & Hot Path Optimization (Items 5, small refactors, bitboards (10))
5. Concurrency & Simulation (Item 6)
6. Observability & Diagnostics (Items 9, 12, 13)
7. Developer Experience & Quality Gates (Items 11, 13, 15 + small refactors)
8. Public API Facade (Deferred) (Item 8)
9. Small Structural Refactors (LINQ removal, record structs, adjacency caches, no hidden globals)

## Phase Timeline

| Phase | Focus | Key Workstreams | Exit Criteria |
|-------|-------|-----------------|---------------|
| Phase 1 Foundations | Deterministic Core & Plan Skeleton | 1,2,3 (initial), 7 (typedef scaffolding) | DecisionPlan prototype; IRandomSource integrated; Zipper state model behind feature flag; tests green |
| Phase 2 Acceleration | Performance & Pattern Compilation | 3 (DFA complete), 4, 5 (sim proto), 10 (internal bitboards chess) | Benchmarks show >=5x faster path resolve; simulator runs parallel playouts deterministically |
| Phase 3 Observability & DX | Traces, Visualizer, Property Tests | 6, 9 (observer hooks), 11,12,13 | Trace UI/MVP; invariants passing 100+ FsCheck cases; perf guardrail CI failing on regression |
| Phase 4 Hardening & Ecosystem | Stability & Packaging | 15, refactors finalize | Extension point docs; migration guide (HTTP facade deferred) |

## Sequencing Refinements (Post Review)

The external architectural review recommends tightening early phase scope and pulling minimal observability forward. Adjusted high-level plan:

| Phase | Duration (target) | Added / Emphasized | Deferred from Earlier Draft |
|-------|-------------------|--------------------|-----------------------------|
| Phase 1 Foundations | 1–2 weeks | Minimal DecisionPlan, IRandomSource, no-op IEvaluationObserver, baseline benchmarks, FsCheck scaffold | Short-circuit masks, advanced predicate hoisting |
| Phase 2 Acceleration | 2–3 weeks | Pattern compiler (subset: sliders, knights, basic forward strides), SoA mirrors, one short-circuit optimization, optional chess bitboards (flag) | Full DFA for exotic patterns |
| Phase 3 Observability & Simulation | 1–2 weeks | JSON trace emitter, CLI viewer MVP, Simulator (sequential + parallel), Merkle hash tooling | Full web visualizer UI |
| Phase 4 Hardening & Ecosystem | ~1 week | Versioned DTOs, package split, analyzers for hidden globals, migration docs | Any experimental feature graduation pending metrics |

Rationale:

- Early instrumentation (observer) accelerates validation of subsequent optimizations.
- Narrow initial DFA scope reduces compiler complexity risk.
- Simulator deferred until after core speedups to avoid benchmarking unstable baselines.

## Scope Guardrails

To mitigate overreach and maintain momentum:

- DecisionPlan v1: fixed rule order, array iteration only (no multi-level mask layering yet).
- Pattern compiler v1: only (a) fixed-step, (b) repeat-until-blocked (sliders), (c) single jump (knight), (d) simple single-direction stride (DirectionPattern). Defer conditional / compound patterns.
- Bitboards: chess module internal adapter (phase 1: occupancy + per-player masks delivered); abort if <15% net gain on attack generation after two profiling runs.
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
| External Reproduction Envelope (Deferred) | Out-of-process artifact (seed, event list, terminal state hash, decision plan version) handled by external tooling – no in-core schema commitment. |
| Performance Gate | Observer enabled adds ≤5% overhead in microbenchmarks. |
| Fallback Safety | Disabling compiled patterns or DecisionPlan (feature flag) reverts to legacy path with identical test outcomes. |

## Feature Toggles & Flags

| Flag | Purpose | Default | Removal Condition |
|------|---------|---------|--------------------|
| LEGACY_RULE_ENGINE (compile symbol) | Enables legacy evaluator in tests for parity checks | On (tests only) | After two successive minor versions with zero diffs |
| EnableCompiledPatterns | Switch between visitor and DFA engine | On (parity achieved) | Can remove flag once perf benchmarks captured |
| EnableBitboards | Use chess bitboard adapter | Off | If sustained >15% attack gen speedup across 3 benchmarks |
| EnableStateHashing | Compute Merkle hash each transition | Off (Phase 1) | Always on after Phase 3 stability |
| EnableTraceCapture | Capture in-memory trace (JSON export pending) of last evaluation | Off | Always on (behind observer presence) once overhead validated |

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
| External reproduction envelope (deferred) | Additional Acceptance Criteria |
| Provide clear feature flags | Feature Toggles & Flags |
| Expand metrics with phased targets | Updated Metrics Targets |

## Detailed Action Items

### 1. Rule Evaluation Engine Modernization

Deliverables (Status annotations in brackets) – Finalization Note (2025-09-25): Core modernization scope now considered COMPLETE (grouping, event kind filtering, manual + static exclusivity masking, debug parity dual-run, observer skip taxonomy). Remaining observer granularity refinements (invalid vs ignored differentiation, composite skip capture) deferred to Observability workstream and do not block engine graduation.

- `DecisionPlan` immutable model: phases, rule table, pre-bound predicates, mutator delegate array. **[COMPLETED (parity subset – minimal model, no delegate table yet)]**
- Phase reference caching in `DecisionPlanEntry` to eliminate runtime tree lookups. **[COMPLETED – micro-optimization]**
- Predicate hoisting v1: Skip evaluation for trivially true `NullGameStateCondition` entries (flag on `DecisionPlanEntry`). **[COMPLETED]**
- Compiler: `DecisionPlanBuilder.Compile(GameBuilderContext ctx)`. **[COMPLETED (integrated into builder; context abstraction simplified)]**
- Execution path: `GameEngine.HandleEvent` uses precomputed plan. **[COMPLETED (flag-gated parity path)]**
-- Typed result: `EventResult` discriminated union + rejection reasons + `HandleEventResult` adapter. **[COMPLETED – extension surface only (core pipeline still returns progress for legacy API)]**
- Observer integration points for rule evaluation events. **[COMPLETED]**
Acceptance Criteria:
- No functional behavior change vs legacy path (golden test suite). **[PENDING – parity tests not yet added]**
- Benchmark: `HandleEvent` median latency reduced ≥30% on sample scenarios (Chess opening moves, Backgammon entry moves). **[PENDING – only baseline harness exists]**
- Trace includes rule index + failing predicate reason. **[COMPLETED – enriched trace entries]**

Upcoming / In-Progress Optimizations (Design Drafted in `decision-plan-optimizations.md`):

- G1 Grouping: compile contiguous identical predicate phases into gated groups (feature flag `EnableDecisionPlanGrouping`). **[COMPLETED – gate evaluated once; no behavior drift; test added]**
-- G2 Event Filtering: introduce `EventKind` tagging to skip irrelevant groups/entries (`EnableDecisionPlanEventFiltering`). **[IN-PROGRESS – classifier + filtering path + initial tests added; expanded tagging & benchmarks pending]**
- G2 Event Filtering: introduce `EventKind` tagging to skip irrelevant groups/entries (`EnableDecisionPlanEventFiltering`). **[COMPLETED – classifier + filtering path + heterogeneous benchmark variants + metrics tests]**
-- M1 Manual Skip Masks: builder hints for mutually exclusive branches producing bitmask skip sets (`EnableDecisionPlanMasks`). **[COMPLETED – runtime masking + tests]**
-- D Debug Parity: dual execution verification path (`EnableDecisionPlanDebugParity`). **[COMPLETED – dual-run comparison + mismatch diagnostics + tests]**
- M2 Static Exclusivity Inference: attribute-driven automatic mask derivation. **[PENDING]**
- M2 Static Exclusivity Inference: attribute-driven automatic mask derivation. **[COMPLETED – precedence builder > phase definition > attribute; integrated into mask compilation]**
Risks & Mitigation:
- Complexity creep: keep plan structure minimal (arrays + bitsets). Stage features (start w/out short-circuit masks, add later).
- Debug difficulty: include verbose validator to cross-check results in tests.
Migration Strategy:
- Keep legacy evaluator for one release hidden behind `#if LEGACY_RULE_ENGINE` or configuration for A/B verification.

### 2. Deterministic Randomness & State History Evolution (FINALIZED 2025-09-25)

Deliverables (Status annotations in brackets):

- `IRandomSource` + `XorShiftRandomSource` implementation (seed + serializable state). **[COMPLETED]**
- GameState includes RNG snapshot (struct) + `WithRandom(IRandomSource)` cloning. **[COMPLETED]**
- Persistent zipper model: `GameTimeline { ImmutableStack<GameState> Past; GameState Present; ImmutableStack<GameState> Future; }`. **[COMPLETED (flag-gated implementation)**]
- Merkle hash: deterministic hash over artifact ids, piece positions, dice values, RNG state. **[COMPLETED (64-bit + 128-bit xxHash128 upgrade)]**
<!-- Removed previous internal `BugReport` envelope scaffold: concept moved out of core; external tool will supply reproduction envelope per roadmap item 14. -->
Acceptance Criteria:

- Replaying same seed + events yields identical final hash and RNG state. **[COMPLETED – automated test `ReplayDeterminismTests` asserts Hash, Hash128, Seed equality]**
- Undo/Redo operations O(1) and hash-stable. **[COMPLETED – zipper invariants test suite active]**
<!-- Removed internal replay harness item (now external responsibility). -->
Risks:

- Hash collisions (mitigated by dual 64/128-bit hashes; future optional 128-bit-only mode under consideration).
- State size growth (hash interning map deferred – documented in backlog).
Migration:
- Hash initially optional; enable via feature toggle in builder (will graduate to always-on once perf gates affirmed).

Finalization Note: Scope for this milestone is COMPLETE. Remaining deferred enhancements (external reproduction envelope tooling, hash interning, timeline diff utilities) tracked under backlog and no longer gate engine graduation.

### 3. Movement & Pattern Engine Compilation (CLOSING NOTE 2025-09-25)

Deliverables (Status annotations in brackets):

-- Pattern IR: normalized representation (directions, repeats, terminals). **[COMPLETED – `CompiledPatternKind`, `CompiledPattern`]**
-- Compiler scaffold: per-piece table generation + service registration (emits empty lists). **[COMPLETED]**
-- Resolver: compiled attempt with visitor fallback. **[COMPLETED – behind `EnableCompiledPatterns`]**
-- Compiler population (translate existing `IPattern` instances to IR). **[COMPLETED – FixedPattern, DirectionPattern, MultiDirectionPattern mapped (Fixed/Ray/MultiRay)]**
-- Performance benchmarks (visitor vs compiled). **[PENDING – scaffold exists (`PatternResolutionBenchmark`)]**
-- Micro-benchmark per IR kind (Fixed/Ray/MultiRay) **[COMPLETED – `CompiledPatternKindsBenchmark`]**
-- Parity test suite (legacy vs compiled). **[COMPLETED – archetypes + adjacency cache parity + scope guard tests]**
-- Scope guard tests ensuring only supported kinds compile (AnyPattern / NullPattern explicitly ignored). **[COMPLETED – `CompiledPatternScopeGuardTests`]**
-- Documentation of current compilation scope & determinism note. **[COMPLETED – `compiled-patterns.md` updated]**
-- Edge-case semantics charter (blocked/capture precedence, tie-breaking, reconstruction failure, determinism invariants) **[COMPLETED – `movement-semantics.md` extended 2025-09-25]**
Acceptance Criteria:

- All movement tests + parity suite green under compiled engine once populated. **[PARTIAL → CORE PARITY ACHIEVED (supported kinds)**; remaining work: broader randomized sample & edge semantics charter]
- ≥5x faster pattern resolution on 1000 random moves. **[PENDING]**
Current Status:
- Infra merged; feature ON by default (Fixed, Direction, MultiDirection). Scope documented; AnyPattern / NullPattern intentionally uncompiled (runtime visitor only). New scope guard tests enforce contract. Visitor fallback retained for unsupported future kinds.
- Edge-case semantics charter finalized – any future pattern scope expansion (e.g., conditional, leaper, wildcard expansion) MUST: (1) update charter, (2) add guard + parity tests, (3) include explicit CHANGELOG entry, (4) pass style charter (no hot-path LINQ, immutable, deterministic).
- Large-sample (1000 randomized queries) benchmark added (`PatternResolutionLargeSampleBenchmark`) – first run shows compiled resolver achieving allocation reduction (~19% fewer MB) but not yet latency speedup vs visitor (1.02× slower on dense random endpoints). Indicates current heuristic mix (frequent negative lookups, early ray termination) diminishes benefits; micro-kernel still shows ~23% latency win per supported IR kind. Workstream closed with note that the ≥5× target is deferred pending additional acceleration layers (sliding fast-path, adjacency shape exploitation) and potential heuristic gating.
- Randomized parity harness (`RandomizedCompiledPatternParityTests`) over 2000 deterministic random samples passes with zero mismatches, reinforcing semantic parity for current compiled scope.
Risks:
- Premature optimization without representative workloads.
- Accidental broadening of scope (guarded by scope tests).
Migration:
- `CompilePatterns()` invoked during game build; toggle to disable for troubleshooting.
Closure & Deferred Work:
- ≥5× aggregate throughput target deferred; will re-evaluate after integrating topology-aware pruning, blocked-path short-circuit heuristics, and potential leaper precomputation. Documented in backlog.
- Remaining tasks (heuristics, LINQ sweep) migrated to backlog; workstream marked closed for current milestone (parity + documentation + representative benchmarks achieved).

### 4. Performance Data Layout & Hot Path Optimization

Initiation Note (2025-09-25): Workstream 3 closed (parity + semantics charter + benchmarks). Beginning focused acceleration pass: stabilize sliding fast-path under mixed densities, introduce blocked/capture microbenchmarks, and prepare pruning heuristics (per-piece occupancy masks, topology-based early exits). Style charter re-emphasized here: zero hot-path allocations on fast-path hits, no LINQ, explicit braces, immutable snapshots, deterministic ordering. Any deviation must be annotated with `// STYLE-DEVIATION:` and logged in CHANGELOG.

Deliverables (Status annotations in brackets):

- Internal `BoardShape` (tile adjacency arrays, directional lookup table). **[COMPLETED – always built; flag controls exploitation]**
- `PieceMap` struct-of-arrays + incremental move update path in `GameProgress`. **[COMPLETED]**
- Incremental Bitboard snapshot (global + per-player occupancy) + dual snapshot propagation on move. **[COMPLETED]**
- Sliding attack generator (precomputed directional rays + blocker aware traversal). **[COMPLETED]**
- Sliding path fast-path (bitboards + attacks) integrated ahead of compiled resolver in `ResolvePathCompiledFirst`. **[COMPLETED]**
- Fast-path parity tests (rook, bishop, queen) vs compiled-only reference (empty-ray scenarios). **[COMPLETED]**
- Replace LINQ in new hot loops (attack generation, fast-path chain build). **[COMPLETED – broader legacy sweep pending]**
- Benchmark harness (baseline present). **[COMPLETED – specific sliding benchmarks pending]**
- Microbenchmarks for sliding fast-path vs compiled-only & legacy visitor. **[PENDING]**
- Sliding fast-path microbenchmark (empty/blocked/capture/off-ray) + allocation probe variant (`FastPathAllocationProbeEmpty`). **[COMPLETED – provides baseline & allocation-free verification harness]**
- Blocked / capture scenario parity suite (decide geometric vs occupancy semantics). **[PENDING]**
- Typed per-piece occupancy masks (future selective attack pruning). **[COMPLETED – flag `EnablePerPieceMasks`; `BitboardLayout` exposes ordered `Pieces[]`; `BitboardOccupancyIndex` maintains optional per-piece mask array rebuilt on snapshot bind (incremental still disabled). Default off pending overhead benchmark.]**
- Bitboard128 / dual-mask strategy for >64 tile boards. **[PENDING]**
- Mobility heuristic prototype leveraging bitboards (feed future evaluation module). **[PENDING]**
- Mobility heuristic prototype leveraging bitboards (feed future evaluation module). **[COMPLETED – internal `MobilityEvaluator` computes per-player sliding mobility counts via attack rays + occupancy; deterministic, allocation-light; foundational test added; future extensions (leapers, pawns, weighting, mask-integrated pruning) deferred]**

## Appendix: Consolidated Style Charter (Re-Emphasized 2025-09-25)

All new or modified engine/core code (including tests & benchmarks) MUST adhere to:

1. File-scoped namespaces only (no nested namespace blocks).
2. Explicit braces for every control flow statement (no single-line implicit bodies).
3. No LINQ in hot paths (mutators, evaluators, fast-path reconstruction, benchmark inner loops). Acceptable in test setup or non-critical doc tooling.
4. Immutability: `GameState` and artifact state objects are never mutated in place; transitions must produce a new snapshot or return the original when no change.
5. Determinism: identical prior state + identical event sequence + identical feature flag configuration must yield identical resulting state & hashes (64-bit + 128-bit) across platforms.
6. Allocation discipline: hot-path success cases should allocate zero heap objects (use stackalloc / pooled buffers when required). Any intentional allocation in a hot path requires an inline comment with justification and a tracking item.
7. Predictable evaluation order: rule evaluation side effects restricted to observer callbacks only; no hidden static caches outside explicit build-time structures.
8. Tests follow AAA pattern (`// arrange`, `// act`, `// assert`) and cover each rule branch (Valid, Invalid, Ignored, NotApplicable) when introducing new rule types.
9. XML documentation for all public APIs; invariants and determinism assumptions placed in `<remarks>`.
10. Feature flag usage limited to controlled scope helpers (`FeatureFlagScope` in tests) or deterministic initialization in builders—no ad-hoc toggling in production pathways.

Deviation Handling: Any temporary deviation must include `// STYLE-DEVIATION:` comment plus justification and must appear in the CHANGELOG under a Temporary Exceptions subsection until removed.

This appendix centralizes style commitments already reiterated in `backlog-next.md` and `backlog-extra.md` to ensure a single authoritative summary within the strategic plan.

- Full LINQ hot-spot sweep across legacy resolver and rule evaluation. **[PENDING]**

Acceptance Criteria:

- Benchmarks show ≥5× improvement on representative sliding path resolutions (rook/bishop/queen) vs visitor baseline; target stretch 8–10×. **[PENDING]**
- Path fast-path yields no parity regressions across empty-ray + (later) blocked/capture suites. **[PARTIAL – empty-ray covered]**
- Hot allocation count <10% of baseline (initial) then <5% after sweep. **[PENDING]**

Risks:

- Diminishing returns if compiled resolver already near memory bandwidth limits – mitigate via focused per-direction metrics.
- Misaligned semantics (geometric vs occupancy-aware) causing future rule ambiguity – mitigate by codifying spec before capture tests.

Migration:

- All acceleration layers internal/flag-gated; fall back path remains compiled resolver → legacy visitor for unsupported patterns.

### 5. Concurrency & Simulation (FINALIZED 2025-09-25)

Progress Update (2025-09-25 – later): Core sequential + parallel simulators COMPLETE. Metrics layer (phase 1) and partial cancellation API LANDED. Workstream now marked FINALIZED for this milestone; remaining enhancements (depth histogram, refined branching factor, advanced helpers) migrated to backlog and are non-blocking.

New in this revision:

- `PlayoutMetrics` (AppliedEvents, RejectedEvents (placeholder 0), PolicyCalls (derived), MaxDepthObserved).
- `PlayoutBatchDetailedResult` (per-playout metrics + aggregate totals + average branching factor approximation).
- `SequentialSimulator.RunDetailed` returning `PlayoutDetailedResult` (non-breaking; legacy `Run` delegates to `RunWithMetrics`).
- `ParallelSimulator.RunManyDetailedAsync` metrics-aware parallel execution.
- `ParallelSimulator.RunManyPartialAsync` – cooperative cancellation returns partial batch (subset results) with `CancellationRequested` flag instead of throwing.
- Determinism parity test (basic) extended to cover basic vs detailed equivalence; multiset expansion (N=32) remains pending.
- CHANGELOG updated; placeholder legacy duplication (`PlayoutResults.cs`) fully REMOVED.

Terminal reason enum previously unified (None, NoMoves, PolicyReturnedNull, StopPredicate, MaxDepth, TimeLimit, CancellationRequested) – new partial API surfaces cancellation structurally rather than via exception.

Deliverables (Status annotations updated):

| Deliverable | Status | Notes |
|-------------|--------|-------|
| Sequential simulator (`SequentialSimulator.Run`) | COMPLETED | Deterministic core loop |
| Feature flag `EnableSimulation` | COMPLETED | Guards all experimental APIs |
| Policy delegates (`PlayoutPolicy`, `PlayoutStopPredicate`) | COMPLETED | Determinism contract documented in XML |
| Parallel playout orchestrator basic (`RunManyAsync`) | COMPLETED | Ordered results, cancellation (exception path) retained for legacy callers |
| Detailed metrics sequential (`RunDetailed`) | COMPLETED | Alloc-light; rejection placeholder 0 pending policy exposure |
| Detailed metrics parallel (`RunManyDetailedAsync`) | COMPLETED | Aggregates per-playout metrics |
| Partial cancellation (`RunManyPartialAsync`) | COMPLETED | Returns subset + flag; no exception semantics |
| Metrics aggregation type (`PlayoutBatchDetailedResult`) | COMPLETED | AverageBranchingFactor coarse (policyCalls / playout count) – refine later |
| Determinism docs | PARTIAL | Need docs/simulation.md enrichment (ordering, seeding, partial semantics) |
| Depth distribution / P50/P95 | NOT STARTED | Deferred – requires histogram capture |
| Rejection count accuracy | PARTIAL | Placeholder 0 until policy surfaces rejected attempts explicitly |
| Multiset terminal hash parity (N=32) | PENDING | Current test covers small sample only |
| Advanced policy helpers | NOT STARTED | Single-step enumerator & stochastic wrappers pending |
| Branching factor refinement | NOT STARTED | Current approximation acceptable for early analysis |

Upcoming Work (ordered):

1. Expand determinism parity test to 32 playout multiset (sequential vs parallel basic & detailed) – ensure hash multiset equality.
2. Introduce depth histogram + percentile (P50/P95) capturing (allocation-free counting buckets) gated behind detailed runs only.
3. Policy rejection instrumentation: extend policy interface or introduce wrapper to report rejected candidate attempts; wire into metrics.
4. Refine branching factor metric: total policyCalls / total applied events (exclude terminal call) and expose both coarse & refined forms.
5. Policy helpers: struct-enumerator single-step move policy; seeded randomized variant using `GameState.Random` (deterministic per seed); composite heuristic chaining.
6. Documentation expansion (`docs/simulation.md`): metrics fields, partial cancellation contract, determinism guarantees, seeding recommendations, style charter block.
7. Benchmark harness for simulation overhead (baseline vs metrics vs partial) validating <3% overhead target for 256 short playouts.

Acceptance Criteria (next increment):

- 32-playout multiset parity test green (sequential vs parallel, basic vs detailed).
- Depth histogram & percentile metrics captured with ≤1 allocation per detailed batch (array reuse or stack span acceptable) – validated via benchmark.
- Overhead from metrics (detailed run) ≤3% vs basic for short playouts (measured).
- Simulation doc updated with partial cancellation semantics table.

Risks & Mitigations:

- Potential metrics overhead: contain histogram logic to post-loop aggregation; intraloop counters only.
- Policy nondeterminism: add guard test verifying deterministic ordering when seed fixed.

Style Charter Reaffirmation (Simulation): No LINQ in playout inner loops (current implementation adheres); aggregation permitted post-run only. All control flow uses explicit braces; state transitions remain immutable; per-playout metrics captured via local primitive counters only. Any future deviation MUST include `// STYLE-DEVIATION:` justification and CHANGELOG entry. This charter is binding for any future backlog enhancements to simulation.

### 6. Observability & Diagnostics

Deliverables (Status annotations in brackets):

- `IEvaluationObserver` minimal v1 (RuleEvaluated, RuleApplied, EventIgnored) implemented + no-op default + builder injection. **[COMPLETED]**
- PhaseEnter callback emitted (legacy + DecisionPlan) **[COMPLETED]**
- StateHashed callback **[COMPLETED]**
- In-memory trace capture scaffold (last evaluation) **[COMPLETED]**
- Decision trace serializer (compact JSON) for last evaluation. **[COMPLETED]**
- CLI or lightweight web visualizer (Phase 3) reading trace JSON. **[DEFERRED – scope reduced for current increment]**
- Evaluation observer batching (buffer + flush on terminal) behind feature flag `EnableObserverBatching`. **[COMPLETED]**
Acceptance Criteria:
- Observer adds ≤5% overhead when enabled in benchmark microtests. **[PENDING – batching variant benchmark added]**
- Batched vs unbatched ordering parity test passes across representative move. **[COMPLETED]**
- Trace includes minimal fields to reproduce reasoning (state hash, rule id, reason enum). **[PARTIAL – fields captured; reason detail & rule index still pending]**
Risks:
- Performance drag; mitigated via feature flag, zero per-callback allocation under capacity, and consolidated flush.
- Complexity creep in trace layering; mitigated by decorator ordering (batching before trace capture).
Migration:
- Started synchronous unbatched; batching introduced as optional decorator (no public API change). Future default activation depends on benchmark thresholds.

### 7. Developer Experience & Quality Gates

Deliverables (Status annotations in brackets):

- Property tests (FsCheck) for listed invariants. **[PARTIAL – scaffold + minimal tests]**
- Benchmark suite (BenchmarkDotNet) in `/benchmarks` project. **[COMPLETED (baseline harness present)]**
- Uniform AAA Arrange/Act/Assert annotation across entire test suite (readability & future analyzer basis). **[COMPLETED]**
- Dedicated Developer Experience & Quality Gates document (`docs/developer-experience.md`) consolidating style charter, hot path definition, benchmark policy, feature flag discipline, determinism checklist. **[COMPLETED – new]**
- CI workflow: run benchmarks on PR; compare against stored baseline JSON; fail on >2% regression. **[NOT STARTED]**
- Documentation for extension points & stability guarantees (architecture & concept pages). **[PARTIAL – new feature docs only]**
- Analyzer roadmap (forbidden `System.Random`, hot-path LINQ, direct flag mutation, mutable state). **[COMPLETED – documented roadmap; implementation pending]**
- Roslyn analyzer package implementing first two rules. **[NOT STARTED]**
- Benchmark baseline artifact & tolerance configuration (JSON) stored in repo. **[NOT STARTED]**

Acceptance Criteria (Phased):

Phase DX-1 (Now):

- Style charter explicitly documented (central doc) and referenced by backlog & CHANGELOG. **[COMPLETED]**
- All new/modified tests use AAA comment sections. **[COMPLETED]**
- Baseline benchmark project compiles & runs locally (manual). **[COMPLETED]**

Phase DX-2:

- Property tests cover at least 3 core invariants (hash determinism, replay determinism, fast-path parity distribution) with ≥99% pass over 10k samples. **[PENDING]**
- Benchmark regression CI gate implemented with <2% default tolerance, failing on synthetic slowdown injection test. **[PENDING]**

Phase DX-3:

- First analyzer package (Random, HotPathLINQ) active in solution; zero violations in main branch. **[PENDING]**
- Allocation guard integration: flagged hot path benchmarks show 0 allocs (enforced by analyzer attribute or test). **[PENDING]**

Risks:

- Flaky benchmarks (mitigate with warmup control, explicit invocation count, dedicated CI runner settings, statistically smoothed comparisons).
- Overly strict analyzer false positives (stage rollout behind `EnableAnalyzersExperimental` flag or severity=info first).

Migration / Notes:

- Property tests can be quarantined (Trait=Quarantine) if instability arises; revisit after seed/retry harness added.
- Analyzer implementation deferred until core observability & acceleration stabilize (avoid churn in diagnostic baselines).

Style Charter Re-Emphasis:
The authoritative style rules live in `.github/copilot-instructions.md` and `docs/developer-experience.md`. All hot path code (evaluation loops, fast-path reconstruction, observer dispatch/batching, simulation loops, benchmark inner iterations) must remain free of LINQ, hidden allocations, and implicit control flow. Deviations require `// STYLE-DEVIATION:` plus CHANGELOG entry.

### 8. Public API Facade (Deferred)

DEFERRED: The HTTP / ASP.NET-facing facade, versioned DTO namespaces, and related packaging/versioning tasks have been intentionally removed from the active strategic scope to reduce complexity and keep focus on deterministic core, performance, and observability. They will be re-imagined later under a dedicated plan once:

1. DecisionPlan optimizations stabilize and legacy evaluator is retired.
2. Observer overhead and trace formats are finalized.
3. Simulation and hashing features reach documented stability.

When revisited, the facade plan will define: version negotiation policy, DTO evolution rules, migration guidelines, and separation boundaries (engine vs transport). Until then, any previous stories referencing `Veggerby.Boards.Api` or versioned DTO namespaces are considered obsolete and are not tracked.

Impact: Current integration surface = engine builders, events, observer, simulator, and future analyzers. External consumers should integrate directly via the engine library packages.

### 9. Small Structural Refactors

Deliverables (Status annotations in brackets):

- Replace LINQ in hot spots (profile-guided list). **[NOT STARTED]**
- Introduce `record struct` wrappers: `TileIndex`, `PlayerId` (implicit conversions avoided for clarity). **[NOT STARTED]**
- Adjacency cache keyed by board hash (dictionary or ConcurrentDictionary) with size cap. **[PARTIAL – basic per-board cache integrated behind feature flag]**
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

Performance Acceleration Tracking (Recent Progress):

- BoardShape now classifies board topology (Orthogonal / Orthogonal+Diagonal / Arbitrary) enabling future specialized move generation heuristics.
- Sliding fast-path instrumentation (FastPathMetrics) added – captures attempt vs hit vs skip reasons and fallback (compiled/legacy) distribution for ongoing benchmark reporting. (Granular reasons added: NoServices, NotSlider, AttackMiss, ReconstructFail; aggregate backward compatible.)
- Reentrant Feature Flag Scope: Unified thread-safe `FeatureFlagScope` (SemaphoreSlim + AsyncLocal depth + snapshot stack) adopted across entire test suite. Eliminates ad-hoc flag toggling, prevents interleaving during parallel test execution, and supports deterministic nested scope restoration. Concurrency stress test added (parallel tasks with distinct flag sets) ensuring isolation & final state restoration. Previous non-thread-safe helper marked for deprecation.
- Immobile piece guard prevents erroneous fast-path single-step path synthesis (maintains determinism).
- Movement semantics charter (`docs/movement-semantics.md`) authored – freezes sliding vs non-sliding rules, occupancy enforcement layer, and determinism guarantees ahead of Parity V2 test expansion.
- Introduced `EnableSlidingFastPath` feature flag (default off pending Parity V2 + benchmarks) separating bitboard occupancy enablement from fast-path activation.
- Capability seam finalized (`EngineCapabilities` aggregating Topology, PathResolver, AccelerationContext) replacing ad-hoc *Services lookups; fast-path & benchmarks now depend exclusively on this sealed immutable context.
- Incremental bitboard updates temporarily disabled (full rebuild fallback) pending extended parity soak; regression guard (`BitboardParityRegressionTests`) added.
- Style charter re-emphasized for acceleration layer (no LINQ in hot loops, explicit braces, file-scoped namespaces, allocation-free fast-path success path).

### Fast-Path Redesign Addendum (2025-09-24 / Updated 2025-09-25)

Goals (Phase FP-R1): (Status Update 2025-09-25 – Curated Parity Pack Landed)

- ≥3× speedup vs compiled resolver for empty sliding paths (rook/bishop/queen) – stretch 5×.
- ≥1.5× speedup at half occupancy density.
- Zero allocations on fast-path hit (validated via benchmark allocation column = 0 for hit scenarios).
- Deterministic parity across ≥10k randomized sliding path resolutions (friendly/enemy blockers, capture terminals, zero-length, non-slider negative cases).

Pipeline (Proposed Deterministic Chain):

1. Eligibility Guard (board size ≤64, feature flag, piece pattern is sliding, services present)
2. Direction Classification (orthogonal / diagonal / mixed) using compiled pattern metadata
3. Attack Ray Fetch (IAttackRays) for each candidate direction
4. Target Inclusion Check (early exit if target tile not on any ray)
5. Ray Truncation at First Blocker (respect capture semantics: include enemy, exclude friendly)
6. Path Reconstruction (allocate-free write into stack-based span, then materialize immutable list only if successful)
7. Occupancy Validation (IOccupancyIndex parity – ensures no stale snapshot)
8. Metrics Emission (Attempts, FastPathHits, SkipReason, HitLatencyTicks)
9. Fallback (compiled resolver → legacy visitor if unresolved)

Skip Reasons (enum – surfaced in metrics & future observer): NotSlider, FeatureDisabled, NoServices, TargetNotOnRay, FriendlyBlocker, ReconstructionFailed, ZeroLength, NonCardinalOrDiagonal.

Rollout Phases:

1. Dark Mode (compute + metrics only, result discarded) – validate stability & cost
2. Rook-Only Activation (parity soak 24h)
3. All Sliders (rook/bishop/queen) activation (parity soak 48h)
4. Default ON (flip flag default) – retain compiled resolver fallback
5. Legacy Scaffold Removal (remove unused branches & dead metrics)

Validation & Tooling (Update 2025-09-25):

- Extend parity test harness for randomized blocker & capture matrices (generate occupancy permutations with deterministic RNG seed).
- Add allocation assertions in benchmark harness (fail test run if hit path allocates >0 objects when feature enabled).
- Introduce micro-timer (Stopwatch.GetTimestamp diff) around steps 3–6 for per-stage profiling (internal only, compiled out in Release if overhead measurable).

KPI Tracking Additions:

- FastPathHitRate (must exceed 60% in empty / sparse benchmark scenarios).
- FastPathMedianLatency (target <40% of compiled resolver latency for empty rook path).
- SkipReasonDistribution (should predominantly be NotSlider or TargetNotOnRay – low ReconstructionFailed rate).

Style Reinforcement (Reaffirmed in Parity Pack & Benchmark additions):

- Allocation-free loops, explicit braces, no LINQ, deterministic ordering, immutable returned path list.
- All public surface unchanged; only internal resolver chain evolves.

Update (2025-09-25): Metrics ownership consolidated (single orchestrator extension); `SlidingFastPathResolver` slimmed to reconstruction-only; legacy traversal extracted to conditional partial (`GameProgress.Legacy`) and marked obsolete; invariant metrics test added; cleanup checklist introduced (`docs/cleanup/2025-fastpath-parity-checklist.md`).
Update (2025-09-25 later): Curated Sliding Fast-Path Parity Pack (`SlidingFastPathParityPackTests`) added as CI gate (representative scenarios) complementing exhaustive suite; DecisionPlan vs Legacy benchmark added (`DecisionPlanVsLegacyBenchmark`) establishing quantitative baseline for forthcoming DecisionPlan optimization milestones.

- Parity V2 sliding test matrix implemented (adjacent friendly/enemy, mid-ray blockers, multi-blocker order permutations, zero-length request) – total test suite now 462; all fast-path vs compiled parity assertions green.
- Style charter reaffirmed in new tests (explicit braces, file-scoped namespaces, no LINQ in engine hot-path code; LINQ confined to test assertions only).
- Sliding benchmark extended (FastPath, CompiledWithBitboardsNoFastPath, CompiledNoBitboards) enabling isolation of bitboards vs sliding fast-path overhead; style charter respected (no additional LINQ in hot loops, deterministic feature flag toggling per variant).
- Bitboards + Sliding FastPath defaults flipped ON (≤64 tiles) after benchmarks demonstrated ≥4.6× (empty), ≥2.4× (quarter), ≥1.5× (half) speedups vs compiled resolver.
- Configuration docs updated with disable snippet; code style rules reiterated in configuration & this plan.
- Capability seam finalized: Introduced `IPathResolver` abstraction and conditionally registered `SlidingFastPathResolver` decorator (gated by `EnableSlidingFastPath`) ahead of the compiled resolver when bitboard occupancy + attack rays + occupancy index services are present. Current decorator delegates (fast-path reconstruction logic pending) to minimize diff size for the upcoming implementation commit while enabling early service wiring validation.
- Style reinforcement: Decorator adheres to repository code style charter (file-scoped namespaces, explicit braces, four-space indentation, no LINQ in hot loops, immutable state). Future fast-path logic must preserve these guarantees and remain allocation-free.

### Next (Post-Acceleration) Items

- Bitboard128 dual-segment representation for >64 tile boards (design note drafted; implementation pending profiling need).
- Typed per-piece occupancy masks (finer sliding pruning / mobility metrics).
- Topology-based pruning (orthogonal-only boards skip diagonal ray setup; diagonal filtering specialization).
- Mobility heuristic / evaluation prototype using attack generator outputs.
- LINQ sweep through remaining legacy visitor & rule evaluation code paths (only outside hot loops now; still a cleanup target).

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
- RNG integration before Simulation (external reproduction envelope independent).
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
   -- Story: (Removed) internal bug report capture + replay (externalized)
3. EPIC: Pattern Compilation
   - Spike: IR representation **(DONE)** (trace overhead benchmark added separately for observability cost tracking)
      - Story: direction adjacency bitsets **(PENDING – initial adjacency cache scaffold complete)**
   - Story: repeat pattern expansion **(PENDING)**
   - Story: DFA executor + benchmarks **(PENDING)** (benchmark scaffold + compiler emits Fixed & MultiDirection patterns; doc added `compiled-patterns.md`)
   - Story: parity tests vs visitor **(IN PROGRESS)**
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
8. (Deferred) HTTP / Public API Facade – removed from active backlog (will return as a standalone plan when re-imagined)
9. EPIC: Structural Refactors
   - Story: record structs adoption
   - Story: adjacency cache keyed by hash
   - Story: analyzer for hidden globals

## Deliverables Summary

This action plan yields: faster deterministic evaluation, richer diagnostics, a compiled pattern engine, parallel simulation scaffolding, stable extension contracts, and hardened performance guardrails. (Replayable bug reports now explicitly out-of-scope for core; handled by future external tooling.)

## Next Immediate Steps

1. Capture current performance baselines (pre-DecisionPlan) in a new benchmark project.
2. Implement DecisionPlan spike with parity tests.
3. Introduce IRandomSource and integrate into GameState (no behavior change yet).
4. (Update) Landing exclusivity mask scaffold (flag-gated) extends DecisionPlan optimization path – future steps: debug parity dual-run + masking benchmarks.

-- End of Plan --
