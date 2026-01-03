using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Othello;

/// <summary>
/// Represents a pass (no disc placed) when the active player has no valid moves.
/// Advances to the next player.
/// </summary>
public sealed class PassTurnGameEvent : IGameEvent
{
}
