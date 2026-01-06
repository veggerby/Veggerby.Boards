namespace Veggerby.Boards.States;

/// <summary>
/// Defines visibility constraints for artifact states in player-projected views.
/// </summary>
/// <remarks>
/// Visibility controls which players or observers can see a given artifact state. This enables
/// imperfect-information games (card games, fog of war) by providing first-class redaction of
/// hidden state when projecting <see cref="GameState"/> to a player-specific view.
/// </remarks>
public enum Visibility
{
    /// <summary>
    /// Visible to all players and observers (e.g., board position, public scores).
    /// </summary>
    Public,

    /// <summary>
    /// Visible only to the owning player (e.g., hand cards, hidden units).
    /// </summary>
    /// <remarks>
    /// Ownership is determined by the <see cref="Artifacts.Artifact"/> associated with the state.
    /// When projected, non-owners see a redacted placeholder.
    /// </remarks>
    Private,

    /// <summary>
    /// Visible to no players until explicitly revealed (e.g., face-down cards, deck contents).
    /// </summary>
    /// <remarks>
    /// Even the owner cannot see <c>Hidden</c> state. Used for information that becomes
    /// available only after a reveal action or rule transition.
    /// </remarks>
    Hidden
}
