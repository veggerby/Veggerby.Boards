# Deferred Features from Feature Flag Elimination

**Date**: 2025-11-14  
**Related Issue**: #35 - Eliminate Feature Flags & Stabilize Engine Baseline  
**Status**: Documented for future work

## Overview

During the feature flag elimination effort (#35), several experimental features were intentionally deferred to future work rather than graduated or removed entirely. This document captures the context, rationale, and recommended next steps for each deferred feature to ensure knowledge retention.

---

## Deferred Performance Optimizations

### 1. Decision Plan Optimizations

**Feature Flags Removed**:
- `EnableDecisionPlanGrouping`
- `EnableDecisionPlanEventFiltering`
- `EnableDecisionPlanMasks`

**Description**:
These three flags controlled optimization layers for the DecisionPlan evaluation engine:

1. **Grouping**: Evaluate shared gate predicates once per group instead of per-entry to reduce redundant condition checks
2. **Event Filtering**: Pre-filter plan entries by EventKind before predicate evaluation to skip irrelevant rules early
3. **Exclusivity Masks**: Skip subsequent entries in the same exclusivity group once one rule has applied

**Why Deferred**:
- Optimizations add complexity with multiple conditional paths
- Current simple linear scan is sufficient for existing game complexity
- Need dedicated performance story with:
  - Comprehensive benchmarks showing measurable wins
  - Large decision plans (100+ entries) to justify overhead
  - Parity tests proving semantic equivalence
  - Memory/allocation profiling

**Current State**:
- Production code uses simple linear scan through DecisionPlan.Entries
- All condition evaluation happens per-entry
- No grouping or filtering optimizations active

**Recommended Next Steps**:
1. **Benchmark First**: Establish baseline performance with current linear scan
2. **Identify Bottlenecks**: Profile real-world games with large rule sets
3. **Targeted Optimization**: Only re-introduce if profiling shows >10% time in plan evaluation
4. **Implementation Approach**: Use strategy pattern instead of feature flags
   - Create `IDecisionPlanEvaluator` interface
   - Implement `LinearEvaluator` (current), `GroupedEvaluator`, `FilteredEvaluator`
   - Select evaluator at engine build time based on plan characteristics

**Future Story Template**:
```
Title: Decision Plan Performance Optimization
Priority: Low (until profiling shows need)
Effort: 2-3 weeks
Acceptance Criteria:
- Benchmark suite shows >=15% improvement in plan evaluation
- Parity tests validate semantic equivalence
- Strategy pattern eliminates need for runtime flags
- Documentation includes when to use each evaluator strategy
```

---

### 2. Observer Batching

**Feature Flag Removed**: `EnableObserverBatching`

**Description**:
Batch high-frequency evaluation observer callbacks (phase enter, rule evaluated, rule skipped) and flush when terminal outcome is reached or buffer capacity exceeded. Reduces delegate invocation and virtual dispatch overhead.

**Why Deferred**:
- Current direct dispatch has acceptable performance for most use cases
- Batching adds complexity and potential ordering concerns
- Needs validation that batching preserves deterministic ordering
- Overhead only measurable with very large decision plans (100+ entries)

**Current State**:
- All observer callbacks dispatched immediately (direct)
- No buffering or batching layer

**Recommended Next Steps**:
1. **Profile First**: Measure observer callback overhead in real workloads
2. **Validate Ordering**: Ensure batched dispatch maintains deterministic callback sequence
3. **Benchmark Threshold**: Only implement if observer callbacks consume >5% of evaluation time
4. **Implementation**: Decorator pattern over `IEvaluationObserver`
   - `BatchingObserverDecorator` wraps base observer
   - Configurable buffer size and flush triggers
   - Optional - enable via explicit observer wrapping, not flags

**Future Story Template**:
```
Title: Observer Batching for High-Frequency Callbacks
Priority: Low (optimization only)
Effort: 1-2 weeks
Acceptance Criteria:
- <=5% overhead for small plans (verified)
- >=10% speedup for large plans (100+ entries)
- Deterministic callback ordering preserved
- Decorator pattern, explicit opt-in
```

---

## Deferred Bitboard Optimizations

### 3. Per-Piece Occupancy Masks

**Feature Flag Removed**: `EnablePerPieceMasks`

**Description**:
Maintain individual occupancy masks (bitboards) for each piece identity in addition to global and per-player masks. Enables fine-grained attack pruning and mobility heuristics without piece-by-piece iteration.

**Why Deferred**:
- Memory overhead: O(piece count) additional 64-bit masks
- Current piece map provides tile→piece lookup efficiently enough
- No consumer code currently leverages per-piece masks
- Needs profiling to show tangible benefit vs allocation cost

**Current State**:
- `BitboardOccupancyIndex.PieceMask()` always returns 0 (no-op)
- Per-piece mask initialization and updates removed
- Piece location queries use piece map, not bitboards

**Recommended Next Steps**:
1. **Identify Use Case**: Find scenario where per-piece masks provide measurable value
   - Attack/defense calculations needing piece-specific occupancy?
   - Mobility heuristics for AI evaluation?
2. **Benchmark Allocation**: Measure memory impact for typical games (32 pieces = 256 bytes)
3. **Lazy Initialization**: Only build masks when explicitly requested
4. **API Design**: Extension method or capability flag, not global feature flag

**Future Story Template**:
```
Title: Per-Piece Bitboard Masks for Advanced Heuristics
Priority: Low (no current consumer)
Effort: 1 week
Acceptance Criteria:
- Identified consumer scenario (AI, mobility calc, etc.)
- Negligible allocation overhead (<1% memory increase)
- Benchmark shows measurable benefit (>=5% speedup in consumer)
- Lazy initialization pattern, explicit opt-in
```

---

### 4. Segmented Bitboards

**Feature Flag Removed**: `EnableSegmentedBitboards`

**Description**:
Unified segmented bitboard abstraction (inline up to 4 segments, spill to heap beyond) to support boards >128 tiles with consistent API. Alternative to current 64/128-bit specialized types.

**Why Deferred**:
- Current Bitboard64 and Bitboard128 cover all existing games (Chess, Backgammon, Go on 19×19)
- No production games require >128 tiles
- Added complexity without clear use case
- Memory/performance tradeoffs need validation

**Current State**:
- `Bitboard64` for boards ≤64 tiles
- `Bitboard128` for boards 65-128 tiles
- Segmented bitboard experimental code removed
- No support for boards >128 tiles (fallback to naive occupancy)

**Recommended Next Steps**:
1. **Wait for Use Case**: Defer until a game module needs >128 tiles
2. **Design First**: Model API before implementation
   - Consistent interface across all sizes
   - Inline optimization for small boards (≤4 segments)
   - Benchmark memory/speed tradeoffs
3. **Incremental**: Add Bitboard256 if needed, then generalize if pattern emerges

**Future Story Template**:
```
Title: Support for Boards >128 Tiles (Segmented Bitboards)
Priority: Blocked (no use case)
Effort: 2-3 weeks
Acceptance Criteria:
- Real game module requires >128 tiles
- Benchmark shows acceptable performance (within 20% of Bitboard128)
- Memory profile shows acceptable overhead
- Parity tests across all sizes (64/128/256/larger)
```

---

### 5. Topology Pruning

**Feature Flag Removed**: `EnableTopologyPruning`

**Description**:
Skip precomputation and lookup for directions not present in the board's `BoardTopology` classification. Reduces iteration and branching in mixed-topology boards (e.g., hex + orthogonal).

**Why Deferred**:
- Current games use uniform topologies (all orthogonal or all diagonal)
- Pruning optimization only helps mixed-topology boards
- Needs parity tests to ensure no false negatives
- Benefit unclear without mixed-topology use case

**Current State**:
- All direction lookups check full direction set
- No pruning based on topology classification
- Works correctly for all current games

**Recommended Next Steps**:
1. **Wait for Mixed-Topology Game**: No current game benefits
2. **Benchmark First**: Measure direction lookup cost in mixed scenarios
3. **Parity Tests**: Ensure pruning doesn't skip valid directions
4. **Consider Alternative**: Precomputed direction masks per topology in BoardShape

**Future Story Template**:
```
Title: Topology-Based Direction Pruning
Priority: Low (no current use case)
Effort: 1-2 weeks
Acceptance Criteria:
- Mixed-topology game module exists
- Parity tests validate no false negatives
- Benchmark shows measurable reduction in lookups (>=10%)
- Integration with BoardShape direction caching
```

---

## Removed Features (No Future Work Planned)

### Timeline Zipper

**Feature Flag Removed**: `EnableTimelineZipper`

**Description**: Immutable zipper data structure for undo/redo navigation of game history.

**Why Removed Entirely**:
- No production code usage
- External history management (e.g., UI-level undo stack) more appropriate
- Adds complexity without clear engine-level benefit
- Undo/redo better handled in presentation layer, not core engine

**Recommendation**: **No future work**. If undo/redo needed, implement in consumer layer using GameProgress history chain.

---

### Compiled Pattern Adjacency Cache

**Feature Flag Removed**: `EnableCompiledPatternsAdjacencyCache`

**Description**: Pre-built (tile, direction) → relation cache for compiled pattern resolver.

**Why Removed Entirely**:
- Fully superseded by `BoardShape` O(1) neighbor lookup (now graduated)
- BoardShape provides same functionality with better API
- No benefit over BoardShape for supported topologies

**Recommendation**: **No future work**. BoardShape is the graduated solution.

---

## Summary Table

| Feature | Status | Priority | Blocking Factor | Next Action |
|---------|--------|----------|-----------------|-------------|
| Decision Plan Optimizations | Deferred | Low | Need profiling showing >10% time in plan eval | Benchmark first |
| Observer Batching | Deferred | Low | Need profiling showing >5% observer overhead | Benchmark first |
| Per-Piece Masks | Deferred | Low | No current consumer use case | Wait for consumer |
| Segmented Bitboards | Deferred | Blocked | No game >128 tiles | Wait for use case |
| Topology Pruning | Deferred | Low | No mixed-topology game | Wait for use case |
| Timeline Zipper | **Removed** | N/A | Not engine responsibility | No future work |
| Adjacency Cache | **Removed** | N/A | Superseded by BoardShape | No future work |

---

## Implementation Guidance for Future Work

When re-introducing any deferred feature:

1. **No Feature Flags**: Use dependency injection, strategy pattern, or explicit API opt-in
2. **Benchmark First**: Establish baseline and prove need with profiling
3. **Parity Tests**: Validate semantic equivalence with existing implementation
4. **Documentation**: Include when/why to use, performance characteristics, tradeoffs
5. **Incremental**: Start with smallest useful implementation, expand only if needed

---

## Related Documentation

- `docs/feature-flags.md` - Deprecated feature flag reference
- `CHANGELOG.md` - Breaking changes from flag elimination
- `docs/performance.md` - Performance architecture and optimization guidance
- Issue #35 - Original feature flag elimination story

---

**Maintainer Notes**:
- Review this document quarterly to assess if deferred features are still relevant
- Update priority/status when new game modules or profiling data changes context
- Consider archiving entries that remain unused for >1 year
