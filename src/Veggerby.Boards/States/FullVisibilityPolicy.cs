using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Visibility policy that grants full visibility to all states (perfect information games).
/// </summary>
/// <remarks>
/// This is the default policy for backward compatibility with existing games (Chess, Go, Checkers).
/// All artifact states are visible to all players and observers without redaction.
/// </remarks>
public sealed class FullVisibilityPolicy : IVisibilityPolicy
{
    /// <summary>
    /// Gets the singleton instance of the full visibility policy.
    /// </summary>
    public static FullVisibilityPolicy Instance { get; } = new FullVisibilityPolicy();

    private FullVisibilityPolicy()
    {
    }

    /// <inheritdoc />
    public bool CanSee(Player? viewer, IArtifactState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return true;
    }

    /// <inheritdoc />
    public IArtifactState? Redact(Player? viewer, IArtifactState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return state;
    }
}
