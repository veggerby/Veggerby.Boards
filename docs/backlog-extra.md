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
- Counting observer benchmark (baseline) — COMPLETED (see `ObserverOverheadBenchmark`).
- Overhead target doc update once hashing added.

## Parity & Testing Utilities

- Golden parity harness comparing legacy vs DecisionPlan across randomized event streams (property-based). **PARTIAL – deterministic harness + randomized short pawn advance sequence scaffold added; broader property distributions (captures, promotions, dice modules) pending**
- Test helper: `using FeatureFlagScope(decisionPlan: true)` disposable to restore flags automatically. **COMPLETED**
- Multi-event deterministic parity test sequence — COMPLETED.
- Movement pattern parity scaffold (legacy visitor vs compiled resolver pre-population) **COMPLETED**
- Restore and expand compiled pattern parity tests (legacy visitor multi-direction repeat + chess pawn double move) after investigating legacy visitor null path edge case. **PENDING**
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
-- Event filtering evaluation fast-path (skip non-matching kinds before predicate). **COMPLETED (flag gated + initial tests + baseline benchmark scaffold)**
  - Follow-up: Expand rule tagging coverage, add mixed-kind multi-phase benchmark & debug parity shadow path. **IN PROGRESS – taxonomy expanded (State/Phase), marker interfaces now public, concrete `SelectActivePlayerGameEvent` added; filtering tests & benchmarks pending**
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

-- End of Backlog --
