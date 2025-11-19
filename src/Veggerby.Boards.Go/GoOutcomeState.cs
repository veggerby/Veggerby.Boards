using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Go;

/// <summary>
/// Marker artifact for Go game outcome.
/// </summary>
internal sealed class GoOutcomeMarker : Artifact
{
    public GoOutcomeMarker() : base("go-outcome-marker") { }
}

/// <summary>
/// Immutable state representing the final outcome of a Go game after territory scoring.
/// </summary>
/// <remarks>
/// This state is added when the game ends (after two consecutive passes) and territory scores have been computed.
/// It implements <see cref="IGameOutcome"/> to provide standardized outcome information.
/// </remarks>
public sealed class GoOutcomeState : IArtifactState, IGameOutcome
{
    private static readonly GoOutcomeMarker Marker = new();

    /// <summary>
    /// Gets the territory scores for each player (empty intersections controlled).
    /// </summary>
    public IReadOnlyDictionary<Player, int> TerritoryScores
    {
        get;
    }

    /// <summary>
    /// Gets the stone counts for each player (stones on the board).
    /// </summary>
    public IReadOnlyDictionary<Player, int> StoneCounts
    {
        get;
    }

    /// <summary>
    /// Gets the total area scores for each player (territory + stones).
    /// </summary>
    public IReadOnlyDictionary<Player, int> TotalScores
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
    /// Initializes a new instance of the <see cref="GoOutcomeState"/> class.
    /// </summary>
    /// <param name="territoryScores">Territory scores for each player.</param>
    /// <param name="stoneCounts">Stone counts for each player.</param>
    /// <param name="totalScores">Total area scores for each player.</param>
    /// <param name="winner">The winning player (null if tie).</param>
    public GoOutcomeState(
        IReadOnlyDictionary<Player, int> territoryScores,
        IReadOnlyDictionary<Player, int> stoneCounts,
        IReadOnlyDictionary<Player, int> totalScores,
        Player? winner)
    {
        TerritoryScores = territoryScores ?? throw new ArgumentNullException(nameof(territoryScores));
        StoneCounts = stoneCounts ?? throw new ArgumentNullException(nameof(stoneCounts));
        TotalScores = totalScores ?? throw new ArgumentNullException(nameof(totalScores));
        Winner = winner;
    }

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        if (other is not GoOutcomeState gos)
        {
            return false;
        }

        return Equals(Winner, gos.Winner) &&
               DictionariesEqual(TotalScores, gos.TotalScores);
    }

    private static bool DictionariesEqual<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> a, IReadOnlyDictionary<TKey, TValue> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (var kvp in a)
        {
            if (!b.TryGetValue(kvp.Key, out var bValue) || !Equals(kvp.Value, bValue))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public string TerminalCondition => "TerritoryScoring";

    /// <inheritdoc />
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            var ranked = TotalScores
                .OrderByDescending(kv => kv.Value)
                .ToList();

            // Handle ties: players with same score get same rank
            var results = new List<PlayerResult>();
            var currentRank = 1;

            for (var i = 0; i < ranked.Count; i++)
            {
                var score = ranked[i].Value;
                var player = ranked[i].Key;

                // Check if this is a tie with the previous player
                if (i > 0 && ranked[i - 1].Value == score)
                {
                    // Same rank as previous
                }
                else
                {
                    currentRank = i + 1;
                }

                var outcome = player.Equals(Winner)
                    ? OutcomeType.Win
                    : (currentRank == 1 ? OutcomeType.Draw : OutcomeType.Loss);

                results.Add(new PlayerResult
                {
                    Player = player,
                    Rank = currentRank,
                    Outcome = outcome,
                    Metrics = new Dictionary<string, object>
                    {
                        ["Territory"] = TerritoryScores.TryGetValue(player, out var territory) ? territory : 0,
                        ["Stones"] = StoneCounts.TryGetValue(player, out var stones) ? stones : 0,
                        ["TotalScore"] = score
                    }
                });
            }

            return results;
        }
    }
}
