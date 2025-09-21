using System;

using Veggerby.Boards.Internal;

namespace Veggerby.Boards.Tests.Infrastructure;

/// <summary>
/// Disposable helper for temporarily toggling internal feature flags inside tests.
/// Restores previous values on dispose to ensure isolation between tests.
/// </summary>
public sealed class FeatureFlagScope : IDisposable
{
    private readonly (bool decisionPlan, bool grouping, bool filtering, bool debugParity, bool compiledPatterns, bool adjacencyCache, bool hashing, bool trace, bool timeline) _prior;

    public FeatureFlagScope(
        bool? decisionPlan = null,
        bool? grouping = null,
        bool? filtering = null,
        bool? debugParity = null,
        bool? compiledPatterns = null,
        bool? adjacencyCache = null,
        bool? hashing = null,
        bool? trace = null,
        bool? timeline = null)
    {
        _prior = (
            FeatureFlags.EnableDecisionPlan,
            FeatureFlags.EnableDecisionPlanGrouping,
            FeatureFlags.EnableDecisionPlanEventFiltering,
            FeatureFlags.EnableDecisionPlanDebugParity,
            FeatureFlags.EnableCompiledPatterns,
            FeatureFlags.EnableCompiledPatternsAdjacencyCache,
            FeatureFlags.EnableStateHashing,
            FeatureFlags.EnableTraceCapture,
            FeatureFlags.EnableTimelineZipper);

        if (decisionPlan.HasValue) { FeatureFlags.EnableDecisionPlan = decisionPlan.Value; }
        if (grouping.HasValue) { FeatureFlags.EnableDecisionPlanGrouping = grouping.Value; }
        if (filtering.HasValue) { FeatureFlags.EnableDecisionPlanEventFiltering = filtering.Value; }
        if (debugParity.HasValue) { FeatureFlags.EnableDecisionPlanDebugParity = debugParity.Value; }
        if (compiledPatterns.HasValue) { FeatureFlags.EnableCompiledPatterns = compiledPatterns.Value; }
        if (adjacencyCache.HasValue) { FeatureFlags.EnableCompiledPatternsAdjacencyCache = adjacencyCache.Value; }
        if (hashing.HasValue) { FeatureFlags.EnableStateHashing = hashing.Value; }
        if (trace.HasValue) { FeatureFlags.EnableTraceCapture = trace.Value; }
        if (timeline.HasValue) { FeatureFlags.EnableTimelineZipper = timeline.Value; }
    }

    public void Dispose()
    {
        FeatureFlags.EnableDecisionPlan = _prior.decisionPlan;
        FeatureFlags.EnableDecisionPlanGrouping = _prior.grouping;
        FeatureFlags.EnableDecisionPlanEventFiltering = _prior.filtering;
        FeatureFlags.EnableDecisionPlanDebugParity = _prior.debugParity;
        FeatureFlags.EnableCompiledPatterns = _prior.compiledPatterns;
        FeatureFlags.EnableCompiledPatternsAdjacencyCache = _prior.adjacencyCache;
        FeatureFlags.EnableStateHashing = _prior.hashing;
        FeatureFlags.EnableTraceCapture = _prior.trace;
        FeatureFlags.EnableTimelineZipper = _prior.timeline;
    }
}