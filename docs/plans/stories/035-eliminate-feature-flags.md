---
story_id: 035
title: "Eliminate Feature Flags & Stabilize Engine Baseline"
status: planned
priority: high
tier: foundational
estimated_effort: 3-4 weeks
workstreams: [4, 6, 7, 8, 9]
dependencies: []
related_issues: [35]
labels: [technical-debt, maintainability, performance, determinism]
owner: core
created: 2025-11-14
updated: 2025-11-14
---

# Eliminate Feature Flags & Stabilize Engine Baseline

## Overview

Remove all feature flags from `FeatureFlags.cs` and eliminate conditional compilation paths throughout the engine. This represents a **major technical debt reduction** and baseline stabilization effort. Each graduated/stable feature should become the unconditional implementation; all experimental features should either be promoted to stable (with tests/docs), removed entirely, or deferred to future dedicated work.

The current feature flag system creates:

- **Global mutable state** that prevents parallel test execution
- **Code complexity** with multiple conditional paths that increase cognitive load
- **Maintenance burden** with duplicate code paths (legacy vs optimized)
- **Test fragility** requiring careful scope management to avoid interference
- **Unclear maturity signals** about which features are production-ready

## Background

### Current Feature Flag Inventory (18 flags)

**Graduated (Default ON, Should Remove Flag):**

1. `EnableCompiledPatterns` = true ✅ (visitor fallback exists)
2. `EnableBitboards` = true ✅ (boards ≤128 tiles)
3. `EnableStateHashing` = true ✅ (cross-platform validated)
4. `EnableSlidingFastPath` = true ✅ (4.6× speedup validated)
5. `EnableBitboardIncremental` = true ✅ (10k+ move soak tests pass)
6. `EnableTurnSequencing` = true ✅ (rotation validated)

**Stable (Default ON, Keep Unconditionally or Make Configurable):**

- None currently in this category

**Experimental (Default OFF, Decision Needed):**
7. `EnableDecisionPlanGrouping` = false ⚠️
8. `EnableDecisionPlanEventFiltering` = false ⚠️
9. `EnableDecisionPlanMasks` = false ⚠️
10. `EnableTimelineZipper` = false ⚠️
11. `EnableTraceCapture` = false ⚠️
12. `EnableSimulation` = false ⚠️
13. `EnableCompiledPatternsAdjacencyCache` = false ⚠️
14. `EnableBoardShape` = false ⚠️
15. `EnablePerPieceMasks` = false ⚠️
16. `EnableSegmentedBitboards` = false ⚠️
17. `EnableTopologyPruning` = false ⚠️
18. `EnableObserverBatching` = false ⚠️

### Code Impact Analysis

**44 usage sites in src/ across:**

- `GameBuilder.cs`: Service registration gating (6 checks)
- `GameProgress.cs`, `GameProgress.Legacy.cs`: Evaluation paths (5 checks)
- `GameExtensions.*.cs`: Acceleration path selection (6 checks)
- `CompiledPatternResolver.cs`: Neighbor lookup strategy (3 checks)
- Turn mutators: Sequencing enablement (4 checks)
- Bitboard subsystems: Incremental updates, per-piece masks (4 checks)
- Simulation: Feature gating (4 checks)
- Observers/diagnostics: Batching, trace capture (2 checks)
- Misc internal: Segmented bitboards, topology pruning (10 checks)

**200+ test usages of `FeatureFlagScope`:**

- Parity tests comparing acceleration on/off
- Hash stability tests
- Integration tests validating specific feature combinations
- Concurrency tests (which exposed the global state problem)

### Problems with Current Architecture

1. **Prevents Parallel Testing**: Global static flags + `FeatureFlagScope` serialization means tests using scopes cannot run concurrently
2. **Code Duplication**: Many files have legacy + optimized paths with feature flag conditionals
3. **Unclear Baseline**: New contributors don't know which code paths are "real" vs experimental
4. **Maintenance Burden**: Every change must consider multiple feature combinations
5. **Performance Regression Risk**: Conditional checks in hot paths (even if predictable)
6. **Testing Explosion**: Parity tests must validate flag combinations rather than a single truth

## Acceptance Criteria

### Phase 1: Graduated Features (Unconditional)

- [ ] Remove `EnableCompiledPatterns` flag
  - [ ] Delete visitor pattern fallback code or clearly mark as legacy/troubleshooting only
  - [ ] Remove conditional service registration in `GameBuilder`
  - [ ] Update all extension methods to use compiled resolver unconditionally
  - [ ] Remove parity tests comparing compiled vs visitor (keep visitor tests if retained for edge cases)

- [ ] Remove `EnableBitboards` flag
  - [ ] Always register bitboard services for boards ≤128 tiles
  - [ ] Remove conditional paths in `GameExtensions.Bitboards.cs`
  - [ ] Simplify `BitboardOccupancyIndex` construction (no flag checks)

- [ ] Remove `EnableStateHashing` flag
  - [ ] Always compute hashes in `GameState` construction
  - [ ] Remove conditional hash checks in `GameProgress` evaluation paths
  - [ ] Update tests to always assert hash presence (graduated feature)

- [ ] Remove `EnableSlidingFastPath` flag
  - [ ] Always use sliding ray generator when services available
  - [ ] Remove fallback conditional in `GameExtensions.Compiled.cs`
  - [ ] Keep service registration conditional on bitboards + topology (architectural, not flag-based)

- [ ] Remove `EnableBitboardIncremental` flag
  - [ ] Always use incremental bitboard updates in `BitboardAccelerationContext`
  - [ ] Remove full-rebuild fallback code path
  - [ ] Delete soak test flag toggling (graduated = always incremental)

- [ ] Remove `EnableTurnSequencing` flag
  - [ ] Always include turn sequencing in `GameBuilder`
  - [ ] Remove conditional checks in turn mutators
  - [ ] Update all game module builders to include turn profiles unconditionally

### Phase 2: Experimental Features (Evaluate & Decide)

For each experimental flag, **one of three outcomes**:

#### A. **Graduate** (promote to unconditional baseline)

Criteria: Parity tests pass, performance validated, docs exist, no known issues
Actions: Remove flag, make unconditional, update tests

#### B. **Remove** (delete experimental code entirely)

Criteria: No active use, unclear benefit, or superseded by another approach
Actions: Delete conditional code, remove tests, update docs

#### C. **Defer** (keep as disabled, document why, create dedicated future story)

Criteria: Promising but needs more work; not blocking current baseline
Actions: Keep code, document status, create follow-up issue, do NOT use flags going forward

**Specific Decisions Needed:**

- [ ] **Decision Plan Optimization Flags** (Grouping, EventFiltering, Masks)
  - Recommendation: **DEFER** - requires dedicated performance optimization story with comprehensive benchmarks
  - Action: Create separate story for decision plan acceleration (out of scope for this issue)
  - Keep code but add prominent comments: "Experimental - requires dedicated optimization pass"

- [ ] **EnableTimelineZipper**
  - Recommendation: **DEFER** or **REMOVE** - unclear if needed vs external history management
  - Action: Assess if any consumers rely on zipper; if not, delete

- [ ] **EnableTraceCapture**
  - Recommendation: **GRADUATE** (already integrated with observers)
  - Action: Remove flag, make observer-based trace capture standard (always available when observer configured)

- [ ] **EnableSimulation**
  - Recommendation: **KEEP GATED** but NOT via feature flags
  - Action: Move to explicit opt-in via API design (e.g., simulator constructors can check for required services, throw if missing)

- [ ] **EnableCompiledPatternsAdjacencyCache**
  - Recommendation: **REMOVE** - superseded by `EnableBoardShape`
  - Action: Delete adjacency cache code, update docs, rely on BoardShape for O(1) lookups

- [ ] **EnableBoardShape**
  - Recommendation: **GRADUATE**
  - Action: Always register BoardShape service when board topology supports it; remove flag

- [ ] **EnablePerPieceMasks**
  - Recommendation: **DEFER** - needs dedicated bitboard optimization story
  - Action: Keep disabled, document as future optimization opportunity

- [ ] **EnableSegmentedBitboards**
  - Recommendation: **REMOVE** - no current use case, adds complexity
  - Action: Delete segmented bitboard code (reintroduce if larger boards needed in future)

- [ ] **EnableTopologyPruning**
  - Recommendation: **DEFER** - needs benchmarks + parity tests
  - Action: Keep as commented-out optimization opportunity; remove flag

- [ ] **EnableObserverBatching**
  - Recommendation: **DEFER** - needs performance validation
  - Action: Keep disabled; requires dedicated observer optimization story

### Phase 3: Test Cleanup

- [ ] Remove `FeatureFlagScope` helper entirely
- [ ] Delete or rewrite parity tests that compare flag states
  - Keep tests validating core behavior (just remove flag toggling)
  - Delete tests purely for flag combination coverage
- [ ] Update `FeatureFlagScopeConcurrencyTests.cs` (delete or convert to other concurrency tests)
- [ ] Remove feature flag test infrastructure from `test/Veggerby.Boards.Tests/Infrastructure/`

### Phase 4: Documentation & Cleanup

- [ ] Delete `/docs/feature-flags.md` (or replace with "Graduated Features" retrospective)
- [ ] Update `CONTRIBUTING.md` to remove feature flag policy section
- [ ] Update `.github/copilot-instructions.md` to remove feature flag mentions
- [ ] Update all workstream docs to reflect graduated baseline
- [ ] Create CHANGELOG entry: "BREAKING: Removed feature flags; all graduated optimizations now unconditional"

### Phase 5: Code Quality Pass

- [ ] Search and remove all `if (FeatureFlags.Enable*)` checks
- [ ] Delete `src/Veggerby.Boards/Internal/FeatureFlags.cs` entirely
- [ ] Run full test suite to ensure no regressions
- [ ] Run benchmarks to validate performance parity (no degradation from flag removal)
- [ ] Ensure build succeeds with zero warnings

## Technical Approach

### Step-by-Step Execution Plan

#### Week 1: Inventory & Decision Matrix

1. Create decision matrix for all 18 flags (Graduate / Remove / Defer)
2. Review with stakeholders (document decisions)
3. Identify all code sites affected by each flag
4. Create migration plan for each graduated flag

#### Week 2: Graduate Core Flags (Phase 1)

1. **EnableCompiledPatterns**: Remove flag, update ~15 call sites, delete visitor parity tests
2. **EnableBitboards**: Remove flag, simplify service registration, update ~8 call sites
3. **EnableStateHashing**: Remove flag, update ~6 call sites, ensure hashing always on
4. **EnableSlidingFastPath**: Remove flag, update ~4 call sites
5. **EnableBitboardIncremental**: Remove flag, delete rebuild fallback, update ~2 call sites
6. **EnableTurnSequencing**: Remove flag, update ~5 call sites in turn mutators
7. Run full test suite after each flag removal (incremental validation)

#### Week 3: Process Experimental Flags (Phase 2)

1. **Graduate**: `EnableTraceCapture`, `EnableBoardShape`
2. **Remove**: `EnableCompiledPatternsAdjacencyCache`, `EnableSegmentedBitboards`
3. **Defer**: Decision plan flags, `EnableTimelineZipper`, per-piece masks, topology pruning, observer batching
4. Document deferred features in separate stories
5. Update simulation gating to use architectural checks (not flags)

#### Week 4: Cleanup & Validation (Phases 3-5)

1. Delete `FeatureFlags.cs` entirely
2. Remove `FeatureFlagScope` and related test infrastructure
3. Clean up or delete parity tests (keep behavior tests)
4. Update all documentation
5. Full CI run (all platforms, all test suites)
6. Benchmark suite validation (ensure no perf regressions)
7. Code review pass for residual flag references

### Migration Pattern Example

**Before (with flag):**

```csharp
if (Internal.FeatureFlags.EnableCompiledPatterns && TryGetCompiledResolver(game, out var services))
{
    return services.Resolver.Resolve(from, to, piece);
}
// fallback to visitor
return new ResolveTilePathPatternVisitor(game.Board, from, to).ResultPath;
```

**After (unconditional):**

```csharp
if (TryGetCompiledResolver(game, out var services))
{
    return services.Resolver.Resolve(from, to, piece);
}
// Edge case: compiled resolver unavailable (shouldn't happen in normal flow)
throw new InvalidOperationException("Compiled pattern resolver not available.");
```

Or if keeping visitor for troubleshooting:

```csharp
// Always prefer compiled resolver
if (TryGetCompiledResolver(game, out var services))
{
    return services.Resolver.Resolve(from, to, piece);
}
// Legacy fallback retained for troubleshooting only (should not occur in production)
Debug.Assert(false, "Compiled resolver missing - investigate service registration");
return new ResolveTilePathPatternVisitor(game.Board, from, to).ResultPath;
```

### Test Strategy

1. **Before any changes**: Capture baseline test results and benchmark numbers
2. **Per-flag removal**: Run affected tests immediately after each flag elimination
3. **Integration validation**: Full multi-module test suite after each week
4. **Cross-platform CI**: Validate on Linux, Windows, macOS (determinism preserved)
5. **Performance regression detection**: Compare benchmark suite before/after
6. **Final acceptance**: All tests pass, no perf degradation, zero feature flag references remain

## Dependencies

**Blocks:**

- None (can proceed immediately)

**Blocked By:**

- None (graduated flags already validated)

**Related Work:**

- Story #001: Bitboard graduation (already complete)
- Story #003: State hashing graduation (already complete)
- Story #005: Turn sequencing graduation (already complete)
- Story #004: Roslyn analyzer (will prevent future flag proliferation)

## Success Metrics

### Code Quality Metrics

- **Lines of code removed**: Target 500+ (flag checks, conditional paths, test scaffolding)
- **Cyclomatic complexity reduction**: Simplified evaluation paths in `GameProgress`, `GameBuilder`
- **Test count reduction**: Remove ~50+ flag-combination parity tests (keep behavior tests)

### Functionality Metrics

- **Zero regressions**: All existing tests pass (excluding deleted flag-toggle tests)
- **Performance parity**: Benchmarks within ±2% of baseline (no degradation from flag removal)
- **Cross-platform stability**: Hashing/determinism tests pass on all CI platforms

### Developer Experience Metrics

- **Clarity**: New contributors understand single code path (no "which flag state?" questions)
- **Parallel testing**: Tests can run concurrently without `FeatureFlagScope` serialization
- **Build time**: Potential improvement from fewer conditional compilations

### Documentation Metrics

- **Zero references**: No mentions of "feature flags" in code, tests, or docs (except retrospective)
- **Graduation transparency**: Clear CHANGELOG entry explaining what became baseline

## Risks & Mitigations

### Risk: Breaking Changes for External Consumers

**Impact**: Medium
**Probability**: Low (feature flags are internal)
**Mitigation**:

- Feature flags are in `Internal` namespace (not public API)
- Document breaking change in CHANGELOG
- Provide migration guide (mostly just "delete feature flag toggling code")

### Risk: Undiscovered Dependencies on Experimental Features

**Impact**: High (blocks release)
**Probability**: Low (experimental flags default OFF)
**Mitigation**:

- Thorough grep for all flag references before deleting
- Run full test suite with verbose output
- Check benchmark suite for unexpected behavior
- Keep git history for easy rollback if needed

### Risk: Performance Regression from Unconditional Features

**Impact**: Medium
**Probability**: Very Low (graduated flags already validated)
**Mitigation**:

- Run comprehensive benchmark suite before/after
- Monitor allocation profiles (no unexpected growth)
- Cross-platform CI validation (Linux, Windows, macOS)

### Risk: Test Fragility During Migration

**Impact**: Low
**Probability**: Medium
**Mitigation**:

- Incremental flag removal (one at a time, validate tests)
- Keep git commits small and atomic (easy rollback)
- Use feature branches for risky migrations

### Risk: Losing Troubleshooting Fallback Paths

**Impact**: Low
**Probability**: Low
**Mitigation**:

- For critical paths (compiled patterns), keep legacy visitor code commented with clear rationale
- Document fallback availability in code comments
- Retain ability to manually swap implementations if needed (via code edit, not flags)

## Effort Estimate

**Total: 3-4 weeks** (1 engineer, full-time focus)

- Week 1: Inventory, decision matrix, stakeholder review (5 days)
- Week 2: Graduate 6 core flags, incremental testing (5 days)
- Week 3: Process 12 experimental flags (graduate 2, remove 2, defer 8) (5 days)
- Week 4: Cleanup, validation, documentation, benchmarks (5 days)

**Parallelization potential**: Low (requires sequential flag removal to avoid test interference)

**Complexity**: Medium-High (touches many files, requires careful testing)

## Follow-Up Work

After this story is complete, create dedicated issues for deferred features:

1. **Decision Plan Optimization**: Dedicated story for grouping/filtering/masks with comprehensive benchmarks
2. **Observer Performance**: Batching optimization with overhead validation
3. **Bitboard Advanced Features**: Per-piece masks, topology pruning (if justified by benchmarks)
4. **Timeline Zipper**: Evaluate need vs external undo/redo; implement or delete

## Notes

- This is a **one-way door**: Once flags are removed, reintroducing them is high effort
- Prioritize **clarity over premature optimization**: If an experimental feature isn't clearly beneficial, remove it
- **Document decisions**: Each flag's fate (graduate/remove/defer) should be explicitly justified
- **Communicate breaking change**: Update CHANGELOG, README, migration guide for any external consumers

## References

- `/src/Veggerby.Boards/Internal/FeatureFlags.cs` (18 flags, 135 lines)
- `/docs/feature-flags.md` (current inventory)
- `/test/Veggerby.Boards.Tests/Infrastructure/FeatureFlagScope.cs` (global state helper)
- `/docs/plans/current-priorities.md` (graduation priorities)
- Workstream 4: Performance Data Layout & Hot Paths
- Workstream 7: Developer Experience & Quality Gates
