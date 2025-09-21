using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Builder.Rules;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;

namespace Veggerby.Boards.Core.Builder.Phases;

internal class CompositeGamePhaseConditionDefinition(GameBuilder builder, IThenGameEventRule parent) : GamePhaseConditionDefinitionBase(builder, parent), IGamePhaseConditionDefinition
{
    private readonly IList<GamePhaseConditionDefinitionBase> _childDefinitions = [];
    private CompositeMode? _conditionCompositeMode = null;

    internal CompositeGamePhaseConditionDefinition Add(GamePhaseConditionDefinitionBase conditionDefinition, CompositeMode? mode)
    {
        if (mode is null && _childDefinitions.Any())
        {
            throw new ArgumentException("Must provide mode for multiple conditions", nameof(mode));
        }

        if (_conditionCompositeMode is not null && _conditionCompositeMode.Value != mode)
        {
            throw new ArgumentException("Must be same composite mode", nameof(mode));
        }

        _conditionCompositeMode = mode;
        _childDefinitions.Add(conditionDefinition);
        return this;
    }

    internal override IGameStateCondition Build(Game game)
    {
        if (!(_childDefinitions.Any()))
        {
            return null;
        }

        if (_childDefinitions.Count() == 1)
        {
            return _childDefinitions.Single().Build(game);
        }

        var conditions = _childDefinitions.Select(definition => definition.Build(game)).ToArray();
        return CompositeGameStateCondition.CreateCompositeCondition(_conditionCompositeMode.Value, conditions);
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
}