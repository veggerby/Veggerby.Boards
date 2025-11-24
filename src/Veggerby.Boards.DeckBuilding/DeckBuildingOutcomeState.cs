using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.DeckBuilding.States;
using Veggerby.Boards.States;
namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Marker artifact for deck building game outcome.
/// </summary>
internal sealed class DeckBuildingOutcomeMarker : Artifact
{
    public DeckBuildingOutcomeMarker() : base("deck-building-outcome-marker") { }
}

/// <summary>
/// Immutable state representing the final outcome of a deck building game based on victory points.
/// </summary>
/// <remarks>
/// This state wraps the individual player <see cref="ScoreState"/> entries to provide standardized
/// outcome information via <see cref="IGameOutcome"/>.
/// </remarks>
public sealed class DeckBuildingOutcomeState : IArtifactState, IGameOutcome
{
    private static readonly DeckBuildingOutcomeMarker Marker = new();

    /// <summary>
    /// Gets the individual score states for each player.
    /// </summary>
    public IReadOnlyList<ScoreState> Scores
    {
        get;
    }

    /// <summary>
    /// Gets the player who won (null if tie).
    /// </summary>
    public Player? Winner
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeckBuildingOutcomeState"/> class.
    /// </summary>
    /// <param name="scores">The score states for all players.</param>
    public DeckBuildingOutcomeState(IEnumerable<ScoreState> scores)
    {
        ArgumentNullException.ThrowIfNull(scores);

        Scores = scores.OrderByDescending(s => s.VictoryPoints).ToList();

        // Determine winner
        if (Scores.Count > 0)
        {
            var maxScore = Scores[0].VictoryPoints;
            var topScorers = Scores.Where(s => s.VictoryPoints == maxScore).ToList();

            // If exactly one player has the highest score, they win
            if (topScorers.Count == 1)
            {
                Winner = topScorers[0].Artifact;
            }
            // If multiple players tied for highest score, no winner (tie)
            else
            {
                Winner = null;
            }
        }
        else
        {
            Winner = null;
        }
    }

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        if (other is not DeckBuildingOutcomeState dbo)
        {
            return false;
        }

        if (Scores.Count != dbo.Scores.Count)
        {
            return false;
        }

        for (var i = 0; i < Scores.Count; i++)
        {
            if (!Scores[i].Equals(dbo.Scores[i]))
            {
                return false;
            }
        }

        return Equals(Winner, dbo.Winner);
    }

    /// <inheritdoc />
    public string TerminalCondition => "Scoring";

    /// <inheritdoc />
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            var results = new List<PlayerResult>();
            var currentRank = 1;

            for (var i = 0; i < Scores.Count; i++)
            {
                var score = Scores[i];

                // Check if this is a tie with the previous player
                if (i > 0 && Scores[i - 1].VictoryPoints == score.VictoryPoints)
                {
                    // Same rank as previous
                }
                else
                {
                    currentRank = i + 1;
                }

                var outcome = score.Artifact.Equals(Winner)
                    ? OutcomeType.Win
                    : (currentRank == 1 ? OutcomeType.Draw : OutcomeType.Loss);

                results.Add(new PlayerResult
                {
                    Player = score.Artifact,
                    Rank = currentRank,
                    Outcome = outcome,
                    Metrics = new Dictionary<string, object>
                    {
                        ["VictoryPoints"] = score.VictoryPoints
                    }
                });
            }

            return results;
        }
    }
}
