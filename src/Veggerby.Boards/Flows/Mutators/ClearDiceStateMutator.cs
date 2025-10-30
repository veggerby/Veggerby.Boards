using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Consumes a dice matching the distance of the executed move (sets it to null state).
/// </summary>
public class ClearDiceStateMutator : IStateMutator<MovePieceGameEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearDiceStateMutator"/> class.
    /// </summary>
    /// <param name="dice">Candidate dice collection.</param>
    public ClearDiceStateMutator(IEnumerable<Dice> dice)
    {
        ArgumentNullException.ThrowIfNull(dice);

        if (!dice.Any())
        {
            throw new ArgumentException(ExceptionMessages.AtLeastOneDiceRequired, nameof(dice));
        }

        Dice = dice;
    }

    /// <summary>
    /// Gets the dice considered for clearing.
    /// </summary>
    public IEnumerable<Dice> Dice
    {
        get;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        var diceState = Dice
            .Select(dice => state.GetState<DiceState<int>>(dice))
            .FirstOrDefault(x => x is not null && x.CurrentValue.Equals(@event.Path.Distance));

        if (diceState is null)
        {
            throw new BoardException("No valid dice state for path");
        }

        var newState = new NullDiceState(diceState.Artifact);
        return state.Next([newState]);
    }
}