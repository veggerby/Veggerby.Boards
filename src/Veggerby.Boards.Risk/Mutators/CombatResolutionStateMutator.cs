using System;
using System.Collections.Generic;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Risk.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk.Mutators;

/// <summary>
/// Resolves combat between attacker and defender using deterministic dice rolling.
/// </summary>
/// <remarks>
/// Uses CombatResolver to determine losses. If defender is eliminated, sets MinimumConquestArmies
/// to trigger ConquerTerritoryGameEvent.
/// </remarks>
public sealed class CombatResolutionStateMutator : IStateMutator<AttackGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, AttackGameEvent @event)
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

        var random = gameState.Random;

        if (random is null)
        {
            throw new InvalidOperationException("Random source not available for combat resolution.");
        }

        // Resolve combat
        var result = CombatResolver.Resolve(@event.AttackerDiceCount, toState.ArmyCount, random);

        // Update attacker armies
        var newFromArmies = fromState.ArmyCount - result.AttackerLosses;
        var newFromState = new TerritoryState(fromState.Territory, fromState.Owner, newFromArmies);

        // Update defender armies (minimum 1 for state validity unless eliminated)
        var newDefenderArmies = toState.ArmyCount - result.DefenderLosses;

        var newStates = new List<IArtifactState> { newFromState };

        // Get Risk extras
        var riskExtras = gameState.GetExtras<RiskStateExtras>();

        if (riskExtras is null)
        {
            throw new InvalidOperationException("Risk state not found.");
        }

        RiskStateExtras newRiskExtras;

        if (result.DefenderEliminated)
        {
            // Defender eliminated - territory conquered
            // Set MinimumConquestArmies to attacker dice count
            // Territory state will be updated by ConquerTerritoryStateMutator
            newRiskExtras = riskExtras with
            {
                MinimumConquestArmies = @event.AttackerDiceCount,
                ConqueredThisTurn = true
            };

            // Create a temporary state with 0 armies to mark as conquered
            // The actual ownership change happens in ConquerTerritoryStateMutator
            var conqueredState = new ConqueredTerritoryState(toState.Territory, toState.Owner);
            newStates.Add(conqueredState);
        }
        else
        {
            // Update defender territory with reduced armies
            var newToState = new TerritoryState(toState.Territory, toState.Owner, newDefenderArmies);
            newStates.Add(newToState);
            newRiskExtras = riskExtras; // No change to extras
        }

        return gameState.ReplaceExtras(newRiskExtras).Next(newStates);
    }
}
