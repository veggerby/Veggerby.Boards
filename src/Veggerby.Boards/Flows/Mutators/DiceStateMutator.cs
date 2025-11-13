using System;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Applies dice state updates produced by a roll event.
/// </summary>
public class DiceStateMutator<T> : IStateMutator<RollDiceGameEvent<T>>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, RollDiceGameEvent<T> @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);
        return gameState.Next(@event.NewDiceStates);
    }
}