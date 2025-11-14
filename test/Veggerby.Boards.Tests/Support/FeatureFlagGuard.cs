using System;

namespace Veggerby.Boards.Tests.Support;

/// <summary>
/// NO-OP: Feature flags have been removed. This class remains for test compatibility only.
/// Turn sequencing is always enabled (graduated feature).
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
        // No-op: Turn sequencing always enabled (graduated feature)
        return new FeatureFlagGuard(() => { });
    }

    public void Dispose()
    {
        _restore?.Invoke();
    }
}