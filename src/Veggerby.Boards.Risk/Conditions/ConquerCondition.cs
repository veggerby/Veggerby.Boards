using System;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Risk.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk.Conditions;

/// <summary>
/// Validates that a ConquerTerritoryGameEvent can be executed.
/// </summary>
/// <remarks>
/// Conditions:
/// - Territory must be in conquered state (ConqueredTerritoryState marker present)
/// - Moving armies must be ≥ minimum conquest armies (attacker dice count)
/// - Source territory must have enough armies to leave at least 1 behind
/// </remarks>
public sealed class ConquerCondition : IGameEventCondition<ConquerTerritoryGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, ConquerTerritoryGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Check active player
        if (!state.TryGetActivePlayer(out var activePlayer))
        {
            return ConditionResponse.NotApplicable;
        }

        if (!activePlayer!.Equals(@event.NewOwner))
        {
            return ConditionResponse.Fail("Player is not the active player.");
        }

        // Check territory state - should be ConqueredTerritoryState (set by CombatResolutionMutator)
        var conqueredState = state.GetState<ConqueredTerritoryState>(@event.Territory);

        if (conqueredState is null)
        {
            return ConditionResponse.Fail("Territory is not in conquered state.");
        }

        // Check MinimumConquestArmies in RiskStateExtras
        var riskExtras = state.GetExtras<RiskStateExtras>();

        if (riskExtras is null)
        {
            return ConditionResponse.Fail("Risk state not found.");
        }

        if (!riskExtras.MinimumConquestArmies.HasValue)
        {
            return ConditionResponse.Fail("No pending conquest (territory not yet conquered).");
        }

        if (@event.MovingArmies < riskExtras.MinimumConquestArmies.Value)
        {
            return ConditionResponse.Fail($"Must move at least {riskExtras.MinimumConquestArmies.Value} armies into conquered territory.");
        }

        // Check source territory has enough armies
        var fromState = state.GetState<TerritoryState>(@event.FromTerritory);

        if (fromState is null)
        {
            return ConditionResponse.Fail("Source territory state not found.");
        }

        if (!fromState.Owner.Equals(@event.NewOwner))
        {
            return ConditionResponse.Fail("Player does not own the source territory.");
        }

        // Must leave at least 1 army in source
        if (fromState.ArmyCount - @event.MovingArmies < 1)
        {
            return ConditionResponse.Fail("Must leave at least 1 army in the source territory.");
        }

        return ConditionResponse.Valid;
    }
}
