using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Builder.Phases;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.Flows.Rules.Conditions;

namespace Veggerby.Boards.Core.Builder.Rules
{
    public interface IGameEventRuleDefinition
    {
        IGameEventRule Build(Game game);
    }

    public interface IThenStateMutator<T> where T : IGameEvent
    {
        IGameEventRuleStateMutatorDefinition<T> Then();
    }

    public interface IGameEventConditionDefinitionAnd<T> : IThenStateMutator<T> where T : IGameEvent
    {
        IGameEventConditionDefinitionAnd<T> And(GameEventConditionFactory<T> factory);
        IGameEventConditionDefinitionAnd<T> And<TCondition>() where TCondition : IGameEventCondition<T>, new();
        IGameEventConditionDefinitionAnd<T> And(Action<IGameEventConditionDefinitionOr<T>> action);
    }

    public interface IGameEventConditionDefinitionOr<T> : IThenStateMutator<T> where T : IGameEvent
    {
        IGameEventConditionDefinitionOr<T> Or(GameEventConditionFactory<T> factory);
        IGameEventConditionDefinitionOr<T> Or<TCondition>() where TCondition : IGameEventCondition<T>, new();
        IGameEventConditionDefinitionOr<T> Or(Action<IGameEventConditionDefinitionAnd<T>> action);
    }

    public interface IGameEventConditionDefinitionAndOr<T> : IGameEventConditionDefinitionAnd<T>, IGameEventConditionDefinitionOr<T> where T : IGameEvent
    {
    }

    public interface IGameEventRuleDefinition<T> : IThenStateMutator<T> where T : IGameEvent
    {
        IGameEventConditionDefinitionAndOr<T> If(GameEventConditionFactory<T> factory);
        IGameEventConditionDefinitionAndOr<T> If<TCondition>() where TCondition : IGameEventCondition<T>, new();
    }

    public interface IGameEventRuleStateMutatorDefinition<T> : IForGameEventRule where T : IGameEvent
    {
        IGameEventRuleStateMutatorDefinition<T> Do(StateMutatorFactory<T> mutator);
        IGameEventRuleStateMutatorDefinition<T> Do<TMutator>() where TMutator : IStateMutator<T>, new();
    }
}