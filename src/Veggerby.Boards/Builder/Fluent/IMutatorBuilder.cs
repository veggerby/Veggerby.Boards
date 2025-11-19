using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;

namespace Veggerby.Boards.Builder.Fluent;

/// <summary>
/// Scoped builder for configuring state mutators within an event handler.
/// </summary>
/// <typeparam name="TEvent">The event type being configured.</typeparam>
public interface IMutatorBuilder<TEvent> where TEvent : IGameEvent
{
    /// <summary>
    /// Adds a state mutator by type construction.
    /// </summary>
    /// <typeparam name="TMutator">The mutator type to instantiate.</typeparam>
    /// <returns>This builder for chaining.</returns>
    IMutatorBuilder<TEvent> Apply<TMutator>() where TMutator : IStateMutator<TEvent>, new();

    /// <summary>
    /// Adds a state mutator created by a factory function.
    /// </summary>
    /// <param name="factory">Factory function that creates the mutator instance.</param>
    /// <returns>This builder for chaining.</returns>
    IMutatorBuilder<TEvent> Apply(StateMutatorFactory<TEvent> factory);

    /// <summary>
    /// Conditionally applies mutators based on a runtime condition.
    /// </summary>
    /// <param name="condition">Boolean condition determining if mutators should be applied.</param>
    /// <param name="configure">Lambda that configures mutators when condition is true.</param>
    /// <returns>This builder for chaining.</returns>
    IMutatorBuilder<TEvent> ApplyIf(bool condition, Action<IMutatorBuilder<TEvent>> configure);
}
