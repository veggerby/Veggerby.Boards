using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder;
using Veggerby.Boards.Builder.Rules;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;

namespace Veggerby.Boards.Builder.Fluent;

/// <summary>
/// Internal implementation of <see cref="IMutatorBuilder{TEvent}"/> that applies
/// state mutators to an event rule.
/// </summary>
internal sealed class MutatorBuilder<TEvent> : IMutatorBuilder<TEvent> where TEvent : IGameEvent
{
    private readonly GameBuilder _gameBuilder;
    private readonly IGameEventRuleStateMutatorDefinition<TEvent> _mutatorDefinition;
    private readonly Game? _game;

    public MutatorBuilder(GameBuilder gameBuilder, IGameEventRuleStateMutatorDefinition<TEvent> mutatorDefinition, Game? game = null)
    {
        _gameBuilder = gameBuilder ?? throw new ArgumentNullException(nameof(gameBuilder));
        _mutatorDefinition = mutatorDefinition ?? throw new ArgumentNullException(nameof(mutatorDefinition));
        _game = game;
    }

    public IMutatorBuilder<TEvent> Apply<TMutator>() where TMutator : IStateMutator<TEvent>, new()
    {
        _mutatorDefinition.Do<TMutator>();
        return this;
    }

    public IMutatorBuilder<TEvent> Apply(StateMutatorFactory<TEvent> factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        _mutatorDefinition.Do(factory);
        return this;
    }

    public IMutatorBuilder<TEvent> ApplyIf(bool condition, Action<IMutatorBuilder<TEvent>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        if (condition)
        {
            configure(this);
        }

        return this;
    }
}
