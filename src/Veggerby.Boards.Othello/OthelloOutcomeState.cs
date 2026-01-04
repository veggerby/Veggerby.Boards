using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Othello;

/// <summary>
/// Outcome state for Othello games implementing <see cref="IGameOutcome"/> for unified termination tracking.
/// </summary>
public sealed class OthelloOutcomeState : IArtifactState, IGameOutcome
{
    private static readonly OthelloOutcomeMarker Marker = new();

    /// <summary>
    /// Gets the artifact marker.
    /// </summary>
    public Artifact Artifact => Marker;

    /// <summary>
    /// Gets the winner of the game (null if draw).
    /// </summary>
    public Player? Winner
    {
        get;
    }

    /// <summary>
    /// Gets the black player's final disc count.
    /// </summary>
    public int BlackScore
    {
        get;
    }

    /// <summary>
    /// Gets the white player's final disc count.
    /// </summary>
    public int WhiteScore
    {
        get;
    }

    /// <summary>
    /// Gets the terminal condition description.
    /// </summary>
    public string TerminalCondition => Winner != null
        ? $"Victory (Black: {BlackScore}, White: {WhiteScore})"
        : $"Draw (Black: {BlackScore}, White: {WhiteScore})";

    /// <summary>
    /// Gets the ordered player results.
    /// </summary>
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OthelloOutcomeState"/> class.
    /// </summary>
    /// <param name="winner">The winning player (null for draw).</param>
    /// <param name="blackScore">Black player's final disc count.</param>
    /// <param name="whiteScore">White player's final disc count.</param>
    /// <param name="players">All players in the game.</param>
    public OthelloOutcomeState(Player? winner, int blackScore, int whiteScore, IEnumerable<Player> players)
    {
        Winner = winner;
        BlackScore = blackScore;
        WhiteScore = whiteScore;

        var playerList = players.ToList();

        if (winner == null)
        {
            // Draw - both players tied
            PlayerResults = playerList.Select(p => new PlayerResult
            {
                Player = p,
                Outcome = OutcomeType.Draw,
                Rank = 1
            }).ToList();
        }
        else
        {
            // Victory - winner rank 1, loser rank 2
            var loser = playerList.First(p => p != winner);

            PlayerResults = new[]
            {
                new PlayerResult { Player = winner, Outcome = OutcomeType.Win, Rank = 1 },
                new PlayerResult { Player = loser, Outcome = OutcomeType.Loss, Rank = 2 }
            };
        }
    }

    /// <summary>
    /// Checks equality with another artifact state.
    /// </summary>
    /// <param name="other">Other state to compare.</param>
    /// <returns>True if states are equal.</returns>
    public bool Equals(IArtifactState? other)
    {
        return other is OthelloOutcomeState otherOutcome
            && Winner == otherOutcome.Winner
            && BlackScore == otherOutcome.BlackScore
            && WhiteScore == otherOutcome.WhiteScore;
    }

    private sealed class OthelloOutcomeMarker : Artifact
    {
        public OthelloOutcomeMarker() : base("othello-outcome-marker")
        {
        }
    }
}
