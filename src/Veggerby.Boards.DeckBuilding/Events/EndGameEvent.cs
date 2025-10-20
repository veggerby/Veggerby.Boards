using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Intentional event signaling the game should terminate (triggered after scoring in cleanup when max turns reached).
/// </summary>
public sealed class EndGameEvent : IGameEvent
{
}