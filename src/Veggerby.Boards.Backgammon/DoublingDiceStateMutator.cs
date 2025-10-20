using System;
using System.Linq;

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
    public Dice DoublingDice
    {
        get;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, RollDiceGameEvent<int> @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        // Require active turn sequencing flag for multi-turn gating; if disabled behave as original single first doubling semantics.
        var turnState = state.GetStates<TurnState>().FirstOrDefault();

        // Active player now derived via centralized projection helper; inactive is the other player in two-player Backgammon
        Player? activePlayer = null;
        if (state.TryGetActivePlayer(out var ap))
        {
            activePlayer = ap;
        }

        Player? inactivePlayer = null;
        if (activePlayer is not null)
        {
            // find the other player (Backgammon is strictly two-player in this module)
            foreach (var p in engine.Game.Players)
            {
                if (!p.Equals(activePlayer))
                {
                    inactivePlayer = p;
                    break;
                }
            }
        }

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

            // Only the current owner (who must be on roll) may offer the next redouble in Backgammon cube semantics.
            // Thus the active player must equal the current owner; otherwise gating blocks.
            if (specialized.CurrentPlayer is null || activePlayer is null)
            {
                return state; // cannot redouble without an owner or active player projection
            }

            if (!activePlayer.Equals(specialized.CurrentPlayer))
            {
                return state; // opponent (non-owner) attempting to redouble – blocked
            }

            var generatorRedouble = new DoublingDiceValueGenerator();
            var nextValue = generatorRedouble.GetValue(specialized);
            var newOwner = inactivePlayer ?? specialized.CurrentPlayer; // ownership transfers to opponent
            if (newOwner is null)
            {
                return state; // cannot assign ownership
            }
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
        var firstOwner = inactivePlayer ?? activePlayer; // fallback: assign someone if opponent missing
        var lastTurn = turnState?.TurnNumber ?? 0;
        var firstOwnerNonNull = firstOwner ?? throw new InvalidOperationException("Unable to determine doubling dice owner.");
        var first = new DoublingDiceState(DoublingDice, value, firstOwnerNonNull, lastTurn);
        return state.Next([first]);
    }
}