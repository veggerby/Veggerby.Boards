using System;

using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

/// <summary>
/// Stateless mutator producing a single (optional) artifact state delta based on the incoming event.
/// </summary>
/// <typeparam name="T">Event type.</typeparam>
public class SimpleGameStateMutator<T> : IStateMutator<T> where T : IGameEvent
{
    private readonly Func<T, IArtifactState> _stateFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleGameStateMutator{T}"/> class.
    /// </summary>
    /// <param name="stateFunc">Delegate producing a new artifact state or null (no change).</param>
    public SimpleGameStateMutator(Func<T, IArtifactState> stateFunc)
    {
        ArgumentNullException.ThrowIfNull(stateFunc);

        _stateFunc = stateFunc;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, T @event)
    {
        var artifactState = _stateFunc(@event);
        if (artifactState is null)
        {
            return gameState;
        }

        return gameState.Next([artifactState]);
    }
}