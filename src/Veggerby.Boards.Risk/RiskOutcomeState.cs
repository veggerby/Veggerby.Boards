using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk;

/// <summary>
/// Marker artifact for Risk game outcome.
/// </summary>
internal sealed class RiskOutcomeMarker : Artifact
{
    public RiskOutcomeMarker() : base("risk-outcome-marker") { }
}

/// <summary>
/// Immutable state representing the final outcome of a Risk game.
/// </summary>
/// <remarks>
/// This state is added when the game ends (one player controls all territories - world domination).
/// It implements <see cref="IGameOutcome"/> to provide standardized outcome information.
/// </remarks>
public sealed class RiskOutcomeState : IArtifactState, IGameOutcome
{
    private static readonly RiskOutcomeMarker Marker = new();

    private readonly Player _winner;
    private readonly IReadOnlyList<Player> _eliminationOrder;

    /// <summary>
    /// Gets the player who won (conquered all territories).
    /// </summary>
    public Player Winner => _winner;

    /// <summary>
    /// Gets the order in which players were eliminated (first eliminated to last).
    /// </summary>
    public IReadOnlyList<Player> EliminationOrder => _eliminationOrder;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiskOutcomeState"/> class.
    /// </summary>
    /// <param name="winner">The winning player.</param>
    /// <param name="eliminationOrder">The order players were eliminated (first eliminated first).</param>
    public RiskOutcomeState(Player winner, IReadOnlyList<Player>? eliminationOrder = null)
    {
        _winner = winner ?? throw new ArgumentNullException(nameof(winner));
        _eliminationOrder = eliminationOrder ?? Array.Empty<Player>();
    }

    /// <inheritdoc />
    public Artifact Artifact => Marker;

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        return other is RiskOutcomeState ros &&
               Equals(ros._winner, _winner);
    }

    /// <inheritdoc />
    public string TerminalCondition => "WorldDomination";

    /// <inheritdoc />
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            var results = new List<PlayerResult>
            {
                new PlayerResult
                {
                    Player = _winner,
                    Outcome = OutcomeType.Win,
                    Rank = 1,
                    Metrics = new Dictionary<string, object>
                    {
                        ["TotalTerritories"] = 42 // Will be updated with actual count
                    }
                }
            };

            // Add eliminated players in reverse order (last eliminated = second place)
            var rank = 2;

            for (int i = _eliminationOrder.Count - 1; i >= 0; i--)
            {
                results.Add(new PlayerResult
                {
                    Player = _eliminationOrder[i],
                    Outcome = OutcomeType.Eliminated,
                    Rank = rank++,
                    Metrics = new Dictionary<string, object>
                    {
                        ["EliminationOrder"] = _eliminationOrder.Count - i
                    }
                });
            }

            return results;
        }
    }
}
