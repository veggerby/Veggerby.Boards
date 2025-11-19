using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder.Fluent;
using Veggerby.Boards.Builder.Rules;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
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
    /// <remarks>
    /// <para>
    /// This is the traditional API. For new code, consider using <see cref="DefineRules"/> which
    /// provides better visual hierarchy through lambda scoping and enables easier extraction
    /// of rule groups into helper methods.
    /// </para>
    /// <para>
    /// This method will be deprecated in a future version in favor of DefineRules().
    /// </para>
    /// </remarks>
    IGameEventRuleDefinitionsWithOption Then();

    /// <summary>
    /// Configures event handlers and rules using a scoped lambda-based fluent API.
    /// </summary>
    /// <param name="configure">Lambda that configures event handlers within a scoped builder.</param>
    /// <returns>The phase definition for further configuration.</returns>
    /// <remarks>
    /// This new API provides better visual hierarchy through lambda scoping and enables
    /// easier extraction of rule groups into helper methods. Prefer this over Then() for new code.
    /// </remarks>
    IGamePhaseDefinition DefineRules(Action<IPhaseRuleBuilder> configure);
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

    /// <summary>
    /// Configures automatic endgame detection for this phase.
    /// When the condition evaluates to valid after an event is processed, the mutator adds terminal states.
    /// </summary>
    /// <param name="conditionFactory">Factory producing the endgame condition.</param>
    /// <param name="mutatorFactory">Factory producing the mutator that adds terminal states.</param>
    /// <returns>This phase definition for fluent chaining.</returns>
    IGamePhaseDefinition WithEndGameDetection(GameStateConditionFactory conditionFactory, Func<Game, IStateMutator<IGameEvent>> mutatorFactory);
}