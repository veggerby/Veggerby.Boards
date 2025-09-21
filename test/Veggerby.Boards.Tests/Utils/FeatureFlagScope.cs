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

    private FeatureFlagScope(bool originalDecisionPlan, bool originalStateHashing)
    {
        _originalDecisionPlan = originalDecisionPlan;
        _originalStateHashing = originalStateHashing;
    }

    /// <summary>
    /// Enables or disables the DecisionPlan flag for the scope lifetime.
    /// </summary>
    /// <param name="enableDecisionPlan">New value.</param>
    /// <returns>Disposable scope.</returns>
    public static FeatureFlagScope DecisionPlan(bool enableDecisionPlan)
    {
        var scope = new FeatureFlagScope(FeatureFlags.EnableDecisionPlan, FeatureFlags.EnableStateHashing);
        FeatureFlags.EnableDecisionPlan = enableDecisionPlan;
        return scope;
    }

    /// <summary>
    /// Enables or disables the StateHashing flag for the scope lifetime.
    /// </summary>
    public static FeatureFlagScope StateHashing(bool enable)
    {
        var scope = new FeatureFlagScope(FeatureFlags.EnableDecisionPlan, FeatureFlags.EnableStateHashing);
        FeatureFlags.EnableStateHashing = enable;
        return scope;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        FeatureFlags.EnableDecisionPlan = _originalDecisionPlan;
        FeatureFlags.EnableStateHashing = _originalStateHashing;
    }
}