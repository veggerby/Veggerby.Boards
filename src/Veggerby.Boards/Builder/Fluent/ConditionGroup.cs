using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;

namespace Veggerby.Boards.Builder.Fluent;

/// <summary>
/// Represents a reusable group of event conditions that can be applied together.
/// </summary>
/// <typeparam name="TEvent">The event type these conditions apply to.</typeparam>
/// <remarks>
/// Condition groups enable extraction of common condition patterns into reusable components,
/// reducing duplication across event handlers.
/// </remarks>
public sealed class ConditionGroup<TEvent> where TEvent : IGameEvent
{
    private readonly List<GameEventConditionFactory<TEvent>> _conditions = new();

    private ConditionGroup()
    {
    }

    /// <summary>
    /// Creates a new empty condition group.
    /// </summary>
    /// <returns>A new condition group builder.</returns>
    public static ConditionGroup<TEvent> Create()
    {
        return new ConditionGroup<TEvent>();
    }

    /// <summary>
    /// Adds a condition to this group by type construction.
    /// </summary>
    /// <typeparam name="TCondition">The condition type to add.</typeparam>
    /// <returns>This group for fluent chaining.</returns>
    public ConditionGroup<TEvent> Require<TCondition>() where TCondition : IGameEventCondition<TEvent>, new()
    {
        _conditions.Add(game => new TCondition());
        return this;
    }

    /// <summary>
    /// Adds a condition to this group using a factory function.
    /// </summary>
    /// <param name="factory">Factory function that creates the condition instance.</param>
    /// <returns>This group for fluent chaining.</returns>
    public ConditionGroup<TEvent> Require(GameEventConditionFactory<TEvent> factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        _conditions.Add(factory);
        return this;
    }

    /// <summary>
    /// Gets all conditions in this group.
    /// </summary>
    internal IEnumerable<GameEventConditionFactory<TEvent>> GetConditions()
    {
        return _conditions.AsReadOnly();
    }
}
