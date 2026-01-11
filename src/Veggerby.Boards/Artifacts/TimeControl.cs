using System;

namespace Veggerby.Boards.Artifacts;

/// <summary>
/// Defines time control rules for a game clock.
/// </summary>
/// <remarks>
/// Time controls define how time is allocated and managed during gameplay.
/// Common patterns include:
/// - Fischer time: Initial time + increment per move
/// - Bronstein delay: Grace period before clock starts
/// - Classical: Initial time + bonus after N moves
/// All time spans must be non-negative. Null values indicate the feature is disabled.
/// </remarks>
public sealed record TimeControl
{
    /// <summary>
    /// Gets the initial time allocated to each player.
    /// </summary>
    /// <remarks>
    /// This is the starting time bank for each player (e.g., 5 minutes, 1 hour).
    /// Must be positive.
    /// </remarks>
    public required TimeSpan InitialTime
    {
        get; init;
    }

    /// <summary>
    /// Gets the time increment added after each move (Fischer time control).
    /// </summary>
    /// <remarks>
    /// After completing a move, this amount is added to the player's remaining time.
    /// Example: 2 seconds per move in "5+2" Fischer time.
    /// Null indicates no increment.
    /// </remarks>
    public TimeSpan? Increment
    {
        get; init;
    }

    /// <summary>
    /// Gets the delay before the clock starts counting down (Bronstein delay).
    /// </summary>
    /// <remarks>
    /// The clock waits this duration before starting to count down.
    /// Time used within the delay period is not deducted from remaining time.
    /// Null indicates no delay.
    /// </remarks>
    public TimeSpan? Delay
    {
        get; init;
    }

    /// <summary>
    /// Gets the number of moves required before bonus time is added.
    /// </summary>
    /// <remarks>
    /// Used in classical time controls (e.g., "40 moves in 2 hours, then 30 minutes").
    /// Must be paired with <see cref="BonusTime"/>.
    /// Null indicates no move-based bonus.
    /// </remarks>
    public int? MovesPerTimeControl
    {
        get; init;
    }

    /// <summary>
    /// Gets the bonus time added after reaching the move count threshold.
    /// </summary>
    /// <remarks>
    /// Added to remaining time after completing <see cref="MovesPerTimeControl"/> moves.
    /// Must be paired with <see cref="MovesPerTimeControl"/>.
    /// Null indicates no move-based bonus.
    /// </remarks>
    public TimeSpan? BonusTime
    {
        get; init;
    }
}
