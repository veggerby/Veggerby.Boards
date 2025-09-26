using System;


using Veggerby.Boards.Builder.Rules;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Builder.Phases;

/// <summary>
/// Entry point to append rule definitions once phase conditions are specified.
/// </summary>
public interface IThenGameEventRule
{
    /// <summary>
    /// Transition to event rule definition configuration.
    /// </summary>
    IGameEventRuleDefinitionsWithOption Then();
}

/// <summary>
/// Fluent AND combination of game phase state conditions.
/// </summary>
public interface IGamePhaseConditionDefinitionAnd : IThenGameEventRule
{
    /// <summary>
    /// Adds another condition instance produced by the supplied factory.
    /// </summary>
    IGamePhaseConditionDefinitionAnd And(GameStateConditionFactory factory);
    /// <summary>
    /// Adds another condition by type construction.
    /// </summary>
    IGamePhaseConditionDefinitionAnd And<T>() where T : IGameStateCondition, new();
    /// <summary>
    /// Adds a grouped OR section configured inside the action.
    /// </summary>
    IGamePhaseConditionDefinitionAnd And(Action<IGamePhaseConditionDefinitionOr> action);
}

/// <summary>
/// Fluent OR combination of game phase state conditions.
/// </summary>
public interface IGamePhaseConditionDefinitionOr : IThenGameEventRule
{
    /// <summary>
    /// Adds an alternative condition instance produced by the supplied factory.
    /// </summary>
    IGamePhaseConditionDefinitionOr Or(GameStateConditionFactory factory);
    /// <summary>
    /// Adds an alternative condition by type construction.
    /// </summary>
    IGamePhaseConditionDefinitionOr Or<T>() where T : IGameStateCondition, new();
    /// <summary>
    /// Adds a grouped AND section configured inside the action.
    /// </summary>
    IGamePhaseConditionDefinitionOr Or(Action<IGamePhaseConditionDefinitionAnd> action);
}

/// <summary>
/// Combines AND/OR fluent interfaces for phase condition building.
/// </summary>
public interface IGamePhaseConditionDefinition : IThenGameEventRule, IGamePhaseConditionDefinitionAnd, IGamePhaseConditionDefinitionOr
{
}

/// <summary>
/// Root interface for defining a game phase in the builder.
/// </summary>
public interface IGamePhaseDefinition
{
    /// <summary>
    /// Adds an initial condition using a factory.
    /// </summary>
    IGamePhaseConditionDefinition If(GameStateConditionFactory factory);
    /// <summary>
    /// Adds an initial condition by constructing the type.
    /// </summary>
    IGamePhaseConditionDefinition If<T>() where T : IGameStateCondition, new();

    /// <summary>
    /// Assigns an exclusivity group identifier to this phase. Phases sharing the same non-null group are mutually exclusive
    /// candidates enabling runtime masking (skipping later phases once one applies) when the corresponding feature flag is enabled.
    /// </summary>
    /// <param name="group">Group identifier (non-empty).</param>
    /// <returns>The same phase definition for fluent chaining.</returns>
    IGamePhaseDefinition Exclusive(string group);
}