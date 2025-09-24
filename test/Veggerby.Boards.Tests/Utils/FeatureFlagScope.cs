using System;

using Veggerby.Boards.Internal;

namespace Veggerby.Boards.Tests.Utils;

/// <summary>
/// Disposable helper to temporarily set feature flags inside a test, restoring previous values on dispose.
/// </summary>
public sealed class FeatureFlagScope : IDisposable
{
    private readonly bool _originalDecisionPlan;
    private readonly bool _originalStateHashing;
    private readonly bool _originalCompiledPatterns;
    private readonly bool _originalAdjacencyCache;
    private readonly bool _originalBitboards;
    private readonly bool _originalDebugParity;
    private readonly bool _originalMasks;

    private FeatureFlagScope(bool originalDecisionPlan, bool originalStateHashing, bool originalCompiledPatterns, bool originalAdjacencyCache, bool originalBitboards, bool originalDebugParity, bool originalMasks)
    {
        _originalDecisionPlan = originalDecisionPlan;
        _originalStateHashing = originalStateHashing;
        _originalCompiledPatterns = originalCompiledPatterns;
        _originalAdjacencyCache = originalAdjacencyCache;
        _originalBitboards = originalBitboards;
        _originalDebugParity = originalDebugParity;
        _originalMasks = originalMasks;
    }

    /// <summary>
    /// Enables or disables the DecisionPlan flag for the scope lifetime.
    /// </summary>
    /// <param name="enableDecisionPlan">New value.</param>
    /// <returns>Disposable scope.</returns>
    public static FeatureFlagScope DecisionPlan(bool enableDecisionPlan)
    {
        var scope = new FeatureFlagScope(FeatureFlags.EnableDecisionPlan, FeatureFlags.EnableStateHashing, FeatureFlags.EnableCompiledPatterns, FeatureFlags.EnableCompiledPatternsAdjacencyCache, FeatureFlags.EnableBitboards, FeatureFlags.EnableDecisionPlanDebugParity, FeatureFlags.EnableDecisionPlanMasks);
        FeatureFlags.EnableDecisionPlan = enableDecisionPlan;
        return scope;
    }

    /// <summary>
    /// Enables or disables the StateHashing flag for the scope lifetime.
    /// </summary>
    public static FeatureFlagScope StateHashing(bool enable)
    {
        var scope = new FeatureFlagScope(FeatureFlags.EnableDecisionPlan, FeatureFlags.EnableStateHashing, FeatureFlags.EnableCompiledPatterns, FeatureFlags.EnableCompiledPatternsAdjacencyCache, FeatureFlags.EnableBitboards, FeatureFlags.EnableDecisionPlanDebugParity, FeatureFlags.EnableDecisionPlanMasks);
        FeatureFlags.EnableStateHashing = enable;
        return scope;
    }

    /// <summary>
    /// Composite constructor allowing simultaneous toggling of multiple feature flags (used in integration style tests).
    /// </summary>
    public FeatureFlagScope(bool? decisionPlan = null, bool? stateHashing = null, bool? compiledPatterns = null, bool? adjacencyCache = null, bool? bitboards = null, bool? debugParity = null, bool? decisionPlanMasks = null)
        : this(FeatureFlags.EnableDecisionPlan, FeatureFlags.EnableStateHashing, FeatureFlags.EnableCompiledPatterns, FeatureFlags.EnableCompiledPatternsAdjacencyCache, FeatureFlags.EnableBitboards, FeatureFlags.EnableDecisionPlanDebugParity, FeatureFlags.EnableDecisionPlanMasks)
    {
        if (decisionPlan.HasValue) FeatureFlags.EnableDecisionPlan = decisionPlan.Value;
        if (stateHashing.HasValue) FeatureFlags.EnableStateHashing = stateHashing.Value;
        if (compiledPatterns.HasValue) FeatureFlags.EnableCompiledPatterns = compiledPatterns.Value;
        if (adjacencyCache.HasValue) FeatureFlags.EnableCompiledPatternsAdjacencyCache = adjacencyCache.Value;
        if (bitboards.HasValue) FeatureFlags.EnableBitboards = bitboards.Value;
        if (debugParity.HasValue) FeatureFlags.EnableDecisionPlanDebugParity = debugParity.Value;
        if (decisionPlanMasks.HasValue) FeatureFlags.EnableDecisionPlanMasks = decisionPlanMasks.Value;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        FeatureFlags.EnableDecisionPlan = _originalDecisionPlan;
        FeatureFlags.EnableStateHashing = _originalStateHashing;
        FeatureFlags.EnableCompiledPatterns = _originalCompiledPatterns;
        FeatureFlags.EnableCompiledPatternsAdjacencyCache = _originalAdjacencyCache;
        FeatureFlags.EnableBitboards = _originalBitboards;
        FeatureFlags.EnableDecisionPlanDebugParity = _originalDebugParity;
        FeatureFlags.EnableDecisionPlanMasks = _originalMasks;
    }
}