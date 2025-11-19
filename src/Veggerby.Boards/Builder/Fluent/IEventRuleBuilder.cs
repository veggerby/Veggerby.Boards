using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;

namespace Veggerby.Boards.Builder.Fluent;

/// <summary>
/// Scoped builder for configuring event conditions within an event handler.
/// </summary>
/// <typeparam name="TEvent">The event type being configured.</typeparam>
public interface IEventRuleBuilder<TEvent> where TEvent : IGameEvent
{
    /// <summary>
    /// Starts a condition chain with a factory-created condition.
    /// </summary>
    /// <param name="factory">Factory function that creates the condition instance.</param>
    /// <returns>Condition builder for chaining AND/OR conditions.</returns>
    IEventConditionBuilder<TEvent> When(GameEventConditionFactory<TEvent> factory);

    /// <summary>
    /// Starts a condition chain with a type-constructed condition.
    /// </summary>
    /// <typeparam name="TCondition">The condition type to instantiate.</typeparam>
    /// <returns>Condition builder for chaining AND/OR conditions.</returns>
    IEventConditionBuilder<TEvent> When<TCondition>() where TCondition : IGameEventCondition<TEvent>, new();

    /// <summary>
    /// Applies a predefined condition group.
    /// </summary>
    /// <param name="group">The condition group to apply.</param>
    /// <returns>Condition builder for chaining additional conditions.</returns>
    IEventConditionBuilder<TEvent> With(ConditionGroup<TEvent> group);
}
