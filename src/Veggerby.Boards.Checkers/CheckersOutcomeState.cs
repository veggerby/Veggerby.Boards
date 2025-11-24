using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Checkers;

/// <summary>
/// Outcome state for checkers games implementing <see cref="IGameOutcome"/> for unified termination tracking.
/// </summary>
public sealed class CheckersOutcomeState : IArtifactState, IGameOutcome
{
    private static readonly CheckersOutcomeMarker Marker = new();

    /// <summary>
    /// Gets the artifact marker.
    /// </summary>
    public Artifact Artifact => Marker;

    /// <summary>
    /// Gets the winner of the game (null if draw, though draws are rare in checkers).
    /// </summary>
    public Player? Winner
    {
        get;
    }

    /// <summary>
    /// Gets the terminal condition description.
    /// </summary>
    public string TerminalCondition => Winner != null ? "Victory" : "Draw";

    /// <summary>
    /// Gets the ordered player results.
    /// </summary>
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckersOutcomeState"/> class.
    /// </summary>
    /// <param name="winner">The winning player (null for draw).</param>
    /// <param name="players">All players in the game.</param>
    public CheckersOutcomeState(Player? winner, IEnumerable<Player> players)
    {
        Winner = winner;

        var playerList = players.ToList();

        if (winner == null)
        {
            // Draw - both players tied (rare in checkers)
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
        return other is CheckersOutcomeState otherOutcome
            && Winner == otherOutcome.Winner;
    }

    private sealed class CheckersOutcomeMarker : Artifact
    {
        public CheckersOutcomeMarker() : base("checkers-outcome-marker")
        {
        }
    }
}
