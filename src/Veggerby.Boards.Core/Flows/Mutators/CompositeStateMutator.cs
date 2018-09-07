using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public class CompositeStateMutator<T> : IStateMutator<T> where T : IGameEvent
    {
        public CompositeStateMutator(IEnumerable<IStateMutator<T>> childMutators)
        {
            if (childMutators == null)
            {
                throw new ArgumentNullException(nameof(childMutators));
            }

            if (!childMutators.Any())
            {
                throw new ArgumentException("Must provide at least one child mutator", nameof(childMutators));
            }

            ChildMutators = childMutators;
        }

        public IEnumerable<IStateMutator<T>> ChildMutators { get; }

        public GameState MutateState(GameState gameState, T @event)
        {
            return ChildMutators.Aggregate(gameState, (seed, mutator) => mutator.MutateState(seed, @event));
        }
    }
}