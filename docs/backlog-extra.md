# Extra Backlog (Out-of-Plan Items)

This file tracks auxiliary improvement ideas not explicitly covered in `docs/plans/action-plan.md`. Items here are lower priority or opportunistic; they may graduate into the formal plan once justified.

## Observability Enhancements

- PhaseEnter callback in `IEvaluationObserver` (emit before evaluating any rule when phase deemed active). **COMPLETED**
- StateHashed callback (after future Merkle/xxHash computation) for correlation. **COMPLETED**
- In-memory trace capture scaffold (last evaluation) **COMPLETED**
- JSON trace serializer / exporter (compact) **PENDING**
- Trace overhead microbenchmark (compare enabled vs disabled) **PENDING**
- Trace entry object pooling / reuse strategy (avoid per-callback alloc) **PENDING**
- Aggregated batch observer adapter reducing callback overhead via struct buffer.
- Counting observer benchmark (baseline) — COMPLETED (see `ObserverOverheadBenchmark`).
- Overhead target doc update once hashing added.

## Parity & Testing Utilities

- Golden parity harness comparing legacy vs DecisionPlan across randomized event streams (property-based).
- Test helper: `using FeatureFlagScope(decisionPlan: true)` disposable to restore flags automatically.
- Multi-event deterministic parity test sequence — COMPLETED.
- Replay harness for BugReport captured event streams **PENDING**
- Dual-run test asserting trace sequence identical with/without hashing enabled **PENDING**

## Performance Tooling

- Microbenchmark variant with observer enabled vs disabled to measure overhead delta.
- Microbenchmark variant with trace capture enabled vs disabled (overhead quantification) **PENDING**
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
