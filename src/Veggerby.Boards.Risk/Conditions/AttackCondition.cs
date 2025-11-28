using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Risk.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk.Conditions;

/// <summary>
/// Validates that an AttackGameEvent can be executed.
/// </summary>
/// <remarks>
/// Conditions:
/// - Must be in Attack phase
/// - Attacker must own source territory
/// - Defender must own target territory (not same player)
/// - Territories must be adjacent
/// - Attacker dice count must be ≤ (source armies - 1)
/// </remarks>
public sealed class AttackCondition : IGameEventCondition<AttackGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttackCondition"/> class.
    /// </summary>
    /// <param name="game">The game instance for adjacency checks.</param>
    public AttackCondition(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, AttackGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Check active player
        if (!state.TryGetActivePlayer(out var activePlayer))
        {
            return ConditionResponse.NotApplicable;
        }

        // Check phase
        var riskExtras = state.GetExtras<RiskStateExtras>();

        if (riskExtras is null)
        {
            return ConditionResponse.Fail("Risk state not found.");
        }

        if (riskExtras.CurrentPhase != RiskPhase.Attack)
        {
            return ConditionResponse.Fail("Not in attack phase.");
        }

        // Check territory ownership
        var fromState = state.GetState<TerritoryState>(@event.FromTerritory);
        var toState = state.GetState<TerritoryState>(@event.ToTerritory);

        if (fromState is null || toState is null)
        {
            return ConditionResponse.Fail("Territory state not found.");
        }

        if (!fromState.Owner.Equals(activePlayer))
        {
            return ConditionResponse.Fail("Player does not own the attacking territory.");
        }

        if (toState.Owner.Equals(activePlayer))
        {
            return ConditionResponse.Fail("Cannot attack own territory.");
        }

        // Check adjacency
        var isAdjacent = false;

        foreach (var relation in _game.Board.TileRelations)
        {
            if ((relation.From.Equals(@event.FromTerritory) && relation.To.Equals(@event.ToTerritory)) ||
                (relation.From.Equals(@event.ToTerritory) && relation.To.Equals(@event.FromTerritory)))
            {
                isAdjacent = true;
                break;
            }
        }

        if (!isAdjacent)
        {
            return ConditionResponse.Fail("Territories are not adjacent.");
        }

        // Check attacker dice count vs available armies
        // Must leave at least 1 army behind
        var maxDice = fromState.ArmyCount - 1;

        if (@event.AttackerDiceCount > maxDice)
        {
            return ConditionResponse.Fail($"Insufficient armies to attack with {@event.AttackerDiceCount} dice. Maximum: {maxDice}");
        }

        return ConditionResponse.Valid;
    }
}
