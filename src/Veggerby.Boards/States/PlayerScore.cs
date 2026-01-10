using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Represents a player's score with ranking information.
/// </summary>
/// <remarks>
/// Used by scoring helpers to represent ranked score results.
/// Players with the same rank are considered tied.
/// </remarks>
public sealed record PlayerScore
{
    /// <summary>
    /// Gets the player this score applies to.
    /// </summary>
    public required Player Player
    {
        get; init;
    }

    /// <summary>
    /// Gets the numeric score value.
    /// </summary>
    public required int Score
    {
        get; init;
    }

    /// <summary>
    /// Gets the rank position (1 = highest, 2 = second, etc.).
    /// Players with the same score have the same rank.
    /// </summary>
    public required int Rank
    {
        get; init;
    }
}
