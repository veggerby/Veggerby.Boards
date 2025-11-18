using System;

using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Builder.Fluent;

/// <summary>
/// Scoped builder for configuring event handlers within a game phase using lambda-based fluent API.
/// </summary>
/// <remarks>
/// This interface provides improved ergonomics over the traditional fluent API by introducing
/// lambda scopes that create clear visual hierarchy and enable helper method extraction.
/// </remarks>
public interface IPhaseRuleBuilder
{
    /// <summary>
    /// Registers an event handler for a specific event type with scoped configuration.
    /// </summary>
    /// <typeparam name="TEvent">The event type to handle.</typeparam>
    /// <param name="configure">Lambda that configures conditions and mutators for this event.</param>
    /// <returns>This builder for fluent chaining.</returns>
    IPhaseRuleBuilder On<TEvent>(Action<IEventRuleBuilder<TEvent>> configure) where TEvent : IGameEvent;

    /// <summary>
    /// Conditionally includes rules based on a runtime condition.
    /// </summary>
    /// <param name="condition">Boolean condition determining if rules should be added.</param>
    /// <param name="configure">Lambda that configures rules when condition is true.</param>
    /// <returns>This builder for fluent chaining.</returns>
    IPhaseRuleBuilder When(bool condition, Action<IPhaseRuleBuilder> configure);
}
