# Feature Flags (DEPRECATED)

> **⚠️ NOTICE**: Feature flags have been eliminated from the production codebase. This document is retained for historical reference only.
>
> All features documented below are now unconditionally enabled based on their graduated status. The `FeatureFlags` class exists only as a test compatibility shim and will be removed in a future release.

## Migration Status

As of this release, all feature flags have been processed:

### ✅ Graduated Features (Always Enabled)
These features have been validated and are now unconditionally enabled in production code:

- **EnableCompiledPatterns** - Compiled movement patterns (DFA)
- **EnableBitboards** - Bitboard occupancy for boards ≤128 tiles
- **EnableStateHashing** - Deterministic state fingerprints
- **EnableSlidingFastPath** - Ray-based sliding piece resolution
- **EnableBitboardIncremental** - Incremental bitboard updates
- **EnableTurnSequencing** - Turn advancement system
- **EnableTraceCapture** - Evaluation trace diagnostics
- **EnableBoardShape** - O(1) neighbor lookup

### ❌ Removed Features (Deferred/Superseded)
These experimental features have been removed from production code:

- **EnableCompiledPatternsAdjacencyCache** - Superseded by BoardShape
- **EnableSegmentedBitboards** - No current use case
- **EnablePerPieceMasks** - Deferred to future optimization
- **EnableTopologyPruning** - Needs benchmarks + parity tests
- **EnableDecisionPlanGrouping** - Deferred to future perf story
- **EnableDecisionPlanEventFiltering** - Deferred to future perf story
- **EnableDecisionPlanMasks** - Deferred to future perf story
- **EnableTimelineZipper** - No usage, removed
- **EnableObserverBatching** - Deferred to future perf story

### ✅ Always Available
- **EnableSimulation** - Simulation APIs are always available; using the API is the explicit opt-in

## Historical Context

Feature flags were used during development to:
1. Enable gradual rollout of experimental features
2. Allow A/B testing of optimization paths
3. Maintain parity between old and new implementations
4. Facilitate performance benchmarking

The system has now matured to the point where:
- Graduated features have proven stable and performant
- All parity tests pass consistently
- Performance benchmarks meet thresholds
- Code complexity from conditional paths outweighs benefits

## Benefits of Elimination

1. **Parallel test execution** - No more global mutable state
2. **Reduced codebase** - ~500+ lines eliminated
3. **Clear code path** - Single implementation per feature
4. **Better performance** - No conditional overhead in hot paths
5. **Easier onboarding** - Obvious what's production-ready

## For Test Authors

The `FeatureFlags` class and `FeatureFlagScope` test helper remain temporarily for backward compatibility but are deprecated:

```csharp
// ❌ DEPRECATED - Will be removed
using var _ = new FeatureFlagScope(compiledPatterns: true);

// ✅ PREFERRED - Feature is always enabled, no scope needed
// (just remove the scope usage)
```

Tests should be gradually updated to remove `FeatureFlagScope` usage as the flags no longer affect production behavior.

---

**Last Updated**: 2025-11-14  
**Deprecated**: 2025-11-14  
**Removal Target**: Future release (TBD)
