using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Utilities.Scoring;

/// <summary>
/// Fluent builder for constructing game outcomes.
/// </summary>
/// <remarks>
/// Provides a convenient API for building <see cref="IGameOutcome"/> instances
/// with ranked players and terminal conditions.
/// <para>
/// Note: This builder is stateful and maintains an internal list of player results.
/// The Build() method should only be called once after all players have been added.
/// Each With* method modifies the builder's internal state and returns the same instance for chaining.
/// </para>
/// </remarks>
public sealed class OutcomeBuilder
{
    private readonly List<PlayerResult> _results = new();
    private string _terminalCondition = "GameEnded";

    /// <summary>
    /// Adds a winning player to the outcome.
    /// </summary>
    /// <param name="player">The winning player.</param>
    /// <param name="metrics">Optional game-specific metrics.</param>
    /// <returns>This builder for chaining.</returns>
    public OutcomeBuilder WithWinner(Player player, Dictionary<string, object>? metrics = null)
    {
        _results.Add(new PlayerResult
        {
            Player = player,
            Rank = 1,
            Outcome = OutcomeType.Win,
            Metrics = metrics
        });

        return this;
    }

    /// <summary>
    /// Adds a losing player to the outcome.
    /// </summary>
    /// <param name="player">The losing player.</param>
    /// <param name="metrics">Optional game-specific metrics.</param>
    /// <returns>This builder for chaining.</returns>
    /// <remarks>
    /// The rank is automatically assigned based on the order players are added.
    /// </remarks>
    public OutcomeBuilder WithLoser(Player player, Dictionary<string, object>? metrics = null)
    {
        var nextRank = _results.Count + 1;

        _results.Add(new PlayerResult
        {
            Player = player,
            Rank = nextRank,
            Outcome = OutcomeType.Loss,
            Metrics = metrics
        });

        return this;
    }

    /// <summary>
    /// Adds ranked players from score results.
    /// </summary>
    /// <param name="scores">The ranked scores.</param>
    /// <returns>This builder for chaining.</returns>
    /// <remarks>
    /// If only one player has rank 1, they are marked as the winner.
    /// If multiple players are tied at rank 1, they are all marked as Draw.
    /// All other players are assigned Loss outcome.
    /// </remarks>
    public OutcomeBuilder WithRankedPlayers(IEnumerable<PlayerScore> scores)
    {
        ArgumentNullException.ThrowIfNull(scores);

        var scoresList = scores.ToList();

        // Count how many players are tied at rank 1
        var rankOneCount = 0;

        foreach (var score in scoresList)
        {
            if (score.Rank == 1)
            {
                rankOneCount++;
            }
        }

        foreach (var score in scoresList)
        {
            OutcomeType outcome;

            if (score.Rank == 1)
            {
                outcome = rankOneCount > 1 ? OutcomeType.Draw : OutcomeType.Win;
            }
            else
            {
                outcome = OutcomeType.Loss;
            }

            _results.Add(new PlayerResult
            {
                Player = score.Player,
                Rank = score.Rank,
                Outcome = outcome,
                Metrics = new Dictionary<string, object>
                {
                    ["Score"] = score.Score
                }
            });
        }

        return this;
    }

    /// <summary>
    /// Adds tied players (all at rank 1 with Draw outcome).
    /// </summary>
    /// <param name="players">The tied players.</param>
    /// <param name="metrics">Optional shared metrics for all tied players.</param>
    /// <returns>This builder for chaining.</returns>
    public OutcomeBuilder WithTiedPlayers(IEnumerable<Player> players, Dictionary<string, object>? metrics = null)
    {
        ArgumentNullException.ThrowIfNull(players);

        foreach (var player in players)
        {
            _results.Add(new PlayerResult
            {
                Player = player,
                Rank = 1,
                Outcome = OutcomeType.Draw,
                Metrics = metrics
            });
        }

        return this;
    }

    /// <summary>
    /// Sets the terminal condition description.
    /// </summary>
    /// <param name="condition">The terminal condition (e.g., "Checkmate", "Scoring", "Elimination").</param>
    /// <returns>This builder for chaining.</returns>
    public OutcomeBuilder WithTerminalCondition(string condition)
    {
        _terminalCondition = condition ?? "GameEnded";

        return this;
    }

    /// <summary>
    /// Builds the final outcome state.
    /// </summary>
    /// <returns>A standard game outcome instance.</returns>
    public IGameOutcome Build()
    {
        return new StandardGameOutcome
        {
            TerminalCondition = _terminalCondition,
            PlayerResults = _results.ToList()
        };
    }
}
