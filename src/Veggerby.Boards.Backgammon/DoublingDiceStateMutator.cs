using System;
using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Mutator that updates the doubling cube value transferring ownership to the opponent.
/// </summary>
public class DoublingDiceStateMutator : IStateMutator<RollDiceGameEvent<int>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DoublingDiceStateMutator"/> class.
    /// </summary>
    public DoublingDiceStateMutator(Dice doublingDice)
    {
        ArgumentNullException.ThrowIfNull(doublingDice);

        DoublingDice = doublingDice;
    }

    /// <summary>
    /// Gets the doubling dice artifact.
    /// </summary>
    public Dice DoublingDice { get; }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, RollDiceGameEvent<int> @event)
    {
        // If specialized state already exists we no-op (multi-turn redoubling deferred until turn sequencing implemented).
        var specialized = state.GetState<DoublingDiceState>(DoublingDice);
        if (specialized is not null)
        {
            return state; // already doubled
        }

        // Otherwise retrieve generic base state for first doubling.
        var current = state.GetState<DiceState<int>>(DoublingDice);
        if (current is null)
        {
            throw new InvalidOperationException("Doubling dice state not initialized.");
        }
        var nonActivePlayer = state.GetStates<ActivePlayerState>().SingleOrDefault(x => !x.IsActive);
        var generator = new DoublingDiceValueGenerator();
        var newValue = generator.GetValue(current);
        var owner = nonActivePlayer?.Artifact; // ownership transfers to opponent of active player
        var newState = new DoublingDiceState(DoublingDice, newValue, owner);
        return state.Next([newState]);
    }
}