using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Marker artifact for backgammon game outcome.
/// </summary>
internal sealed class BackgammonOutcomeMarker : Artifact
{
    public BackgammonOutcomeMarker() : base("backgammon-outcome-marker") { }
}

/// <summary>
/// Represents the type of victory in backgammon.
/// </summary>
public enum BackgammonVictoryType
{
    /// <summary>
    /// Normal win - winner bears off all checkers, opponent has borne off at least one.
    /// Worth 1 point.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Gammon - winner bears off all, opponent hasn't borne off any.
    /// Worth 2 points.
    /// </summary>
    Gammon = 2,

    /// <summary>
    /// Backgammon - winner bears off all, opponent hasn't borne off any AND has checkers on bar or in winner's home.
    /// Worth 3 points.
    /// </summary>
    Backgammon = 3
}

/// <summary>
/// Immutable state representing the final outcome of a backgammon game.
/// </summary>
/// <remarks>
/// This state is added when the game ends (one player bears off all checkers).
/// It implements <see cref="IGameOutcome"/> to provide standardized outcome information.
/// </remarks>
public sealed class BackgammonOutcomeState : IArtifactState, IGameOutcome
{
    private static readonly BackgammonOutcomeMarker Marker = new();

    /// <summary>
    /// Gets the player who won.
    /// </summary>
    public Player Winner { get; }

    /// <summary>
    /// Gets the type of victory (Normal, Gammon, or Backgammon).
    /// </summary>
    public BackgammonVictoryType VictoryType { get; }

    /// <summary>
    /// Gets the points awarded for this victory (1, 2, or 3).
    /// </summary>
    public int Points => (int)VictoryType;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgammonOutcomeState"/> class.
    /// </summary>
    /// <param name="winner">The winning player.</param>
    /// <param name="victoryType">The type of victory.</param>
    public BackgammonOutcomeState(Player winner, BackgammonVictoryType victoryType)
    {
        Winner = winner ?? throw new ArgumentNullException(nameof(winner));
        VictoryType = victoryType;
    }

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        return other is BackgammonOutcomeState bos &&
               Equals(bos.Winner, Winner) &&
               bos.VictoryType == VictoryType;
    }

    /// <inheritdoc />
    public string TerminalCondition => VictoryType.ToString();

    /// <inheritdoc />
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            // In backgammon, there is always a winner and a loser (no draws)
            var results = new List<PlayerResult>
            {
                new PlayerResult
                {
                    Player = Winner,
                    Outcome = OutcomeType.Win,
                    Rank = 1,
                    Metrics = new Dictionary<string, object>
                    {
                        ["VictoryType"] = VictoryType.ToString(),
                        ["Points"] = Points
                    }
                }
            };

            return results;
        }
    }
}
