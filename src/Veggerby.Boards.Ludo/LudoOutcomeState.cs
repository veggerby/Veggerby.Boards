using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Ludo;

/// <summary>
/// Marker artifact for Ludo game outcome.
/// </summary>
internal sealed class LudoOutcomeMarker : Artifact
{
    public LudoOutcomeMarker() : base("ludo-outcome-marker") { }
}

/// <summary>
/// Represents the outcome of a completed Ludo game.
/// </summary>
public class LudoOutcomeState : IArtifactState, IGameOutcome
{
    private static readonly LudoOutcomeMarker Marker = new();

    /// <summary>
    /// Gets the winning player.
    /// </summary>
    public Player Winner
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LudoOutcomeState"/> class.
    /// </summary>
    /// <param name="winner">The winning player.</param>
    public LudoOutcomeState(Player winner)
    {
        ArgumentNullException.ThrowIfNull(winner);

        Winner = winner;
    }

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        return other is LudoOutcomeState los &&
               Equals(los.Winner, Winner);
    }

    /// <inheritdoc />
    public string TerminalCondition => "AllPiecesHome";

    /// <inheritdoc />
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            var results = new List<PlayerResult>
            {
                new PlayerResult
                {
                    Player = Winner,
                    Outcome = OutcomeType.Win,
                    Rank = 1
                }
            };

            return results;
        }
    }
}
