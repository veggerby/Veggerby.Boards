using System;


using Veggerby.Boards.Core.Builder.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Builder.Phases;

public interface IThenGameEventRule
{
    IGameEventRuleDefinitionsWithOption Then();
}

public interface IGamePhaseConditionDefinitionAnd : IThenGameEventRule
{
    IGamePhaseConditionDefinitionAnd And(GameStateConditionFactory factory);
    IGamePhaseConditionDefinitionAnd And<T>() where T : IGameStateCondition, new();
    IGamePhaseConditionDefinitionAnd And(Action<IGamePhaseConditionDefinitionOr> action);
}

public interface IGamePhaseConditionDefinitionOr : IThenGameEventRule
{
    IGamePhaseConditionDefinitionOr Or(GameStateConditionFactory factory);
    IGamePhaseConditionDefinitionOr Or<T>() where T : IGameStateCondition, new();
    IGamePhaseConditionDefinitionOr Or(Action<IGamePhaseConditionDefinitionAnd> action);
}

public interface IGamePhaseConditionDefinition : IThenGameEventRule, IGamePhaseConditionDefinitionAnd, IGamePhaseConditionDefinitionOr
{
}

public interface IGamePhaseDefinition
{
    IGamePhaseConditionDefinition If(GameStateConditionFactory factory);
    IGamePhaseConditionDefinition If<T>() where T : IGameStateCondition, new();
}