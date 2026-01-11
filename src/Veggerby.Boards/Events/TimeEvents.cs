using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Events;

/// <summary>
/// Event to start the clock for a player's turn.
/// </summary>
/// <param name="Clock">The game clock to start.</param>
/// <param name="Player">The player whose clock is starting.</param>
/// <param name="Timestamp">Explicit timestamp when the clock starts (for deterministic replay).</param>
/// <remarks>
/// This event signals the beginning of a timed turn. The timestamp parameter is critical
/// for deterministic replay - it must be provided explicitly rather than using wall-clock time.
/// </remarks>
public sealed record StartClockEvent(GameClock Clock, Player Player, DateTime Timestamp) : IGameEvent;

/// <summary>
/// Event to stop the clock after a move.
/// </summary>
/// <param name="Clock">The game clock to stop.</param>
/// <param name="Timestamp">Explicit timestamp when the clock stops (for deterministic replay).</param>
/// <remarks>
/// This event signals the end of a timed turn. Elapsed time is deducted and any increment
/// or bonus is applied based on the clock's time control settings. The timestamp parameter
/// ensures deterministic state transitions during replay.
/// </remarks>
public sealed record StopClockEvent(GameClock Clock, DateTime Timestamp) : IGameEvent;

/// <summary>
/// Event triggered when a player runs out of time.
/// </summary>
/// <param name="Player">The player who ran out of time.</param>
/// <remarks>
/// This event signals that a player's time has expired. It typically triggers game termination
/// with the opponent winning by time forfeit. Applications must explicitly send this event
/// when detecting time expiration (no automatic timeout enforcement).
/// </remarks>
public sealed record TimeFlagEvent(Player Player) : IGameEvent;
