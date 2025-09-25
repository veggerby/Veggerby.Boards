# EventKind Filtering Benchmark Notes

This document summarizes the intent and current status of the `EventKindFilteringBenchmark` suite.

## Purpose

Measure the impact of early EventKind-based rule gate filtering on:

1. Rule evaluation counts (observer captured)
2. Wall clock execution time (BenchmarkDotNet Mean)
3. Allocations (Gen0/Gen1 and total KB)

## Variants Implemented

| Variant | Composition | Description |
|---------|-------------|-------------|
| Legacy_NoFiltering_MoveBurst | 100% MovePiece | Baseline without event kind filtering. |
| Filtering_MoveBurst | 100% MovePiece | Same event stream with filtering enabled. |
| Mixed50_50_EvaluationCounts | 50% MovePiece / 50% NoOp | Heterogeneous stream; evaluation counts only. |
| Mixed80_20_EvaluationCounts | 80% MovePiece / 20% NoOp | Skewed toward move events. |
| Mixed20_80_EvaluationCounts | 20% MovePiece / 80% NoOp | Skewed toward non-move events. |

`NoOp` denotes the inert `BenchmarkStateNoOpEvent` (classified as a state mutation but performing no change) to diversify EventKind distribution without mutating board state.

## Methodology

All benchmarks construct a deterministic oscillating rook path (a1 <-> b1) to ensure each generated `MovePieceGameEvent` is legal relative to the evolving game state. The inert events are interleaved according to the selected ratio.

Rule evaluation counts are captured via a lightweight `IEvaluationObserver` implementation that increments per evaluated rule and provides the aggregate after execution.

## Status

All performance variants executed (2025-09-25). Filtering shows neutral to slight overhead in current distributions; evaluation pruning benefit not yet translating into faster wall-clock for tested ratios. Allocation footprint identical across paired variants (expected: filtering path does not allocate).

Captured metrics (Mean ± Error, StdDev omitted for brevity):

### MoveBurst (100% MovePiece)

| Variant | Mean | Delta vs Legacy | Gen0 | Gen1 | Allocated |
|---------|------|-----------------|------|------|-----------|
| Legacy_NoFiltering_MoveBurst | (existing) | – | (existing) | (existing) | (existing) |
| Filtering_MoveBurst | (existing) | ~+small overhead | (existing) | (existing) | (existing) |

### Mixed 50/50

| Variant | Mean | Delta vs Legacy | Gen0 | Gen1 | Allocated |
|---------|------|-----------------|------|------|-----------|
| Legacy_NoFiltering_Mixed50_50 | 286.348 µs | – | 18.0664 | 1.4648 | 335.28 KB |
| Filtering_Mixed50_50 | 287.766 µs | +0.5% | 18.0664 | 1.4648 | 335.28 KB |

### Mixed 80/20 (Move/NoOp)

| Variant | Mean | Delta vs Legacy | Gen0 | Gen1 | Allocated |
|---------|------|-----------------|------|------|-----------|
| Legacy_NoFiltering_Mixed80_20 | 467.440 µs | – | 29.2969 | 3.4180 | 543.39 KB |
| Filtering_Mixed80_20 | 472.369 µs | +1.06% | 29.2969 | 3.4180 | 543.39 KB |

### Mixed 20/80 (Move/NoOp)

| Variant | Mean | Delta vs Legacy | Gen0 | Gen1 | Allocated |
|---------|------|-----------------|------|------|-----------|
| Legacy_NoFiltering_Mixed20_80 | 122.870 µs | – | 7.3242 | 0.2441 | 137.94 KB |
| Filtering_Mixed20_80 | 117.251 µs | -4.58% | 7.4463 | 0.2441 | 137.94 KB |

Observations:

* Filtering overhead is slightly positive (slower) in high move-density streams (100%, 80/20, 50/50) where rule evaluation pruning is minimal.
* In the move-sparse 20/80 distribution filtering gains ~4.6% speedup, indicating pruning benefit outweighs branch cost when a majority of events are quickly dismissible by kind.
* Allocation parity across all variants confirms filtering branch adds no managed allocations.

Pending insertion: raw evaluation count deltas for mixed ratios (already captured in evaluation-count benchmarks) to quantify pruning vs timing correlation.

## Style & Determinism Assurance

* No LINQ in the hot loop generating events or applying them (tight for-loop used).
* Immutable event arrays built once per benchmark invocation.
* Rook movement chosen because it permits reversible oscillation without violating movement legality.
* Inert events do not alter state → deterministic outcome irrespective of their positions.

## Next Steps

1. Insert evaluation count totals & compute reduction % for each mixed ratio (50/50, 80/20, 20/80).
2. Add computed reduction ratios table correlating evaluation savings to wall-clock deltas.
3. Evaluate adding dice roll event heterogeneity only if ruleset composition materially differs (deferred).
4. Optional: isolate filtering branch microbenchmark (synthetic no-op rules) to measure pure dispatch cost.

---
Last updated: 2025-09-25
