namespace Veggerby.Boards.Simulation;

/// <summary>
/// Terminal classification for a playout.
/// </summary>
public enum PlayoutTerminalReason
{
    /// <summary>No candidate events available (policy produced none or all rejected).</summary>
    NoMoves = 0,
    /// <summary>Reached configured maximum event limit.</summary>
    MaxEvents = 1,
    /// <summary>Time limit reached.</summary>
    TimeLimit = 2,
}