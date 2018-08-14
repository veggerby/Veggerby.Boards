using System;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public class DiceStateMutator<T> : IStateMutator<RollDiceGameEvent<T>>
    {
        public IArtifactState MutateState(IArtifactState currentState, RollDiceGameEvent<T> @event)
        {
            if (!currentState.Artifact.Equals(@event.Dice))
            {
                throw new ArgumentException("Incorrect artifact state", nameof(currentState));
            }

            return new DiceState<T>(@event.Dice, @event.NewDiceValue);
        }
    }
}