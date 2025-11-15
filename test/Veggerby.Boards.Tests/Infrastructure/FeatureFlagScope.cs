using System;

namespace Veggerby.Boards.Tests.Infrastructure;

/// <summary>
/// No-op disposable scope helper retained for test compatibility after feature flag elimination.
/// All features are now always enabled, so this class does nothing.
/// </summary>
public sealed class FeatureFlagScope : IDisposable
{
    public FeatureFlagScope(
        bool? bitboards = null,
        bool? compiledPatterns = null,
        bool? slidingFastPath = null,
        bool? hashing = null,
        bool? incrementalBitboards = null,
        bool? turnSequencing = null,
        bool? trace = null,
        bool? boardShape = null,
        bool? adjacencyCache = null,
        bool? segmentedBitboards = null,
        bool? decisionPlanGrouping = null,
        bool? decisionPlanEventFiltering = null,
        bool? decisionPlanMasks = null,
        bool? timelineZipper = null,
        bool? perPieceMasks = null,
        bool? topologyPruning = null,
        bool? observerBatching = null,
        bool? simulation = null,
        bool? grouping = null)
    {
        // No-op: all features are now always enabled
    }

    public void Dispose()
    {
        // No-op
    }
}
