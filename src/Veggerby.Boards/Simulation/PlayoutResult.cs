using System.Collections.Generic;

using Veggerby.Boards.States;

namespace Veggerby.Boards.Simulation;

/// <summary>
/// Result of executing a single playout sequence.
/// </summary>
/// <param name="Initial">Initial progress snapshot.</param>
/// <param name="Final">Final progress after playout termination (may equal initial).</param>
/// <param name="AppliedEvents">Number of successfully applied events.</param>
/// <param name="TerminalReason">Reason playout terminated.</param>
/// <param name="Trace">Optional captured intermediate states (includes initial + final when enabled).</param>
public sealed record PlayoutResult(GameProgress Initial, GameProgress Final, int AppliedEvents, PlayoutTerminalReason TerminalReason, IReadOnlyList<GameState>? Trace)
{
    /// <summary>True if at least one event applied.</summary>
    public bool Progressed => AppliedEvents > 0;
}