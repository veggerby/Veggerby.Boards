using System;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Visibility policy for observers (spectators, arbiters, analysts) with role-based access control.
/// </summary>
/// <remarks>
/// This policy supports different observer roles:
/// <list type="bullet">
/// <item><description><see cref="ObserverRole.Full"/>: See all state (admin, arbiter, post-game analysis)</description></item>
/// <item><description><see cref="ObserverRole.Limited"/>: See only public state (live tournament spectator)</description></item>
/// <item><description><see cref="ObserverRole.PlayerPerspective"/>: See what a specific player sees (coaching, training)</description></item>
/// </list>
/// </remarks>
public sealed class ObserverVisibilityPolicy : IVisibilityPolicy
{
    private readonly ObserverRole _role;
    private readonly IVisibilityPolicy? _playerPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObserverVisibilityPolicy"/> class.
    /// </summary>
    /// <param name="role">The observer role determining visibility permissions.</param>
    /// <param name="playerPolicy">
    /// Optional player-specific policy for <see cref="ObserverRole.PlayerPerspective"/> role.
    /// Defaults to <see cref="PlayerOwnedVisibilityPolicy"/> if not provided.
    /// </param>
    public ObserverVisibilityPolicy(ObserverRole role, IVisibilityPolicy? playerPolicy = null)
    {
        _role = role;
        _playerPolicy = playerPolicy ?? PlayerOwnedVisibilityPolicy.Instance;
    }

    /// <inheritdoc />
    public bool CanSee(Player? viewer, IArtifactState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return _role switch
        {
            ObserverRole.Full => true, // Full visibility - see everything

            ObserverRole.Limited => state.Visibility == Visibility.Public, // Public only - no private or hidden

            ObserverRole.PlayerPerspective => _playerPolicy!.CanSee(viewer, state), // Delegate to player policy

            _ => throw new ArgumentOutOfRangeException(nameof(_role), _role, "Unknown observer role")
        };
    }

    /// <inheritdoc />
    public IArtifactState? Redact(Player? viewer, IArtifactState state)
    {
        if (CanSee(viewer, state))
        {
            return state;
        }

        // For Limited observers, completely omit non-public state
        if (_role == ObserverRole.Limited)
        {
            return null;
        }

        // For PlayerPerspective, delegate redaction to the player policy
        if (_role == ObserverRole.PlayerPerspective)
        {
            return _playerPolicy!.Redact(viewer, state);
        }

        // Full observers see everything, so this shouldn't be reached
        return state;
    }
}
