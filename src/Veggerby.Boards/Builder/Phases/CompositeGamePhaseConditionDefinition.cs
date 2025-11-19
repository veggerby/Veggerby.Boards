using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder.Fluent;
using Veggerby.Boards.Builder.Rules;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Builder.Phases;

internal class CompositeGamePhaseConditionDefinition(GameBuilder builder, IThenGameEventRule parent) : GamePhaseConditionDefinitionBase(builder, parent), IGamePhaseConditionDefinition
{
    private readonly IList<GamePhaseConditionDefinitionBase> _childDefinitions = [];
    private CompositeMode? _conditionCompositeMode = null;

    internal CompositeGamePhaseConditionDefinition Add(GamePhaseConditionDefinitionBase conditionDefinition, CompositeMode? mode)
    {
        if (mode is null && _childDefinitions.Count > 0)
        {
            throw new ArgumentException(ExceptionMessages.CompositeModeRequired, nameof(mode));
        }

        if (_conditionCompositeMode is not null && _conditionCompositeMode.Value != mode)
        {
            throw new ArgumentException(ExceptionMessages.CompositeModeMismatch, nameof(mode));
        }

        _conditionCompositeMode = mode;
        _childDefinitions.Add(conditionDefinition);
        return this;
    }

    internal override IGameStateCondition Build(Game game)
    {
        if (_childDefinitions.Count == 0)
        {
            return new NullGameStateCondition();
        }
        if (_childDefinitions.Count == 1)
        {
            return _childDefinitions[0].Build(game) ?? new NullGameStateCondition();
        }

        var conditions = new IGameStateCondition[_childDefinitions.Count];
        for (int i = 0; i < _childDefinitions.Count; i++)
        {
            conditions[i] = _childDefinitions[i].Build(game);
        }
        return CompositeGameStateCondition.CreateCompositeCondition(_conditionCompositeMode.GetValueOrDefault(CompositeMode.All), conditions);
    }

    IGamePhaseConditionDefinitionAnd IGamePhaseConditionDefinitionAnd.And(GameStateConditionFactory factory)
    {
        return Add(new GamePhaseConditionDefinition(Builder, factory, Parent), CompositeMode.All);
    }

    IGamePhaseConditionDefinitionAnd IGamePhaseConditionDefinitionAnd.And<T>()
    {
        return Add(new GamePhaseConditionDefinition(Builder, game => new T(), Parent), CompositeMode.All);
    }

    IGamePhaseConditionDefinitionAnd IGamePhaseConditionDefinitionAnd.And(Action<IGamePhaseConditionDefinitionOr> action)
    {
        var composite = new CompositeGamePhaseConditionDefinition(Builder, Parent);
        action(composite);
        Add(composite, CompositeMode.All);
        return this;
    }

    IGamePhaseConditionDefinitionOr IGamePhaseConditionDefinitionOr.Or(GameStateConditionFactory factory)
    {
        return Add(new GamePhaseConditionDefinition(Builder, factory, Parent), CompositeMode.Any);
    }

    IGamePhaseConditionDefinitionOr IGamePhaseConditionDefinitionOr.Or<T>()
    {
        return Add(new GamePhaseConditionDefinition(Builder, game => new T(), Parent), CompositeMode.Any);
    }

    IGamePhaseConditionDefinitionOr IGamePhaseConditionDefinitionOr.Or(Action<IGamePhaseConditionDefinitionAnd> action)
    {
        var composite = new CompositeGamePhaseConditionDefinition(Builder, this);
        action(composite);
        Add(composite, CompositeMode.Any);
        return this;
    }

    IGameEventRuleDefinitionsWithOption IThenGameEventRule.Then()
    {
        return Parent.Then();
    }

    IGamePhaseDefinition IThenGameEventRule.DefineRules(Action<IPhaseRuleBuilder> configure)
    {
        return Parent.DefineRules(configure);
    }
}