using System;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Risk.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk.Conditions;

/// <summary>
/// Validates that a PlaceArmiesGameEvent can be executed.
/// </summary>
/// <remarks>
/// Conditions:
/// - Player must be the active player
/// - Player must own the target territory
/// - Player must have sufficient reinforcements remaining
/// </remarks>
public sealed class PlaceArmiesCondition : IGameEventCondition<PlaceArmiesGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, PlaceArmiesGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Check active player
        if (!state.TryGetActivePlayer(out var activePlayer))
        {
            return ConditionResponse.NotApplicable;
        }

        if (!activePlayer!.Equals(@event.Player))
        {
            return ConditionResponse.Fail("Player is not the active player.");
        }

        // Check territory ownership
        var territoryState = state.GetState<TerritoryState>(@event.Territory);

        if (territoryState is null)
        {
            return ConditionResponse.Fail("Territory state not found.");
        }

        if (!territoryState.Owner.Equals(@event.Player))
        {
            return ConditionResponse.Fail("Player does not own this territory.");
        }

        // Check reinforcements remaining
        var riskExtras = state.GetExtras<RiskStateExtras>();

        if (riskExtras is null)
        {
            return ConditionResponse.Fail("Risk state not found.");
        }

        if (riskExtras.CurrentPhase != RiskPhase.Reinforce)
        {
            return ConditionResponse.Fail("Not in reinforcement phase.");
        }

        if (@event.ArmyCount > riskExtras.ReinforcementsRemaining)
        {
            return ConditionResponse.Fail($"Insufficient reinforcements. Remaining: {riskExtras.ReinforcementsRemaining}");
        }

        return ConditionResponse.Valid;
    }
}
