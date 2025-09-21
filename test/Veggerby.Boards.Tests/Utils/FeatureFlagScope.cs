using System;

using Veggerby.Boards.Internal;

namespace Veggerby.Boards.Tests.Utils;

/// <summary>
/// Disposable helper to temporarily set feature flags inside a test, restoring previous values on dispose.
/// </summary>
public sealed class FeatureFlagScope : IDisposable
{
    private readonly bool _originalDecisionPlan;

    private FeatureFlagScope(bool originalDecisionPlan)
    {
        _originalDecisionPlan = originalDecisionPlan;
    }

    /// <summary>
    /// Enables or disables the DecisionPlan flag for the scope lifetime.
    /// </summary>
    /// <param name="enableDecisionPlan">New value.</param>
    /// <returns>Disposable scope.</returns>
    public static FeatureFlagScope DecisionPlan(bool enableDecisionPlan)
    {
        var scope = new FeatureFlagScope(FeatureFlags.EnableDecisionPlan);
        FeatureFlags.EnableDecisionPlan = enableDecisionPlan;
        return scope;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        FeatureFlags.EnableDecisionPlan = _originalDecisionPlan;
    }
}