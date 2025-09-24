# Extra Backlog (Out-of-Plan Items)

This file tracks auxiliary improvement ideas not explicitly covered in `docs/plans/action-plan.md`. Items here are lower priority or opportunistic; they may graduate into the formal plan once justified.

## Observability Enhancements

- PhaseEnter callback in `IEvaluationObserver` (emit before evaluating any rule when phase deemed active). **COMPLETED**
- StateHashed callback (after future Merkle/xxHash computation) for correlation. **COMPLETED**
- In-memory trace capture scaffold (last evaluation) **COMPLETED**
- JSON trace serializer / exporter (compact) **COMPLETED** (now includes rule index + condition reason enrichment)
- Trace overhead microbenchmark (compare enabled vs disabled) **COMPLETED (TraceCaptureOverheadBenchmark)**
- Trace entry object pooling / reuse strategy (avoid per-callback alloc) **PENDING**
- Aggregated batch observer adapter reducing callback overhead via struct buffer.
- Counting observer benchmark (baseline) — COMPLETED (see `ObserverOverheadBenchmark`). (Will record % overhead once benchmarks are run at end of perf cycle.)
- Overhead target doc update once hashing added.
- Typed EventResult + rejection reasons + `HandleEventResult` extension method (non-breaking) — COMPLETED (initial heuristic BoardException mapping; future refinement for precise rule invalid vs engine invariant differentiation).

## Parity & Testing Utilities

- Golden parity harness comparing legacy vs DecisionPlan across randomized event streams (property-based). **PARTIAL – deterministic harness + randomized short pawn advance sequence scaffold added; broader property distributions (captures, promotions, dice modules) pending**
- Test helper: `using FeatureFlagScope(decisionPlan: true)` disposable to restore flags automatically. **COMPLETED**
- Multi-event deterministic parity test sequence — COMPLETED.
- Movement pattern parity scaffold (legacy visitor vs compiled resolver pre-population) **COMPLETED**
- Restore and expand compiled pattern parity tests (legacy visitor multi-direction repeat + chess pawn scenarios). **COMPLETED** (double-step pawn intentionally not modeled; integration test asserts null parity for unreachable e2->e4)
-- Integration-level compiled patterns parity (Chess builder under feature flag). **COMPLETED**
- Adjacency cache on/off compiled pattern parity tests (representative chess archetypes). **COMPLETED**
-- Enable compiled patterns by default after parity + cache validation. **COMPLETED**
- Uniform AAA Arrange/Act/Assert comments across test suite — COMPLETED.
- (Removed) Former BugReport replay harness concept replaced by future roadmap item 14 (external issue reproduction) – not in core code.
- Deterministic seeding API (`GameBuilder.WithSeed`) **COMPLETED**
- Dual-run test asserting trace sequence identical with/without hashing enabled **PENDING**

## Performance Tooling

- Microbenchmark variant with observer enabled vs disabled to measure overhead delta.
- Microbenchmark variant with trace capture enabled vs disabled (overhead quantification) **PENDING**
- Pattern resolution benchmark scaffold (visitor vs compiled placeholder) **COMPLETED (compiler now populated for Fixed & MultiDirection)**
- DecisionPlan phase cache optimization benchmark follow-up (measure delta from removing ResolvePhase traversal). **PENDING**
- Predicate hoisting extended: future phases include grouping conditions, short-circuit bit masks, and invalidation sets. **PENDING**

### DecisionPlan Optimization (Grouping & Masking) Tasks

- Add feature flags: `EnableDecisionPlanGrouping`, `EnableDecisionPlanEventFiltering`, `EnableDecisionPlanMasks`, `EnableDecisionPlanDebugParity`. **PARTIAL (Grouping + EventFiltering + Masks flags added)**
- Group compiler pass: identify contiguous identical predicate entries -> emit groups. **COMPLETED**
- Group evaluation path + tests (gate evaluated once, skip when invalid). **COMPLETED (basic positive test; add invalid-gate test later)**
-- EventKind enum & basic classifier (Move/Roll heuristic) + SupportedKinds table compile. **COMPLETED (initial)**
-- Event filtering evaluation fast-path (skip non-matching kinds before predicate). **COMPLETED (flag gated + tests + baseline benchmark scaffold)**
  - Follow-up: Expand rule tagging coverage, add mixed-kind multi-phase benchmark & debug parity shadow path. **COMPLETED – taxonomy expanded (State/Phase), marker interfaces public, concrete `SelectActivePlayerGameEvent` added; tests cover Move/Roll/State/Phase; benchmarks deferred**
  - Metrics: quantitative filtering evaluation count test. **COMPLETED (DecisionPlanEventFilteringMetricsTests)**
- Builder hint API for exclusivity (e.g., `.Exclusive("phase-set-1")`). **COMPLETED**
- Mask table generation from exclusivity hints. **COMPLETED (ExclusivityGroups + ExclusivityGroupRoots compiled)**
- Evaluation mask application logic + tests (skip flagged entries). **COMPLETED (feature-flagged; masking tests added)**
- Debug parity dual-scan path + assertion (shadow linear evaluator). **COMPLETED**
- Property-based randomized parity suite for grouping + masks. **PENDING**
- Benchmarks: GroupingBaseline, EventFilteringBaseline, MaskingBaseline, DebugParityOverhead. **PENDING**
- Static exclusivity inference attribute + analyzer (optional). **PENDING**
- OptimizationVersion property + observer trace inclusion. **PENDING**
- Allocation profiler script capturing top 10 hot allocations in `HandleEvent`.
- Future: capture observer callback counts per benchmark iteration to detect silent regression.

## Developer Experience

- Roslyn analyzer: warn if state mutator returns same instance (should always produce new or explicitly skip change).
- Analyzer: detect accidental use of `System.Random` in engine code (must use `IRandomSource`).

## Documentation

- Observer guide: mapping callbacks to evaluation lifecycle with timing diagram.
- Parity testing methodology doc (dual-run strategy, risk mitigation).

## Future Nice-To-Haves

- Lightweight structured logging adapter (compile-time optional) layering on observer events.
- Event correlation id generator (deterministic increment) surfaced in observer callbacks.
- Optional HTTP facade re-introduction as separate package (`Veggerby.Boards.Http`) with versioned DTOs; keep separate until hashing & observer extensions stabilize.

## Simulation Enhancement Backlog

New items following initial Simulator API landing (core + metrics + observer):

- Parallel early-stop playout variant (evaluate predicate against rolling shard metrics without full synchronization each iteration).
- Branching factor / candidate distribution sampling helper leveraging `IPlayoutObserver` (aggregate mean, p95, max across batch).
- Policy helper: multi-step path expansion (follow relation chains up to N distance) yielding longer `MovePieceGameEvent` paths deterministically.
- Policy helper: stochastic epsilon-greedy wrapper (uses deterministic RNG in state) combining heuristic policy + randomized fallback.
- Observer adapter aggregating only every K steps to reduce callback overhead for deep playouts.
- Batch percentile convergence predicate utilities (extension methods returning `Func<PlayoutBatchResult,bool>` for common criteria).
- Playout trace diff tool (compare two traces step-by-step for divergence analysis when policies differ).
- Incremental batch metrics aggregator (avoid reallocating list each iteration in `PlayoutManyUntil`).
- Optional transposition avoidance (skip repeating identical terminal states across playouts when hashing enabled) – evaluate determinism and overhead.
- Simulation metrics benchmark suite (measure overhead of observer on/off and randomized policy shuffle cost).

-- End of Backlog --
