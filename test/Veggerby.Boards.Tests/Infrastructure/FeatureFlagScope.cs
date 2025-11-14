using System;

namespace Veggerby.Boards.Tests.Infrastructure;

/// <summary>
/// NO-OP: Feature flags have been completely removed. This class remains for test compatibility only.
/// All parameters are ignored. Tests should be updated to remove FeatureFlagScope usage.
/// </summary>
public sealed class FeatureFlagScope : IDisposable
{
    // Complete no-op - all parameters ignored, all features are unconditionally enabled in production
    public FeatureFlagScope(
        bool? grouping = null,
        bool? filtering = null,
        bool? compiledPatterns = null,
        bool? adjacencyCache = null,
        bool? hashing = null,
        bool? trace = null,
        bool? timeline = null,
        bool? decisionPlanMasks = null,
        bool? bitboards = null,
        bool? boardShape = null,
        bool? slidingFastPath = null)
    {
        // No-op: All features are now unconditionally enabled in production code
    }

    public void Dispose()
    {
        // No-op: Nothing to restore
    }
}