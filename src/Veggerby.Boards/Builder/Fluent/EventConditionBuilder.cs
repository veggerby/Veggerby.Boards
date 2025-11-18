using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder.Rules;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;

namespace Veggerby.Boards.Builder.Fluent;

/// <summary>
/// Internal implementation of <see cref="IEventConditionBuilder{TEvent}"/> that chains
/// conditions and terminates with mutator configuration.
/// </summary>
internal sealed class EventConditionBuilder<TEvent> : IEventConditionBuilder<TEvent> where TEvent : IGameEvent
{
    private readonly GameBuilder _gameBuilder;
    private readonly IGameEventConditionDefinitionAndOr<TEvent> _conditionDefinition;
    private readonly Game? _game;

    public EventConditionBuilder(GameBuilder gameBuilder, IGameEventConditionDefinitionAndOr<TEvent> conditionDefinition, Game? game = null)
    {
        _gameBuilder = gameBuilder ?? throw new ArgumentNullException(nameof(gameBuilder));
        _conditionDefinition = conditionDefinition ?? throw new ArgumentNullException(nameof(conditionDefinition));
        _game = game;
    }

    public IEventConditionBuilder<TEvent> And<TCondition>() where TCondition : IGameEventCondition<TEvent>, new()
    {
        var updated = _conditionDefinition.And<TCondition>();
        // Return the same instance since the chain continues
        return this;
    }

    public IEventConditionBuilder<TEvent> And(GameEventConditionFactory<TEvent> factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        var updated = _conditionDefinition.And(factory);
        // Return the same instance since the chain continues
        return this;
    }

    public IEventConditionBuilder<TEvent> Or<TCondition>() where TCondition : IGameEventCondition<TEvent>, new()
    {
        var updated = _conditionDefinition.Or<TCondition>();
        // Return the same instance since the chain continues
        return this;
    }

    public IEventConditionBuilder<TEvent> Or(GameEventConditionFactory<TEvent> factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        var updated = _conditionDefinition.Or(factory);
        // Return the same instance since the chain continues
        return this;
    }

    public IEventConditionBuilder<TEvent> With(ConditionGroup<TEvent> group)
    {
        ArgumentNullException.ThrowIfNull(group, nameof(group));

        // Apply all conditions from the group with AND logic
        var conditions = group.GetConditions().ToArray();
        if (conditions.Length == 0)
        {
            return this;
        }

        IEventConditionBuilder<TEvent> builder = this;
        foreach (var condition in conditions)
        {
            builder = builder.And(condition);
        }

        return builder;
    }

    public void Execute(Action<IMutatorBuilder<TEvent>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        // Transition to mutator definition using Then()
        var mutatorDef = _conditionDefinition.Then();

        // Create the scoped mutator builder and let the lambda configure it
        var mutatorBuilder = new MutatorBuilder<TEvent>(_gameBuilder, mutatorDef, _game);
        configure(mutatorBuilder);
    }
}
