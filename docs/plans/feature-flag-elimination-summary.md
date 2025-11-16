# Feature Flag Elimination - Summary

## Problem Statement

The current `FeatureFlags` system creates significant technical debt:

1. **Global mutable state** prevents parallel test execution
2. **18 feature flags** with unclear maturity (6 graduated, 12 experimental)
3. **44 conditional checks** in production code create maintenance burden
4. **200+ test uses** of `FeatureFlagScope` add complexity
5. **Multiple code paths** (legacy + optimized) increase cognitive load

## Proposed Solution

**Complete elimination of all feature flags** through a 3-4 week migration:

### Graduated Flags → Unconditional Baseline (Remove flags)

- `EnableCompiledPatterns`, `EnableBitboards`, `EnableStateHashing`
- `EnableSlidingFastPath`, `EnableBitboardIncremental`, `EnableTurnSequencing`

**Action**: Make these features always-on; delete legacy fallback paths

### Experimental Flags → Decide fate (Graduate / Remove / Defer)

- **Graduate**: `EnableTraceCapture`, `EnableBoardShape` (promote to baseline)
- **Remove**: `EnableCompiledPatternsAdjacencyCache`, `EnableSegmentedBitboards` (no value)
- **Defer**: Decision plan optimizations, per-piece masks, topology pruning (needs dedicated work)

**Action**: Either stabilize or delete; no more runtime toggles

### Test Infrastructure → Complete cleanup

- Delete `FeatureFlagScope` (no longer needed without flags)
- Remove ~50+ flag-combination parity tests
- Enable parallel test execution

## Benefits

1. **Cleaner codebase**: ~500+ lines removed (flags, conditionals, test scaffolding)
2. **Simpler mental model**: One code path, not 2^18 combinations
3. **Parallel testing**: No global state serialization
4. **Better onboarding**: Clear what's "real" vs experimental
5. **Performance**: Eliminate conditional overhead in hot paths

## Migration Plan

**Week 1**: Inventory + decision matrix for all 18 flags
**Week 2**: Graduate 6 core flags (incremental validation)
**Week 3**: Process 12 experimental flags (graduate 2, remove 2, defer 8)
**Week 4**: Delete `FeatureFlags.cs`, cleanup tests, validate CI

## Acceptance Criteria

- Zero `FeatureFlags` references in codebase
- All tests pass (minus deleted flag-toggle tests)
- Benchmarks show no performance regression (±2%)
- Cross-platform CI validates determinism (Linux, Windows, macOS)
- Documentation updated (CHANGELOG, migration guide)

## Full Details

See `/docs/plans/stories/014-eliminate-feature-flags.md` for comprehensive plan including:

- Detailed flag-by-flag decisions
- Migration patterns and code examples
- Risk mitigation strategies
- Testing approach
- Follow-up work for deferred features

---

**Created**: 2025-11-14
**Estimated Effort**: 3-4 weeks
**Priority**: High (technical debt + maintainability)
**Status**: Ready for execution
