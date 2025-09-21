using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

/// <summary>
/// Chains multiple mutators executing them sequentially against the evolving state.
/// </summary>
public class CompositeStateMutator<T> : IStateMutator<T> where T : IGameEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeStateMutator{T}"/> class.
    /// </summary>
    /// <param name="childMutators">Ordered mutators.</param>
    public CompositeStateMutator(IEnumerable<IStateMutator<T>> childMutators)
    {
        ArgumentNullException.ThrowIfNull(childMutators);

        if (!childMutators.Any())
        {
            throw new ArgumentException("Must provide at least one child mutator", nameof(childMutators));
        }

        ChildMutators = childMutators;
    }

    /// <summary>
    /// Gets the child mutators.
    /// </summary>
    public IEnumerable<IStateMutator<T>> ChildMutators { get; }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, T @event)
    {
        return ChildMutators.Aggregate(gameState, (seed, mutator) => mutator.MutateState(engine, seed, @event));
    }
}