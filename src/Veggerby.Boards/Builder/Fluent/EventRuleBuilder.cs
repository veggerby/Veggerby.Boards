using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder.Rules;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;

namespace Veggerby.Boards.Builder.Fluent;

/// <summary>
/// Internal implementation of <see cref="IEventRuleBuilder{TEvent}"/> that creates
/// event conditions using the traditional builder API.
/// </summary>
internal sealed class EventRuleBuilder<TEvent> : IEventRuleBuilder<TEvent> where TEvent : IGameEvent
{
    private readonly GameBuilder _gameBuilder;
    private readonly IGameEventRuleDefinition<TEvent> _ruleDefinition;
    private readonly Game? _game;

    public EventRuleBuilder(GameBuilder gameBuilder, IGameEventRuleDefinition<TEvent> ruleDefinition, Game? game = null)
    {
        _gameBuilder = gameBuilder ?? throw new ArgumentNullException(nameof(gameBuilder));
        _ruleDefinition = ruleDefinition ?? throw new ArgumentNullException(nameof(ruleDefinition));
        _game = game;
    }

    public IEventConditionBuilder<TEvent> When(GameEventConditionFactory<TEvent> factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        var conditionDef = _ruleDefinition.If(factory);
        return new EventConditionBuilder<TEvent>(_gameBuilder, conditionDef, _game);
    }

    public IEventConditionBuilder<TEvent> When<TCondition>() where TCondition : IGameEventCondition<TEvent>, new()
    {
        var conditionDef = _ruleDefinition.If<TCondition>();
        return new EventConditionBuilder<TEvent>(_gameBuilder, conditionDef, _game);
    }

    public IEventConditionBuilder<TEvent> With(ConditionGroup<TEvent> group)
    {
        ArgumentNullException.ThrowIfNull(group, nameof(group));

        // Apply the first condition from the group to start the chain
        var conditions = group.GetConditions().ToArray();
        if (conditions.Length == 0)
        {
            throw new InvalidOperationException("Condition group must contain at least one condition.");
        }

        // Start with the first condition
        var conditionDef = _ruleDefinition.If(conditions[0]);
        var builder = new EventConditionBuilder<TEvent>(_gameBuilder, conditionDef, _game);

        // Chain the remaining conditions with AND logic
        for (int i = 1; i < conditions.Length; i++)
        {
            builder = (EventConditionBuilder<TEvent>)builder.And(conditions[i]);
        }

        return builder;
    }
}
