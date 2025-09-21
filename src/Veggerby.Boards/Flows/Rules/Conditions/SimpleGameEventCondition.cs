using System;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Adapts a delegate into an <see cref="IGameEventCondition{T}"/>.
/// </summary>
public class SimpleGameEventCondition<T> : IGameEventCondition<T> where T : IGameEvent
{
    private readonly Func<GameEngine, GameState, T, ConditionResponse> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleGameEventCondition{T}"/> class.
    /// </summary>
    /// <param name="handler">Delegate producing a condition response.</param>
    public SimpleGameEventCondition(Func<GameEngine, GameState, T, ConditionResponse> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _handler = handler;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, T @event)
    {
        return _handler(engine, state, @event);
    }
}