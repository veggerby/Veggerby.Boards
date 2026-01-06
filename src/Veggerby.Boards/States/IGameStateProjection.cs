using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Provides projection capabilities to create player-specific views of game state.
/// </summary>
/// <remarks>
/// Projections filter <see cref="GameState"/> based on visibility policies, enabling
/// imperfect-information games by masking hidden state from non-authorized viewers.
/// </remarks>
public interface IGameStateProjection
{
    /// <summary>
    /// Creates a view of the game state scoped to a specific player.
    /// </summary>
    /// <param name="player">The player whose perspective to project.</param>
    /// <returns>A filtered view showing only states visible to the specified player.</returns>
    /// <remarks>
    /// The returned view applies the configured <see cref="IVisibilityPolicy"/> to determine
    /// which artifact states are visible, redacted, or omitted. The projection is deterministic:
    /// same state + same player always produces the same view.
    /// </remarks>
    GameStateView ProjectFor(Player player);

    /// <summary>
    /// Creates a view of the game state for an observer role.
    /// </summary>
    /// <param name="role">The observer role determining visibility permissions.</param>
    /// <returns>A filtered view based on the observer's access level.</returns>
    /// <remarks>
    /// Observer views support spectator modes, tournament displays, and post-game analysis.
    /// The <see cref="ObserverRole"/> controls which states are visible (e.g., public only
    /// for live spectators, full visibility for admin/arbiter).
    /// </remarks>
    GameStateView ProjectForObserver(ObserverRole role);
}
