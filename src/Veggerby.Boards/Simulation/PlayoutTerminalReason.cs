namespace Veggerby.Boards.Simulation;

/// <summary>
/// Terminal classification for a playout.
/// </summary>
public enum PlayoutTerminalReason
{
    /// <summary>No terminal classification explicitly set (internal use).</summary>
    None = 0,
    /// <summary>No candidate events available (policy produced none).</summary>
    NoMoves,
    /// <summary>Policy returned null after at least one successful event (normal termination).</summary>
    PolicyReturnedNull,
    /// <summary>Stop predicate indicated early termination.</summary>
    StopPredicate,
    /// <summary>Reached configured maximum depth / event limit.</summary>
    MaxDepth,
    /// <summary>Time limit reached (legacy simulator only).</summary>
    TimeLimit,
    /// <summary>Cancellation requested (parallel / async orchestration).</summary>
    CancellationRequested,
}