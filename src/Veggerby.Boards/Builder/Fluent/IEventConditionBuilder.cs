using System;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;

namespace Veggerby.Boards.Builder.Fluent;

/// <summary>
/// Builder for chaining event conditions with AND/OR logic before executing mutators.
/// </summary>
/// <typeparam name="TEvent">The event type being configured.</typeparam>
public interface IEventConditionBuilder<TEvent> where TEvent : IGameEvent
{
    /// <summary>
    /// Adds an additional condition that must also be valid (AND logic).
    /// </summary>
    /// <typeparam name="TCondition">The condition type to instantiate.</typeparam>
    /// <returns>This builder for chaining.</returns>
    IEventConditionBuilder<TEvent> And<TCondition>() where TCondition : IGameEventCondition<TEvent>, new();

    /// <summary>
    /// Adds an additional condition created by a factory (AND logic).
    /// </summary>
    /// <param name="factory">Factory function that creates the condition instance.</param>
    /// <returns>This builder for chaining.</returns>
    IEventConditionBuilder<TEvent> And(GameEventConditionFactory<TEvent> factory);

    /// <summary>
    /// Adds an alternative condition (OR logic).
    /// </summary>
    /// <typeparam name="TCondition">The condition type to instantiate.</typeparam>
    /// <returns>This builder for chaining.</returns>
    IEventConditionBuilder<TEvent> Or<TCondition>() where TCondition : IGameEventCondition<TEvent>, new();

    /// <summary>
    /// Adds an alternative condition created by a factory (OR logic).
    /// </summary>
    /// <param name="factory">Factory function that creates the condition instance.</param>
    /// <returns>This builder for chaining.</returns>
    IEventConditionBuilder<TEvent> Or(GameEventConditionFactory<TEvent> factory);

    /// <summary>
    /// Applies a predefined condition group with AND logic.
    /// </summary>
    /// <param name="group">The condition group to apply.</param>
    /// <returns>This builder for chaining.</returns>
    IEventConditionBuilder<TEvent> With(ConditionGroup<TEvent> group);

    /// <summary>
    /// Terminates the condition chain and configures mutators to execute when conditions are valid.
    /// </summary>
    /// <param name="configure">Lambda that configures the mutators to apply.</param>
    void Execute(Action<IMutatorBuilder<TEvent>> configure);
}
