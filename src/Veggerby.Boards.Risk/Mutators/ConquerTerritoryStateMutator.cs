using System;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Risk.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk.Mutators;

/// <summary>
/// Applies territory conquest after defender elimination.
/// </summary>
public sealed class ConquerTerritoryStateMutator : IStateMutator<ConquerTerritoryGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, ConquerTerritoryGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var fromState = gameState.GetState<TerritoryState>(@event.FromTerritory);

        if (fromState is null)
        {
            throw new InvalidOperationException($"Source territory state not found for '{@event.FromTerritory.Id}'.");
        }

        // Get Risk extras
        var riskExtras = gameState.GetExtras<RiskStateExtras>();

        if (riskExtras is null)
        {
            throw new InvalidOperationException("Risk state not found.");
        }

        // Update source territory (reduce armies)
        var newFromState = new TerritoryState(
            fromState.Territory,
            fromState.Owner,
            fromState.ArmyCount - @event.MovingArmies);

        // Create new territory state for conquered territory
        var newToState = new TerritoryState(
            @event.Territory,
            @event.NewOwner,
            @event.MovingArmies);

        // Clear MinimumConquestArmies
        var newRiskExtras = riskExtras with { MinimumConquestArmies = null };

        return gameState.ReplaceExtras(newRiskExtras).Next([newFromState, newToState]);
    }
}
