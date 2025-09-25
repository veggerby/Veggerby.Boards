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

- [ ] Property test expansion (captures, blocked moves, multi-step paths, dice modules). (**PARTIAL – chess movement + backgammon dice invariants added; further randomized generators pending**)
- [ ] CI benchmark regression gate (JSON baseline compare, threshold config). **DEFERRED (2025-09-25)** – moved to medium-term DX bucket; current focus remains on completing Backgammon doubling invariants & multi-turn tests before adding gating infra.
- [ ] Roslyn analyzers: forbidden `System.Random`, mutable state mutation, FeatureFlags usage outside scopes. **DEFERRED (2025-09-25)** – analyzer design spec captured in `developer-experience.md`; implementation postponed until post-invariants stabilization.
- [ ] Analyzer tests (happy/diagnostic/fix). (Fixers optional initially.) **DEFERRED (2025-09-25)** – blocked by previous analyzer implementation deferral; will co-land with initial rule set.
- [x] Central Developer Experience & Quality Gates doc (`docs/developer-experience.md`) consolidating style charter, benchmark policy, determinism checklist, feature flag discipline.
- [x] Analyzer roadmap documented (rules list + phased rollout) – implementation pending.

Granular Breakdown / New Tasks (Updated – Workstream 7 FINALIZED 2025-09-25):

- [ ] Backgammon: Multi-turn doubling invariant (ownership gating + redouble sequence 2→4→8→… until cap) – pending turn progression semantics (DEFERRED; not blocking Workstream 7 finalization).
- [x] Backgammon: Negative invariants – pre-active-player attempt & same-turn redouble covered (tests added); remaining scenarios (non-owner redouble, exceeding cap) pending richer turn semantics.
- [x] Backgammon: Remove experimental `SelectActivePlayerRule` wrapper in favor of lightweight post-roll mutator (`SelectStartingPlayerStateMutator`). CHANGELOG + action plan updated; tests adjusted (no manual active player injection). Style charter reaffirmed (file-scoped namespaces, explicit braces, no hot-path LINQ, immutability, deterministic transitions).
- [ ] Adopt `TestPathHelper.ResolveFirstValidPath` in any remaining observer / movement tests duplicating path resolution (grep for `ResolveTilePathPatternVisitor` outside helper file) – ensures single authoritative path logic (Residual hygiene; not gating finalization).
- [x] Chess: King-side castling blocked invariant (manual deterministic scenario) – no FsCheck generators.
- [x] Chess: Capture sequence invariant (piece removal + position update) ensuring state immutability and history link correctness.
- [x] Property test acceptance criteria doc block (append to `developer-experience.md`) enumerating deterministic seed, AAA structure, explicit feature flag scopes, zero LINQ in invariant loop.
- [x] Style charter enforcement script stub (non-analyzer): simple Roslyn-based whitespace + `goto` + mutable field scan (placeholder until analyzers implemented).
- [x] Benchmark JSON schema draft for deferred regression gate (store name, p50, p95, allocated bytes, commit SHA, feature flag set).
- [x] CONTRIBUTING.md section referencing developer-experience doc & style deviation process.
- [x] Event rejection mapping guard test (enum exhaustiveness) – prevents silent unclassified reasons.
- [x] Feature flag test isolation pattern (FeatureFlagScope usage) doc with anti-pattern examples.
- [x] Deterministic chess opening sequence helper for reuse across invariants (reduces duplication & drift risk) – Optional (nice-to-have) now that key invariants landed.

Acceptance Criteria for Section 7 Completion (FINALIZED 2025-09-25):

- All listed non-deferred granular tasks completed & documented (capture invariant, castling blocked, negative doubling invariants, property test criteria, feature flag isolation, style enforcement stub, benchmark schema, CONTRIBUTING update, rejection guard test).
- No duplicated path resolution logic (grep for `ResolveTilePathPatternVisitor` outside helper returns 0 once task closed).
- Backgammon multi-turn doubling invariants: 3 successive legal doublings (2→4→8) validated once turn sequencing semantics introduced (DEFERRED – tracked but not blocking).
- Chess capture + castling blocked invariants added and deterministic. (DONE)
- `developer-experience.md` extended with: property test acceptance criteria + feature flag isolation pattern. (DONE)
- CONTRIBUTING.md references style charter + deviation process explicitly. (DONE)
- Event rejection guard test present and green (exhaustive over enum values except intentional future additions which must update test). (DONE)
- Style enforcement stub landed (documented non-blocking) – no style deviations without inline `// STYLE-DEVIATION:` + CHANGELOG entry. (DONE)

## 8. Public API Facade (Deferred)

Removed from active scope. Re-imagining (HTTP transport, DTO versioning policy) will occur under a future dedicated plan; no tasks tracked here.

## 9. Structural Refactors

- [ ] Replace residual LINQ in performance-sensitive paths (post profiling report).
- [ ] Record struct wrappers (`TileIndex`, `PlayerId`) – minimal; ensure no boxing.
- [ ] Hidden global analyzer (ensure only FeatureFlags used under controlled scope helper).

## 10. Unified Turn / Round Sequencing Semantics (NEW)

Goal: Introduce a deterministic, extensible, and feature-flag–gated turn / round sequencing model that supports current games (Chess, Backgammon) and future families (Go, Risk, Ludo, Kalaha/Mancala, Settlers of Catan, bidding/auction & simultaneous-reveal titles) without leaking game‑specific rules into core.

### Core Concepts (Proposed New Primitives)

- TurnArtifact: Sentinel artifact representing the global turn timeline (one per Game).
- TurnState: Immutable snapshot (TurnNumber, ActivePlayer, RoundNumber?, Segment, PriorityIndex, PhaseOrdinal, CycleId, PassCount, MetadataHash(optional)).
- Round vs Turn: Round aggregates a full cycle of each eligible player receiving exactly one primary turn (unless skipped). Games like Go = simple Player cycle; Catan = round-scoped start-of-round production hooks; Risk = potential reinforcements at round start.
- TurnSegments: Formal sub-structure (Upkeep, Roll, Main, Commit, Resolution, End). Configurable per Game through a TurnProfile builder. Absent segments omitted (no overhead).
- TurnAdvanceStateMutator: Pure transition mutator that increments turn counter, rotates active player (strategy definable: FixedOrder, DynamicOrder (insertion), PriorityQueue, ScoreSorted, TokenHolder). Produces new TurnState linking previous.
- TurnCommitEvent / TurnPassEvent: Explicit events to finalize segment or entire turn vs implicit heuristics.
- TurnOrderingStrategy abstraction: Pluggable policy invoked only at boundaries (no per-event hot-path overhead). Deterministic given state.
- SimultaneousCommit Envelope: Multi-player provisional sub-turn states collected under a TurnCycle (e.g., hidden orders) then resolved via a deterministic resolution policy.

### Invariants

1. Determinism: Given identical prior GameState + same ordered event list → identical TurnState chain (TurnNumber, ActivePlayer, Segment order) regardless of external timing.
2. Immutability: No in-place mutation of existing TurnState; every advancement yields a new TurnState artifact state.
3. Single ActivePlayer at any given (TurnNumber, Segment) unless in SimultaneousCommit mode (then zero or N provisional actors, but ActivePlayer field still denotes “resolving authority” or stays null with explicit flag).
4. No skipped turn without explicit emission of a TurnPassEvent (or rule-driven auto-pass mutator recorded in history for simulation reproducibility).
5. Segment transitions are guarded by explicit conditions (no hidden fallthrough) – e.g., cannot enter Main before Upkeep completed unless profile config suppresses Upkeep.
6. TurnNumber strictly monotonic +1 sequence; RoundNumber increments after last eligible player finalizes a turn (exclusions: eliminated players removed deterministically at defined cut points).
7. Ownership / gating rules (e.g., Backgammon doubling cube, Chess castling rights timing) depend only on stable TurnState fields (not transient engine counters).

### Cross-Game Mapping

| Game | Needed Features |
|------|-----------------|
| Chess | Simple fixed order (White→Black), segments: Main only, optional Upkeep for future clocks; TurnNumber gates draw / fifty-move potential metrics. |
| Backgammon | Fixed order; Roll segment precedes Move; Doubling permitted only in Roll (before first move) or start of opponent's future turn; cube gating uses DoublingDiceState.LastDoubledTurn. |
| Go | Fixed order; no segments beyond Main; pass handling increments PassCount used to detect consecutive passes → end-of-game scoring phase. |
| Ludo | Fixed order; Roll→Main; extra-turn rule (rolling 6) implemented as TurnReplay flag causing TurnAdvanceStateMutator to keep same ActivePlayer with incremented TurnNumber. |
| Kalaha (Mancala) | Fixed order; sowing may grant extra turn → replay semantics like Ludo; capture + store scoring at end of turn; end condition when one side empty triggers scoring segment. |
| Risk | Turn has Reinforcement→Attack (repeat cycles)→Fortify segments; Attack sub-loop tracked via SegmentOrdinal; elimination dynamically alters TurnOrderingStrategy (removed players). |
| Settlers of Catan | Upkeep (robber / production on dice roll) → Trade → Build → End; Robber relocation triggers conditional mini-phase; Year of Plenty / Monopoly etc. are events gated by TurnSegment. |
| Bidding / Auction (generic) | Simultaneous or sequential bidding cycle implemented as custom TurnProfile with Commit segment; resolution policy aggregates hidden bids deterministically. |

### Data Model Extensions (Tentative Fields)

TurnState Fields:

- int TurnNumber
- int RoundNumber
- Player ActivePlayer
- TurnSegment Segment (enum: Upkeep, Roll, Main, Commit, Resolution, End)
- int SegmentOrdinal (e.g., multiple Attack cycles in Risk, multiple Commit batches)
- int PassCount (resets except in games like Go where consecutive passes tracked)
- int LastActionEventIndex (debug / replay introspection; aids verifying no implicit actions)
- Player[] ProvisionalActors (only populated in simultaneous commit mode)
// Removed from tentative TurnState core fields; LastDoubledTurn now lives solely on DoublingDiceState to avoid leaking Backgammon specifics.

### Event & Rule Integration

- New events: BeginTurnSegmentEvent, EndTurnSegmentEvent, TurnPassEvent, TurnCommitEvent.
- TurnProfile registers allowed segment transitions: (null→Upkeep? Upkeep→Roll? Roll→Main? Main→End) etc.
- Conditions become simpler: DoublingDiceWithActivePlayerGameEventCondition -> (future) additionally require Segment == Roll and TurnNumber > DoublingDiceState.LastDoubledTurn (once segment model extended beyond Start/Main/End and Roll introduced).
- DecisionPlan can treat TurnState conditions as fast-path primitives (no LINQ). Potential future: mark them as static exclusivity mask bits (e.g., segment discriminators) for skip acceleration.

### Simulation & Metrics Hooks

- PlayoutMetrics extension: Track AverageTurnLength (events/turn), Passes, SegmentDistribution, TurnReplayCount (extra turns), RoundsCompleted.
- Deterministic hashing includes TurnNumber + RoundNumber + Segment for reproducibility verification.

### Backward Compatibility & Migration

Phase 0 (Flagged EXPERIMENTAL): Introduce TurnState behind feature flag `EnableTurnSequencing` with shadow emission (legacy path still drives active player). Parity tests compare active player & move legality.
Phase 1: Migrate Backgammon doubling & active player rotation to TurnState; retain legacy active player states for parity.
Phase 2: Remove legacy ActivePlayerState duplication (ActivePlayerState replaced or wrapped by TurnState projection) – AFTER parity benchmark stable.
Phase 3: Introduce segment gating for Backgammon (Roll vs Move) & adopt for Chess minimal (Segment=Main only) with zero overhead.
Phase 4: Implement pass / replay semantics (Go, Ludo/Kalaha prototypes) as separate modules exercising generic scheduling.

### Acceptance Criteria (for enabling by default)

- All existing test suites (core + property) green under flag ON with no regression benchmarks (HandleEvent p50 delta < 3%).
- Deterministic multi-turn doubling property test passes (2→4→8 with enforced opponent-only redoubles across distinct TurnNumbers).
- Refactored Backgammon invariants no longer rely on heuristic same-turn detection.
- New TurnState parity tests: Active player equivalence, turn count progression, no skipped players, replay (extra-turn) semantics deterministic.
- Simulation metrics extended with turn-derived fields validated by unit tests.
- Hash parity: enabling turn sequencing does not change 64/128-bit hash for pre-turn-enabled games (where semantics unchanged) OR documented expected delta with justification.
- Documentation: New `turn-sequencing.md` explaining model, mapping table, invariants, extension points & migration path.

### Non-Goals (Initial Scope)

- Time controls / real-time clocks (future extension; no scheduling deadlines now).
- Partial observable simultaneous resolution (e.g., sealed-bid auctions with bluff commitments) – only deterministic all-or-nothing commit supported first.
- Network / multi-user synchronization semantics – purely engine-level structural modeling.

### Deferred / Stretch

- Branching turn ordering (dynamic initiative reordering mid-round) for advanced war / drafting games.
- Multiple concurrent turn streams (team games) – potential future multi-lane TurnArtifact array.
- Turn rollback semantics (undo mid-turn segments) – requires segment boundary snapshot strategy.

### Tasks

Style Reminder: All sequencing code must follow style charter (file-scoped namespaces, explicit braces, 4-space indentation, no LINQ in hot mutators, immutable state). Deviations require `// STYLE-DEVIATION:` + CHANGELOG entry.

- [x] TurnArtifact & TurnState definitions (immutable; XML docs; invariants in remarks) – scaffolding merged (shadow mode, no behavior change).
- [x] TurnSegment enum (Start/Main/End) – initial minimal set (profile builder deferred).
- [x] TurnProfile configuration builder (declarative segment ordering & optional segments) – minimal default (Start→Main→End) implemented; future builder extensibility deferred.
- [x] TurnAdvanceStateMutator (increment TurnNumber, reset segment; ActivePlayer rotation deferred) – basic advancement behind feature flag.
- [x] Segment transition condition + mutator path (EndTurnSegmentEvent + condition + advancement logic) – Begin/End events introduced; dedicated Begin mutator deferred (handled inside advancement logic for now).
- [x] Minimal rule wiring (condition + mutator invocation path) validated via `TurnSequencingCoreTests`; automatic DecisionPlan registration deferred.
- [x] ActivePlayer projection compatibility layer (rotate legacy ActivePlayerState on terminal segment advancement; future TurnState authoritative projection pending).
- [x] Backgammon: Gating implemented using DoublingDiceState.LastDoubledTurn (same-turn redouble blocked) – TurnState remains domain‑agnostic.
- [x] Backgammon: Resolved abstraction decision – LastDoubledTurn retained on cube state (removed from TurnState).
- [ ] Backgammon: Multi-turn doubling invariant tests (2→4→8), opponent-only redouble gating.
- [ ] Chess: Minimal TurnState integration (Segment=Main) + parity tests vs legacy active player.
- [ ] Go prototype: Pass handling + two-pass termination invariant.
- [ ] Ludo/Kalaha prototype: Extra-turn (replay) semantics test.
- [ ] Simulation: Extend metrics + tests (AverageTurnLength, PassCount, ReplayCount).
- [ ] Benchmarks: Turn sequencing overhead microbenchmark (baseline vs flag ON; target <3% p50).
- [ ] Hash parity / evolution tests (flag off vs on – document expected differences only where intentional).
- [ ] Documentation: `turn-sequencing.md` + updates to `core-concepts.md` and `decision-plan.md` referencing turn gating.
- [ ] Feature flag scope tests (Ensure no TurnState emission when flag OFF).
- [ ] CHANGELOG entry & migration guide section.

### Exit Criteria for Workstream 10

- Feature flag removed or defaulted ON with legacy active player path deleted.
- All Backgammon doubling & new multi-turn invariants rely solely on TurnState.
- New games can register a TurnProfile without modifying core engine internals.
- Benchmarks show acceptable overhead (≤3% p50) & zero additional allocations on fast move path (excluding segment change boundaries).

### Risks / Mitigations

- Scope Creep: Keep initial segment set small; advanced games can layer additional conditions later.
- Performance Regression: Guard with dedicated microbenchmark; bail out early when `EnableTurnSequencing` is OFF.
- Complexity: Provide high-level TurnProfile DSL instead of ad hoc per-game procedural wiring.
- Hash Instability: Confine new hash inputs behind flag until stable.

---

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
