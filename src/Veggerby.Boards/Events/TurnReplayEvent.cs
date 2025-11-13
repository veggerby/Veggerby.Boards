using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Events;

/// <summary>
/// Event signaling an earned replay (extra turn) without rotating the active player (e.g., Ludo six roll, Kalaha extra turn).
/// Only honored when <c>FeatureFlags.EnableTurnSequencing</c> is true.
/// </summary>
public sealed class TurnReplayEvent : IPhaseControlGameEvent
{
}