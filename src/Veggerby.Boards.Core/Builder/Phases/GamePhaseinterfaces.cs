using System;
using Veggerby.Boards.Core.Builder.Rules;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Builder.Phases
{
    public interface IForGameEventRule
    {
        IGameEventRuleDefinition<T> ForEvent<T>() where T : IGameEvent;
    }

    public interface IGamePhaseConditionDefinitionAnd : IForGameEventRule
    {
        IGamePhaseConditionDefinitionAnd And(GameStateConditionFactory factory);
        IGamePhaseConditionDefinitionAnd And<T>() where T : IGameStateCondition, new();
        IGamePhaseConditionDefinitionAnd And(Action<IGamePhaseConditionDefinitionOr> action);
    }

    public interface IGamePhaseConditionDefinitionOr : IForGameEventRule
    {
        IGamePhaseConditionDefinitionOr Or(GameStateConditionFactory factory);
        IGamePhaseConditionDefinitionOr Or<T>() where T : IGameStateCondition, new();
        IGamePhaseConditionDefinitionOr Or(Action<IGamePhaseConditionDefinitionAnd> action);
    }

    public interface IGamePhaseConditionDefinition : IForGameEventRule, IGamePhaseConditionDefinitionAnd, IGamePhaseConditionDefinitionOr
    {
    }

    public interface IGamePhaseDefinition : IForGameEventRule
    {
        IGamePhaseConditionDefinition If(GameStateConditionFactory factory);
        IGamePhaseConditionDefinition If<T>() where T : IGameStateCondition, new();
    }
}