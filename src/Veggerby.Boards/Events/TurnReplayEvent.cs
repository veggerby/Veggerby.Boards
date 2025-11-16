using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Events;

/// <summary>
/// Event signaling an earned replay (extra turn) without rotating the active player (e.g., Ludo six roll, Kalaha extra turn).
/// Always honored (turn sequencing is a graduated feature).
/// </summary>
public sealed class TurnReplayEvent : IPhaseControlGameEvent
{
}