using System;


using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules;

/// <summary>
/// Base class for strongly typed game event rules with optional pre- and post- state mutation steps.
/// </summary>
/// <typeparam name="T">The concrete event type handled.</typeparam>
/// <remarks>
/// Implements <see cref="IGameEventRule"/> by performing type discrimination and delegating to the strongly typed
/// <see cref="Check(GameEngine, GameState, T)"/> and <see cref="HandleEvent(GameEngine, GameState, T)"/> logic. Subclasses focus on
/// validation logic via the abstract <see cref="Check(GameEngine, GameState, T)"/> while mutation hooks are provided through
/// injected <see cref="IStateMutator{T}"/> instances.
/// </remarks>
public abstract class GameEventRule<T>(IStateMutator<T> onBeforeEvent, IStateMutator<T> onAfterEvent) : IGameEventRule where T : IGameEvent
{
    /// <summary>
    /// A permissive no-op rule that always returns <see cref="ConditionResponse.Valid"/> and performs no mutation.
    /// </summary>
    public static IGameEventRule Null = SimpleGameEventRule<T>.New(new SimpleGameEventCondition<T>((engine, state, @event) => ConditionResponse.Valid));

    private readonly IStateMutator<T> _onBeforeEvent = onBeforeEvent;
    private readonly IStateMutator<T> _onAfterEvent = onAfterEvent;

    /// <summary>
    /// Performs the validity check for the concrete event type.
    /// </summary>
    protected abstract ConditionResponse Check(GameEngine engine, GameState gameState, T @event);

    private static GameState MutateState(IStateMutator<T> eventMutator, GameEngine engine, GameState gameState, T @event)
    {
        return eventMutator is not null ? eventMutator.MutateState(engine, gameState, @event) : gameState;
    }

    /// <summary>
    /// Handles the event by applying pre-mutation, validation and post-mutation.
    /// </summary>
    protected GameState HandleEvent(GameEngine engine, GameState gameState, T @event)
    {
        var newState = MutateState(_onBeforeEvent, engine, gameState, @event);

        var check = Check(engine, newState, @event);

        if (check.Result == ConditionResult.Valid)
        {
            return MutateState(_onAfterEvent, engine, newState, @event);
        }
        else if (check.Result == ConditionResult.Ignore)
        {
            return gameState; // ignored – return original state
        }

        throw new BoardException("Invalid game event");
    }

    ConditionResponse IGameEventRule.Check(GameEngine engine, GameState gameState, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (@event is T)
        {
            return Check(engine, gameState, (T)@event);
        }

        return ConditionResponse.NotApplicable;
    }

    GameState IGameEventRule.HandleEvent(GameEngine engine, GameState gameState, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (@event is T)
        {
            return HandleEvent(engine, gameState, (T)@event);
        }

        return gameState;
    }
}