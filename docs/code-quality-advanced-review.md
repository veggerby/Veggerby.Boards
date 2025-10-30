# Advanced Flow & Data Structure Review

Date: 2025-10-27
Branch: core/test-assertion-and-structure

## Purpose

Deeper analysis of non-optimal patterns in how components (builder, progress chain, decision plan, path resolution, simulation) interact. Focus on: unnecessary allocations, repeated linear searches, chaining LINQ in candidate hot paths, and unclear invariant vs defensive semantics.

## High-Impact Candidates

### 1. Sliding Attack Generator Building Rays (Candidate)

`list.ToArray()` per ray.
Issue: Potential repeated dynamic resizing.
Update:

- Replaced per-ray `List<short>` + `HashSet<int>` with shared buffer + boolean visited array; eliminated repeated allocations.

Remaining Opportunity:

- Explore stackalloc for buffer when `TileCount <= 128` and evaluate impact vs managed array reuse.
- Consider emitting rays as contiguous slices (start,length) instead of per-ray array for further allocation reduction.
Impact: Micro perf improvement across many rays (pending benchmark quantification).
Effort: M (further refinements) / S (additional minor tweaks).

### 2. ConditionResponse Reason Aggregation (Watch)

`string.Join` with LINQ each invalid branch.
Issue: Allocation of array/iterator each time.
Opportunity:

- Manual `StringBuilder` (only if profiling shows hotspot) or maintain reason list earlier.
Impact: Possibly negligible; monitor with profiling.
Effort: S (defer until evidence).

### 3. Immutable Progress Event Storage (Research)

Current approach recomputes concatenations.
Opportunity:

- Implement persistent vector (e.g., simple tree of arrays) for O(log n) append with structural sharing; or adopt `ImmutableArray` with builder for finalization only when needed.
Impact: Reduced overhead for long histories (e.g., simulations with thousands of events).
Effort: L.

### 4. CompositeGameEventConditionDefinition Single Child Shortcut (Micro)

Calls `_childDefinitions.Single()`; still builds then returns default when null.
Issue: Type safety fine; performance minor.
Opportunity:

- Avoid `Single()` with direct index `[0]` after length guard.
Impact: Micro improvement.
Effort: S.

### 5. Repeated `.Any()` Checks (Micro)

E.g., builder `_activePlayerAssignments.Any()` then later enumerate.
Issue: Double enumeration for non-list sources.
Opportunity:

- Replace with `Count > 0` or store boolean flags if list enumerated frequently.
Impact: Micro improvement.
Effort: S.

### 6. Ordering for Min Distance With Sorting

Already covered by path resolution min search; ensure all similar `OrderBy(...).First()` cascades replaced (two visitors flagged).
Status: Merge with #6.

## Current Focus Sequence (Updated)

1. DeckBuilding pile cloning struct abstraction (profiling pending) – selective cloning delivered.
2. Public API doc coverage harness (enforced via test, now maintenance mode).
3. Sliding attack generator micro-alloc improvements (profiling scaffold added).

## Risk & Determinism Considerations

All proposed changes must preserve ordering assumptions used for deterministic replay (e.g., artifact sequence in `Game`). When replacing LINQ chains, ensure stable ordering identical to existing enumeration sequence.

## Measurement Plan (Before Large Refactors)

- Microbench path resolution before/after min search change.
- GameBuilder build time with large synthetic set (N tiles, M pieces) before/after dictionary id lookups.
- Simulation event throughput (events applied per second) before persistent event chain implementation.

## Summary

Remaining items target incremental profiling-informed refinement (bitboard incremental path, deck pile cloning) and API clarity. Determinism and immutability remain intact.

---
One-liner: Outstanding refinement opportunities now center on profiling-driven bitboard/deck pile optimization and clearer enumeration/storage semantics while preserving deterministic order.
