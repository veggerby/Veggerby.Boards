using System.Collections.Generic;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States;

/// <summary>
/// Represents the final outcome of a terminated game.
/// </summary>
/// <remarks>
/// Game modules implement this interface to provide game-specific outcome details.
/// The interface standardizes how outcomes are represented across different game types
/// while allowing flexibility for module-specific metrics and data.
/// </remarks>
public interface IGameOutcome
{
    /// <summary>
    /// Gets the terminal condition that ended the game (e.g., "Checkmate", "Stalemate", "Scoring", "TerritoryScoring").
    /// </summary>
    string TerminalCondition { get; }

    /// <summary>
    /// Gets the ordered player results from winner to loser (or tied).
    /// Players with the same rank are considered tied.
    /// </summary>
    IReadOnlyList<PlayerResult> PlayerResults { get; }
}

/// <summary>
/// Represents a single player's outcome in a terminated game.
/// </summary>
public sealed record PlayerResult
{
    /// <summary>
    /// Gets the player this result applies to.
    /// </summary>
    public required Player Player { get; init; }

    /// <summary>
    /// Gets the outcome type for this player.
    /// </summary>
    public required OutcomeType Outcome { get; init; }

    /// <summary>
    /// Gets the final rank for this player (1 = winner, 2 = second place, etc.).
    /// Players with the same rank are considered tied.
    /// </summary>
    public required int Rank { get; init; }

    /// <summary>
    /// Gets optional game-specific metrics (e.g., VictoryPoints, Territory, Armies).
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metrics { get; init; }
}

/// <summary>
/// Represents the type of outcome for a player.
/// </summary>
public enum OutcomeType
{
    /// <summary>
    /// The player won the game.
    /// </summary>
    Win,

    /// <summary>
    /// The player lost the game.
    /// </summary>
    Loss,

    /// <summary>
    /// The game ended in a draw.
    /// </summary>
    Draw,

    /// <summary>
    /// The player was eliminated during play.
    /// </summary>
    Eliminated
}
