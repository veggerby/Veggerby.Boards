using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;

namespace Veggerby.Boards.Builder.Rules;

internal class CompositeGameEventConditionDefinition<T>(GameBuilder builder, IThenStateMutator<T> parent) : GameEventConditionDefinitionBase<T>(builder, parent), IGameEventConditionDefinitionAndOr<T> where T : IGameEvent
{
    private readonly IList<GameEventConditionDefinitionBase<T>> _childDefinitions = [];
    private CompositeMode? _conditionCompositeMode = null;

    internal CompositeGameEventConditionDefinition<T> Add(GameEventConditionDefinitionBase<T> conditionDefinition, CompositeMode? mode)
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

    internal override IGameEventCondition<T> Build(Game game)
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
        return CompositeGameEventCondition<T>.CreateCompositeCondition(_conditionCompositeMode.Value, conditions);
    }

    IGameEventConditionDefinitionAnd<T> IGameEventConditionDefinitionAnd<T>.And(GameEventConditionFactory<T> factory)
    {
        return Add(new GameEventConditionDefinition<T>(Builder, factory, Parent), CompositeMode.All);
    }

    IGameEventConditionDefinitionAnd<T> IGameEventConditionDefinitionAnd<T>.And<TCondition>()
    {
        return Add(new GameEventConditionDefinition<T>(Builder, game => new TCondition(), Parent), CompositeMode.All);
    }

    IGameEventConditionDefinitionAnd<T> IGameEventConditionDefinitionAnd<T>.And(Action<IGameEventConditionDefinitionOr<T>> action)
    {
        var composite = new CompositeGameEventConditionDefinition<T>(Builder, Parent);
        action(composite);
        Add(composite, CompositeMode.All);
        return this;
    }

    IGameEventConditionDefinitionOr<T> IGameEventConditionDefinitionOr<T>.Or(GameEventConditionFactory<T> factory)
    {
        return Add(new GameEventConditionDefinition<T>(Builder, factory, Parent), CompositeMode.Any);
    }

    IGameEventConditionDefinitionOr<T> IGameEventConditionDefinitionOr<T>.Or<TCondition>()
    {
        return Add(new GameEventConditionDefinition<T>(Builder, game => new TCondition(), Parent), CompositeMode.Any);
    }

    IGameEventConditionDefinitionOr<T> IGameEventConditionDefinitionOr<T>.Or(Action<IGameEventConditionDefinitionAnd<T>> action)
    {
        var composite = new CompositeGameEventConditionDefinition<T>(Builder, Parent);
        action(composite);
        Add(composite, CompositeMode.Any);
        return this;
    }

    IGameEventRuleStateMutatorDefinition<T> IThenStateMutator<T>.Then()
    {
        return Parent.Then();
    }
}