using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder.Phases;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules;

namespace Veggerby.Boards.Builder.Rules;

internal class GameEventRuleDefinitions(GameBuilder builder, GamePhaseDefinition parent) : DefinitionBase(builder), IGameEventRuleDefinitionsWithOption
{
    private readonly GamePhaseDefinition _parent = parent;
    private readonly IList<IGameEventRuleDefinition> _ruleDefinitions = [];
    private CompositeMode _eventRuleCompositeMode = CompositeMode.Any;

    private void SetCompositeMode(CompositeMode mode)
    {
        if (_ruleDefinitions.Any())
        {
            throw new ArgumentException("Can only set composite mode when no rules are added");
        }

        _eventRuleCompositeMode = mode;
    }

    public IGameEventRule Build(Game game)
    {
        if (!(_ruleDefinitions?.Any() ?? false))
        {
            return GameEventRule<IGameEvent>.Null;
        }

        if (_ruleDefinitions.Count() == 1)
        {
            return _ruleDefinitions.Single().Build(game) ?? GameEventRule<IGameEvent>.Null;
        }

        var rules = _ruleDefinitions.Select(x => x.Build(game)).ToArray();
        return CompositeGameEventRule.CreateCompositeRule(
            _eventRuleCompositeMode,
            rules);
    }

    IGameEventRuleDefinitions IGameEventRuleDefinitionsWithOption.All()
    {
        SetCompositeMode(CompositeMode.All);
        return this;
    }

    IGameEventRuleDefinitions IGameEventRuleDefinitionsWithOption.First()
    {
        SetCompositeMode(CompositeMode.Any);
        return this;
    }

    IGameEventRuleDefinition<T> IGameEventRuleDefinitions.ForEvent<T>()
    {
        var rule = new GameEventRuleDefinition<T>(Builder, this);
        _ruleDefinitions.Add(rule);
        return rule;
    }

    IGameEventRuleDefinitionsWithOption IGameEventRuleDefinitionsWithOption.PreProcessEvent(GameEventPreProcessorFactory factory)
    {
        var preprocessor = new GameEventPreProcessorDefinition(Builder, this, factory);
        _parent.Add(preprocessor);
        return this;
    }
}