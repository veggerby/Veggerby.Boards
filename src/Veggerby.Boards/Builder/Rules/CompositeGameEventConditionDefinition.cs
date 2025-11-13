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

    internal override IGameEventCondition<T> Build(Game game)
    {
        if (_childDefinitions.Count == 0)
        {
            IGameEventCondition<T> nullCond = new NullGameEventCondition<T>();
            return nullCond;
        }
        if (_childDefinitions.Count == 1)
        {
            var built = _childDefinitions[0].Build(game);
            return built ?? new NullGameEventCondition<T>();
        }

        var conditions = new IGameEventCondition<T>[_childDefinitions.Count];
        for (int i = 0; i < _childDefinitions.Count; i++)
        {
            conditions[i] = _childDefinitions[i].Build(game);
        }
        return CompositeGameEventCondition<T>.CreateCompositeCondition(_conditionCompositeMode.GetValueOrDefault(CompositeMode.Any), conditions);
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