using System;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public class DiceStateMutator<T> : IStateMutator<RollDiceGameEvent<T>>
    {
        public GameState MutateState(GameState gameState, RollDiceGameEvent<T> @event)
        {
            var newState = new DiceState<T>(@event.Dice, @event.NewDiceValue);
            return gameState.Next(new [] { newState });
        }
    }
}