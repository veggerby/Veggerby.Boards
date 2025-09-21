using System;


using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Flows.Rules.Conditions;

namespace Veggerby.Boards.Builder.Rules;

/// <summary>
/// Internal build-time rule definition to construct an executable <see cref="IGameEventRule"/>.
/// </summary>
internal interface IGameEventRuleDefinition
{
    /// <summary>
    /// Creates the executable rule instance.
    /// </summary>
    IGameEventRule Build(Game game);
}

/// <summary>
/// Root interface for rule definitions with selection options (apply all or first valid).
/// </summary>
public interface IGameEventRuleDefinitionsWithOption : IGameEventRuleDefinitions
{
    /// <summary>
    /// Adds an event pre-processor which may transform or expand events prior to evaluation.
    /// </summary>
    IGameEventRuleDefinitionsWithOption PreProcessEvent(GameEventPreProcessorFactory factory);
    /// <summary>
    /// Specifies that all configured rules should be applied.
    /// </summary>
    IGameEventRuleDefinitions All();
    /// <summary>
    /// Specifies that only the first valid rule should be applied.
    /// </summary>
    IGameEventRuleDefinitions First();
}

/// <summary>
/// Entry for declaring rules bound to concrete event types.
/// </summary>
public interface IGameEventRuleDefinitions
{
    /// <summary>
    /// Starts rule definition for a specific event type.
    /// </summary>
    IGameEventRuleDefinition<T> ForEvent<T>() where T : IGameEvent;
}

/// <summary>
/// Adds state mutators to be executed after condition configuration.
/// </summary>
public interface IThenStateMutator<T> where T : IGameEvent
{
    /// <summary>
    /// Transition to mutator configuration.
    /// </summary>
    IGameEventRuleStateMutatorDefinition<T> Then();
}

/// <summary>
/// Fluent AND chain for event conditions.
/// </summary>
public interface IGameEventConditionDefinitionAnd<T> : IThenStateMutator<T> where T : IGameEvent
{
    /// <summary>
    /// Adds an additional condition that must also evaluate valid.
    /// </summary>
    IGameEventConditionDefinitionAnd<T> And(GameEventConditionFactory<T> factory);
    /// <summary>
    /// Adds an additional condition instance created by type.
    /// </summary>
    IGameEventConditionDefinitionAnd<T> And<TCondition>() where TCondition : IGameEventCondition<T>, new();
    /// <summary>
    /// Adds a grouped OR branch configured inside the action.
    /// </summary>
    IGameEventConditionDefinitionAnd<T> And(Action<IGameEventConditionDefinitionOr<T>> action);
}

/// <summary>
/// Fluent OR chain for event conditions.
/// </summary>
public interface IGameEventConditionDefinitionOr<T> : IThenStateMutator<T> where T : IGameEvent
{
    /// <summary>
    /// Adds an alternative condition (any may be valid).
    /// </summary>
    IGameEventConditionDefinitionOr<T> Or(GameEventConditionFactory<T> factory);
    /// <summary>
    /// Adds an alternative condition instance created by type.
    /// </summary>
    IGameEventConditionDefinitionOr<T> Or<TCondition>() where TCondition : IGameEventCondition<T>, new();
    /// <summary>
    /// Adds a grouped AND branch configured inside the action.
    /// </summary>
    IGameEventConditionDefinitionOr<T> Or(Action<IGameEventConditionDefinitionAnd<T>> action);
}

/// <summary>
/// Combines AND and OR interfaces for event condition building.
/// </summary>
public interface IGameEventConditionDefinitionAndOr<T> : IGameEventConditionDefinitionAnd<T>, IGameEventConditionDefinitionOr<T> where T : IGameEvent
{
}

/// <summary>
/// Root rule definition interface for a specific event type.
/// </summary>
public interface IGameEventRuleDefinition<T> : IThenStateMutator<T> where T : IGameEvent
{
    /// <summary>
    /// Starts condition chain with a factory-created condition.
    /// </summary>
    IGameEventConditionDefinitionAndOr<T> If(GameEventConditionFactory<T> factory);
    /// <summary>
    /// Starts condition chain with a type-created condition.
    /// </summary>
    IGameEventConditionDefinitionAndOr<T> If<TCondition>() where TCondition : IGameEventCondition<T>, new();
}

/// <summary>
/// Pre-application (before) mutator configuration for a rule.
/// </summary>
public interface IGameEventRuleStateMutatorDefinitionBefore<T> : IGameEventRuleDefinitions where T : IGameEvent
{
    /// <summary>
    /// Adds a state mutator executed before the main rule mutators.
    /// </summary>
    IGameEventRuleStateMutatorDefinition<T> Before(StateMutatorFactory<T> mutator);
    /// <summary>
    /// Adds a state mutator by type executed before the main rule mutators.
    /// </summary>
    IGameEventRuleStateMutatorDefinition<T> Before<TMutator>() where TMutator : IStateMutator<T>, new();
}

/// <summary>
/// Post-application (after) mutator configuration for a rule.
/// </summary>
public interface IGameEventRuleStateMutatorDefinitionDo<T> : IGameEventRuleDefinitions where T : IGameEvent
{
    /// <summary>
    /// Adds a state mutator executed after primary mutators.
    /// </summary>
    IGameEventRuleStateMutatorDefinitionDo<T> Do(StateMutatorFactory<T> mutator);
    /// <summary>
    /// Adds a state mutator by type executed after primary mutators.
    /// </summary>
    IGameEventRuleStateMutatorDefinitionDo<T> Do<TMutator>() where TMutator : IStateMutator<T>, new();
}

/// <summary>
/// Combines before/after mutator configuration interfaces.
/// </summary>
public interface IGameEventRuleStateMutatorDefinition<T> : IGameEventRuleStateMutatorDefinitionBefore<T>, IGameEventRuleStateMutatorDefinitionDo<T> where T : IGameEvent
{
}