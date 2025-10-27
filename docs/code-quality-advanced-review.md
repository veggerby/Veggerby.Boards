# Advanced Flow & Data Structure Review

Date: 2025-10-27
Branch: core/test-assertion-and-structure

## Purpose

Deeper analysis of non-optimal patterns in how components (builder, progress chain, decision plan, path resolution, simulation) interact. Focus on: unnecessary allocations, repeated linear searches, chaining LINQ in candidate hot paths, and unclear invariant vs defensive semantics.

## High-Impact Candidates

### 2. Event Chain Growth Using `progress.Events.Concat([evt])` (Deferred)

Occurrences in `GameProgress` and `GameSimulator`.
Issue: Each concat creates a new enumerable wrapper; eventual enumeration pays per-link overhead.
Opportunity:

- Internally store events as `ImmutableArray<IGameEvent>` or a custom `EventChain` struct with O(1) append snapshot.
- Alternatively maintain a `List<IGameEvent>` in progress and expose as `IReadOnlyList<IGameEvent>` (careful with immutability guarantee; require internal copy on append).
Impact: Reduced allocation and enumeration overhead for long histories.
Effort: M-L (needs design to preserve immutability semantics). Risk: must ensure deterministic snapshot semantics.

### 3. Composite Policy Enumeration in `GameSimulator` (Pending Profiling)

Materializes sequences with nested enumerator logic.
Issue: Complexity + potential multiple enumerations.
Opportunity:

- Streamlined approach: iterate policies; for each produce first candidate via `GetCandidateEvents(progress).GetEnumerator()`; if none, continue; otherwise accumulate via manual list fill.
Impact: Minor allocation reduction & clearer intent.
Effort: S.

### Removed Completed Items

The following prior review items have been delivered and are removed from active tracking:

- DecisionPlan early pruning of identical-condition groups (implemented).
- Pattern direction array caching baseline (compiler now caches identical direction sequences).
- Normalize helper adoption (builder & retrieval helpers enforce explicit guards; normalization no longer silently applied).
- Benchmark assertion hygiene (fallback exceptions replaced with Debug.Assert where appropriate).
- TilePath caching / LINQ removal (accessors allocation-free).
- Generic extras reflection removal (non-generic `ExtrasState` wrapper in place).
- Builder hot-path LINQ elimination for relation, piece, and pattern creation.

### 10. Bitboard Layout Ordering via LINQ Sort (Candidate)

`BoardBitboardLayout` orders tiles using `OrderBy` for deterministic indexing.
Issue: Allocations + O(n log n).
Opportunity:

- If tile definitions already inserted in deterministic order, rely on insertion order; else implement simple non-alloc counting sort if IDs numeric or maintain sorted insertion.
Impact: Perf improvement for large boards.
Effort: M (depends on ID structure).

### 11. Sliding Attack Generator Building Rays (Candidate)

`list.ToArray()` per ray.
Issue: Potential repeated dynamic resizing.
Opportunity:

- Pre-size `List<Tile>` using board dimension heuristics (max squares per ray) or use stackalloc + span when counts small.
Impact: Micro perf improvement across many rays.
Effort: M (careful with readability).

### 12. ConditionResponse Reason Aggregation (Watch)

`string.Join` with LINQ each invalid branch.
Issue: Allocation of array/iterator each time.
Opportunity:

- Manual `StringBuilder` (only if profiling shows hotspot) or maintain reason list earlier.
Impact: Possibly negligible; monitor with profiling.
Effort: S (defer until evidence).

### (Removed) Extras States Design

Replaced by non-generic typed `ExtrasState` with explicit `ExtrasType` metadata; reflection construction eliminated. No further action until profiling indicates lookup hotspot warranting dictionary caching.

### 14. Immutable Progress Event Storage (Research)

Current approach recomputes concatenations.
Opportunity:

- Implement persistent vector (e.g., simple tree of arrays) for O(log n) append with structural sharing; or adopt `ImmutableArray` with builder for finalization only when needed.
Impact: Reduced overhead for long histories (e.g., simulations with thousands of events).
Effort: L.

### (Removed) Benchmark Fallback Null Checks

Migrated to Debug.Asserts; item closed.

### 16. CompositeGameEventConditionDefinition Single Child Shortcut (Micro)

Calls `_childDefinitions.Single()`; still builds then returns default when null.
Issue: Type safety fine; performance minor.
Opportunity:

- Avoid `Single()` with direct index `[0]` after length guard.
Impact: Micro improvement.
Effort: S.

### 17. Repeated `.Any()` Checks (Micro)

E.g., builder `_activePlayerAssignments.Any()` then later enumerate.
Issue: Double enumeration for non-list sources.
Opportunity:

- Replace with `Count > 0` or store boolean flags if list enumerated frequently.
Impact: Micro improvement.
Effort: S.

### (Removed) Path Tile Access via LINQ

Resolved via cached arrays in `TilePath`; no further optimization needed presently.

### 19. Ordering for Min Distance With Sorting

Already covered by path resolution min search; ensure all similar `OrderBy(...).First()` cascades replaced (two visitors flagged).
Status: Merge with #6.

## Current Focus Sequence (Updated)

1. Bitboard incremental graduation & profiling (post soak).
2. DeckBuilding pile cloning optimization (profiling pending).
3. Simulation composite policy enumeration micro-alloc review.
4. Public API doc coverage harness (baseline test added, to be enforced later).
5. (Completed) Path resolver Try* pattern adoption (documentation updated; removed from active focus).

## Risk & Determinism Considerations

All proposed changes must preserve ordering assumptions used for deterministic replay (e.g., artifact sequence in `Game`). When replacing LINQ chains, ensure stable ordering identical to existing enumeration sequence.

## Measurement Plan (Before Large Refactors)

- Microbench path resolution before/after min search change.
- GameBuilder build time with large synthetic set (N tiles, M pieces) before/after dictionary id lookups.
- Simulation event throughput (events applied per second) before persistent event chain implementation.

## Summary

Prior high-impact optimization tasks are complete. Remaining items target incremental profiling-informed refinement (bitboard incremental path, deck pile cloning) and API clarity (Try* adoption). Determinism and immutability remain intact.

---
One-liner: Most structural hotspots closed; next refinements will be profiling-driven (bitboards, deck piles) and API clarity (Try* path resolution) while preserving deterministic order.
