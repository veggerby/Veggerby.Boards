using System;

namespace Veggerby.Boards.States;

/// <summary>
/// Represents the coarse-grained segment of a logical turn. Segments allow future rules to
/// gate events (e.g. upkeep, main actions, cleanup) without coupling to player activity directly.
/// </summary>
/// <remarks>
/// This enum is introduced in shadow mode; no existing rule references it yet. Future work will
/// advance the segment alongside <see cref="TurnState"/> transitions when turn advancement logic
/// is implemented. Ordering is fixed and stable for potential comparison operations.
/// </remarks>
public enum TurnSegment
{
    /// <summary>
    /// Initial portion of a turn (e.g. upkeep / preparation).
    /// </summary>
    Start = 0,

    /// <summary>
    /// Main action portion where the majority of player-driven events occur.
    /// </summary>
    Main = 1,

    /// <summary>
    /// Final portion used for cleanup / end-of-turn triggers.
    /// </summary>
    End = 2
}