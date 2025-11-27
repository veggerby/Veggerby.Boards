using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly;

/// <summary>
/// Marker artifact for Monopoly game outcome.
/// </summary>
internal sealed class MonopolyOutcomeMarker : Artifact
{
    public MonopolyOutcomeMarker() : base("monopoly-outcome-marker") { }
}

/// <summary>
/// Represents the outcome of a completed Monopoly game.
/// </summary>
public class MonopolyOutcomeState : IArtifactState, IGameOutcome
{
    private static readonly MonopolyOutcomeMarker Marker = new();

    /// <summary>
    /// Gets the winning player.
    /// </summary>
    public Player Winner
    {
        get;
    }

    /// <summary>
    /// Gets all player results in order of elimination (winner first).
    /// </summary>
    public IReadOnlyList<MonopolyPlayerResult> Results
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonopolyOutcomeState"/> class.
    /// </summary>
    public MonopolyOutcomeState(Player winner, IEnumerable<MonopolyPlayerResult> results)
    {
        ArgumentNullException.ThrowIfNull(winner);
        ArgumentNullException.ThrowIfNull(results);

        Winner = winner;
        Results = results.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        return other is MonopolyOutcomeState mos &&
               Equals(mos.Winner, Winner) &&
               mos.Results.Count == Results.Count &&
               mos.Results.Zip(Results, (a, b) => a.Equals(b)).All(x => x);
    }

    /// <inheritdoc />
    public string TerminalCondition => "LastPlayerStanding";

    /// <inheritdoc />
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            return Results.Select((r, index) => new PlayerResult
            {
                Player = r.Player,
                Outcome = r.IsBankrupt ? OutcomeType.Eliminated :
                          r.Player.Equals(Winner) ? OutcomeType.Win : OutcomeType.Loss,
                Rank = r.Player.Equals(Winner) ? 1 : index + 2,
                Metrics = new Dictionary<string, object>
                {
                    ["FinalCash"] = r.FinalCash,
                    ["PropertiesOwned"] = r.PropertiesOwned
                }
            }).ToList();
        }
    }
}

/// <summary>
/// Represents a single player's result in a Monopoly game.
/// </summary>
public sealed record MonopolyPlayerResult
{
    /// <summary>
    /// Gets the player.
    /// </summary>
    public required Player Player
    {
        get; init;
    }

    /// <summary>
    /// Gets whether the player went bankrupt.
    /// </summary>
    public required bool IsBankrupt
    {
        get; init;
    }

    /// <summary>
    /// Gets the final cash amount.
    /// </summary>
    public required int FinalCash
    {
        get; init;
    }

    /// <summary>
    /// Gets the number of properties owned at end.
    /// </summary>
    public required int PropertiesOwned
    {
        get; init;
    }
}
