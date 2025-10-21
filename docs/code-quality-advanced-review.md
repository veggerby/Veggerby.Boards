# Advanced Flow & Data Structure Review

Date: 2025-10-21
Branch: core/test-assertion-and-structure

## Purpose

Deeper analysis of non-optimal patterns in how components (builder, progress chain, decision plan, path resolution, simulation) interact. Focus on: unnecessary allocations, repeated linear searches, chaining LINQ in candidate hot paths, and unclear invariant vs defensive semantics.

## High-Impact Candidates

### 2. Event Chain Growth Using `progress.Events.Concat([evt])`

Occurrences in `GameProgress` and `GameSimulator`.
Issue: Each concat creates a new enumerable wrapper; eventual enumeration pays per-link overhead.
Opportunity:

- Internally store events as `ImmutableArray<IGameEvent>` or a custom `EventChain` struct with O(1) append snapshot.
- Alternatively maintain a `List<IGameEvent>` in progress and expose as `IReadOnlyList<IGameEvent>` (careful with immutability guarantee; require internal copy on append).
Impact: Reduced allocation and enumeration overhead for long histories.
Effort: M-L (needs design to preserve immutability semantics). Risk: must ensure deterministic snapshot semantics.

### 3. Composite Policy Enumeration in `GameSimulator`

Materializes sequences with nested enumerator logic.
Issue: Complexity + potential multiple enumerations.
Opportunity:

- Streamlined approach: iterate policies; for each produce first candidate via `GetCandidateEvents(progress).GetEnumerator()`; if none, continue; otherwise accumulate via manual list fill.
Impact: Minor allocation reduction & clearer intent.
Effort: S.

### 4. Decision Plan Early Pruning (Residual)

Capacity hints have been added (entries, kinds, exclusivity) eliminating dynamic growth concern. Remaining potential: evaluate early pruning of empty rule groups to skip temporary list fill. Retain only if profiling shows material impact.

Status: Candidate (profiling pending). Effort: S.

### 5. Pattern Compilation ToArray Calls

`PatternCompiler`: converting pattern directions to arrays repeatedly.
Issue: Each compiled pattern duplicates direction arrays.
Opportunity:

- Cache compiled direction arrays if identical sequences encountered multiple times.
Impact: Reduces repeated allocations in large pattern sets.
Effort: M (requires equality + caching keyed by sequence hash).

### 9. Normalize Helper Adoption

`Normalize.Text/List` unused; still manual `?? string.Empty` scattered.
Issue: Slight duplication; risk of inconsistent patterns later.
Opportunity:

- Standardize ingestion points (builder inputs, external API boundaries).
Impact: Consistency & future adaptation ease.
Effort: S.

### 10. Bitboard Layout Ordering via LINQ Sort

`BoardBitboardLayout` orders tiles using `OrderBy` for deterministic indexing.
Issue: Allocations + O(n log n).
Opportunity:

- If tile definitions already inserted in deterministic order, rely on insertion order; else implement simple non-alloc counting sort if IDs numeric or maintain sorted insertion.
Impact: Perf improvement for large boards.
Effort: M (depends on ID structure).

### 11. Sliding Attack Generator Building Rays

`list.ToArray()` per ray.
Issue: Potential repeated dynamic resizing.
Opportunity:

- Pre-size `List<Tile>` using board dimension heuristics (max squares per ray) or use stackalloc + span when counts small.
Impact: Micro perf improvement across many rays.
Effort: M (careful with readability).

### 12. ConditionResponse Reason Aggregation

`string.Join` with LINQ each invalid branch.
Issue: Allocation of array/iterator each time.
Opportunity:

- Manual `StringBuilder` (only if profiling shows hotspot) or maintain reason list earlier.
Impact: Possibly negligible; monitor with profiling.
Effort: S (defer until evidence).

### 13. Extras States Design (`_extrasStates`)

Generic `IList<object>`.
Issue: Type safety + ambiguous lifecycle (TODO exists).
Opportunity:

- Introduce typed registry: `Dictionary<string, object>` or generic `ExtraState<T>` wrappers; document usage scenarios (diagnostics vs gameplay).
Impact: Clarity & safer future extension.
Effort: M.

### 14. Immutable Progress Event Storage

Current approach recomputes concatenations.
Opportunity:

- Implement persistent vector (e.g., simple tree of arrays) for O(log n) append with structural sharing; or adopt `ImmutableArray` with builder for finalization only when needed.
Impact: Reduced overhead for long histories (e.g., simulations with thousands of events).
Effort: L.

### 15. Defensive Null Checks in Benchmarks Fallback Paths

`InvalidOperationException` when mid tile missing (setup issue).
Issue: Exception overhead in microbench; might skew measurement.
Opportunity:

- Replace with `Debug.Assert` + direct retrieval (assuming test harness always sets up board correctly).
Impact: Cleaner benchmark timing.
Effort: S.

### 16. CompositeGameEventConditionDefinition Single Child Shortcut

Calls `_childDefinitions.Single()`; still builds then returns default when null.
Issue: Type safety fine; performance minor.
Opportunity:

- Avoid `Single()` with direct index `[0]` after length guard.
Impact: Micro improvement.
Effort: S.

### 17. Repeated `.Any()` Checks

E.g., builder `_activePlayerAssignments.Any()` then later enumerate.
Issue: Double enumeration for non-list sources.
Opportunity:

- Replace with `Count > 0` or store boolean flags if list enumerated frequently.
Impact: Micro improvement.
Effort: S.

### 18. Path Tile Access via LINQ

`TilePath.Tiles => [Relations.First().From, .. Relations.Select(x => x.To)]`
Issue: Allocates enumerator per property access; might appear in loops.
Opportunity:

- Provide `Span<Tile>` or `ReadOnlyMemory<Tile>` generation once; or expose enumerator struct avoiding allocations.
Impact: Perf improvement in tight loops.
Effort: M.

### 19. Ordering for Min Distance With Sorting

Already covered by path resolution min search; ensure all similar `OrderBy(...).First()` cascades replaced (two visitors flagged).
Status: Merge with #6.

## Current Focus Sequence

1. Pattern direction array caching (avoid duplicate allocations when identical sequences repeat).
2. DecisionPlan early pruning (profiling gated).
3. Normalize helper broad adoption.
4. Benchmark fallback asserts conversion.
5. Path tile access optimization (span/enumerator) after profiling.

## Risk & Determinism Considerations

All proposed changes must preserve ordering assumptions used for deterministic replay (e.g., artifact sequence in `Game`). When replacing LINQ chains, ensure stable ordering identical to existing enumeration sequence.

## Measurement Plan (Before Large Refactors)

- Microbench path resolution before/after min search change.
- GameBuilder build time with large synthetic set (N tiles, M pieces) before/after dictionary id lookups.
- Simulation event throughput (events applied per second) before persistent event chain implementation.

## Summary

Most high-impact build/resolution optimizations applied; focus shifts to pattern direction caching and selective pruning guided by profiling.

---
One-liner: Replace sorting and linear scans with specialized constant-time structures and eliminate chained enumerables to sharpen performance while maintaining deterministic order.
