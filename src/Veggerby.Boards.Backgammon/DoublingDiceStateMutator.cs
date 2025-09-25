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
        // Require active turn sequencing flag for multi-turn gating; if disabled behave as original single first doubling semantics.
        var turnState = state.GetStates<TurnState>().FirstOrDefault();

        var activePlayer = state.GetStates<ActivePlayerState>().SingleOrDefault(x => x.IsActive)?.Artifact;
        var inactivePlayer = state.GetStates<ActivePlayerState>().SingleOrDefault(x => !x.IsActive)?.Artifact;

        var specialized = state.GetState<DoublingDiceState>(DoublingDice);
        if (specialized is not null)
        {
            // Already doubled before. If no turn state available we cannot perform multi-turn redouble logic.
            if (turnState is null)
            {
                return state;
            }

            // Gated: allow redouble only if current turn number > last doubled turn on cube state.
            if (turnState.TurnNumber <= specialized.LastDoubledTurn)
            {
                return state; // same turn – blocked
            }

            // Ownership must alternate: only opponent of current owner can redouble.
            if (specialized.CurrentPlayer is null || activePlayer is null || !activePlayer.Equals(specialized.CurrentPlayer))
            {
                // Only owner (the player who possesses cube) cannot redouble on their own turn; opponent triggers new doubling
                // If active player equals owner we block.
                if (activePlayer is not null && specialized.CurrentPlayer is not null && activePlayer.Equals(specialized.CurrentPlayer))
                {
                    return state;
                }
            }

            var generatorRedouble = new DoublingDiceValueGenerator();
            var nextValue = generatorRedouble.GetValue(specialized);
            var newOwner = inactivePlayer ?? specialized.CurrentPlayer; // transfer back to opponent (inactive at time of double)
            var upgraded = new DoublingDiceState(DoublingDice, nextValue, newOwner, turnState.TurnNumber);
            return state.Next([upgraded]);
        }

        // First doubling path – operate on generic base state.
        var baseState = state.GetState<DiceState<int>>(DoublingDice);
        if (baseState is null)
        {
            throw new InvalidOperationException("Doubling dice state not initialized.");
        }

        var generator = new DoublingDiceValueGenerator();
        var value = generator.GetValue(baseState);
        var firstOwner = inactivePlayer; // inactive player receives ownership
        var lastTurn = turnState?.TurnNumber ?? 0;
        var first = new DoublingDiceState(DoublingDice, value, firstOwner, lastTurn);
        return state.Next([first]);
    }
}