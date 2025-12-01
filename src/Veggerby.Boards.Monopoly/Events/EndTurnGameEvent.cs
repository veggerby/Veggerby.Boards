using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Monopoly.Events;

/// <summary>
/// Event representing the end of a player's turn in Monopoly.
/// </summary>
/// <remarks>
/// This event triggers switching to the next player. It should be
/// emitted when a player's turn is complete (either after moving
/// without rolling doubles, or after going to jail).
/// </remarks>
public class EndTurnGameEvent : IGameEvent
{
}
