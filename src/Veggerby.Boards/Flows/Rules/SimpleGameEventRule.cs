using System;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules;

/// <summary>
/// Minimal concrete implementation of <see cref="GameEventRule{T}"/> that defers validation to an injected condition.
/// </summary>
/// <typeparam name="T">The event type handled.</typeparam>
public class SimpleGameEventRule<T> : GameEventRule<T> where T : IGameEvent
{
    private readonly IGameEventCondition<T> _condition;

    private SimpleGameEventRule(IGameEventCondition<T> condition, IStateMutator<T> onBeforeEvent, IStateMutator<T> onAfterEvent)
        : base(onBeforeEvent, onAfterEvent)
    {
        ArgumentNullException.ThrowIfNull(condition);

        _condition = condition;
    }

    /// <inheritdoc />
    protected override ConditionResponse Check(GameEngine engine, GameState state, T @event)
    {
        return _condition.Evaluate(engine, state, @event);
    }

    /// <summary>
    /// Creates a new simple rule instance.
    /// </summary>
    /// <param name="condition">Validation condition executed during <see cref="GameEventRule{T}.Check"/>.</param>
    /// <param name="onBeforeEvent">Optional pre-mutation mutator.</param>
    /// <param name="onAfterEvent">Optional post-mutation mutator (defaults to <see cref="NullStateMutator{T}"/>).</param>
    /// <returns>The rule instance.</returns>
    public static IGameEventRule New(IGameEventCondition<T> condition, IStateMutator<T>? onBeforeEvent = null, IStateMutator<T>? onAfterEvent = null)
    {
        onBeforeEvent ??= NullStateMutator<T>.Instance;
        onAfterEvent ??= NullStateMutator<T>.Instance;
        return new SimpleGameEventRule<T>(condition, onBeforeEvent, onAfterEvent);
    }
}