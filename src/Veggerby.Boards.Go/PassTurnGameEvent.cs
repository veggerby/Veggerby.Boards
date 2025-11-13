using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Go;

/// <summary>
/// Represents a pass (no stone placed) advancing to the next player and incrementing pass count.
/// </summary>
public sealed class PassTurnGameEvent : IGameEvent
{
}