using System;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Risk.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk.Mutators;

/// <summary>
/// Applies army placement during the reinforcement phase.
/// </summary>
public sealed class PlaceArmiesStateMutator : IStateMutator<PlaceArmiesGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, PlaceArmiesGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var territoryState = gameState.GetState<TerritoryState>(@event.Territory);

        if (territoryState is null)
        {
            throw new InvalidOperationException($"Territory state not found for '{@event.Territory.Id}'.");
        }

        // Update territory army count
        var newTerritoryState = territoryState.WithArmyDelta(@event.ArmyCount);

        // Update reinforcements remaining
        var riskExtras = gameState.GetExtras<RiskStateExtras>();

        if (riskExtras is null)
        {
            throw new InvalidOperationException("Risk state not found.");
        }

        var newRemaining = riskExtras.ReinforcementsRemaining - @event.ArmyCount;
        var newRiskExtras = riskExtras with { ReinforcementsRemaining = newRemaining };

        // If all reinforcements placed, transition to Attack phase
        if (newRemaining <= 0)
        {
            newRiskExtras = newRiskExtras with { CurrentPhase = RiskPhase.Attack };
        }

        return gameState.ReplaceExtras(newRiskExtras).Next([newTerritoryState]);
    }
}
