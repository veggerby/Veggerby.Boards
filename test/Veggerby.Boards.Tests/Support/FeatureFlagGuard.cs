using System;

namespace Veggerby.Boards.Tests.Support;

/// <summary>
/// Disposable helper that forces a feature flag value for the duration of a test and restores the prior value on dispose.
/// </summary>
public sealed class FeatureFlagGuard : IDisposable
{
    private readonly Action _restore;

    private FeatureFlagGuard(Action restore)
    {
        _restore = restore;
    }

    public static FeatureFlagGuard ForceTurnSequencing(bool enable = true)
    {
        var original = Boards.Internal.FeatureFlags.EnableTurnSequencing;
        Boards.Internal.FeatureFlags.EnableTurnSequencing = enable;
        return new FeatureFlagGuard(() => Boards.Internal.FeatureFlags.EnableTurnSequencing = original);
    }

    public void Dispose()
    {
        _restore?.Invoke();
    }
}