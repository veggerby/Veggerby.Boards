using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Risk.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk.Mutators;

/// <summary>
/// Applies fortification movement between owned connected territories.
/// </summary>
public sealed class FortifyStateMutator : IStateMutator<FortifyGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, FortifyGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var fromState = gameState.GetState<TerritoryState>(@event.FromTerritory);
        var toState = gameState.GetState<TerritoryState>(@event.ToTerritory);

        if (fromState is null || toState is null)
        {
            throw new InvalidOperationException("Territory state not found.");
        }

        // Update source territory (reduce armies)
        var newFromState = new TerritoryState(
            fromState.Territory,
            fromState.Owner,
            fromState.ArmyCount - @event.ArmyCount);

        // Update destination territory (increase armies)
        var newToState = new TerritoryState(
            toState.Territory,
            toState.Owner,
            toState.ArmyCount + @event.ArmyCount);

        return gameState.Next(new IArtifactState[] { newFromState, newToState });
    }
}
