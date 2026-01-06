using System;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Default implementation of <see cref="IGameStateProjection"/> using a configurable visibility policy.
/// </summary>
/// <remarks>
/// This projection applies a single <see cref="IVisibilityPolicy"/> for all player and observer views.
/// For more complex scenarios (e.g., dynamic visibility based on unit positions), extend this class
/// or provide a custom <see cref="IVisibilityPolicy"/> implementation.
/// </remarks>
public sealed class DefaultGameStateProjection : IGameStateProjection
{
    private readonly GameState _state;
    private readonly IVisibilityPolicy _policy;

    /// <summary>
    /// Initializes a new <see cref="DefaultGameStateProjection"/> with the specified state and policy.
    /// </summary>
    /// <param name="state">The game state to project.</param>
    /// <param name="policy">The visibility policy to apply (defaults to <see cref="FullVisibilityPolicy"/> if null).</param>
    public DefaultGameStateProjection(GameState state, IVisibilityPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(state);

        _state = state;
        _policy = policy ?? FullVisibilityPolicy.Instance;
    }

    /// <inheritdoc />
    public GameStateView ProjectFor(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        return new GameStateView(_state, _policy, player);
    }

    /// <inheritdoc />
    public GameStateView ProjectForObserver(ObserverRole role)
    {
        // For default implementation, Full observer role uses full visibility,
        // Limited uses public-only visibility
        var effectivePolicy = role switch
        {
            ObserverRole.Full => FullVisibilityPolicy.Instance,
            ObserverRole.Limited => new PublicOnlyVisibilityPolicy(),
            ObserverRole.PlayerPerspective => _policy, // Use configured policy without specific player
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown observer role")
        };

        return new GameStateView(_state, effectivePolicy, viewer: null);
    }

    /// <summary>
    /// Simple visibility policy that shows only public states (for limited observers).
    /// </summary>
    private sealed class PublicOnlyVisibilityPolicy : IVisibilityPolicy
    {
        public bool CanSee(Player? viewer, IArtifactState state)
        {
            return state.Visibility == Visibility.Public;
        }

        public IArtifactState? Redact(Player? viewer, IArtifactState state)
        {
            // For limited observers, completely omit non-public state
            return null;
        }
    }
}
