using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder.Phases;
using Veggerby.Boards.Builder.Rules;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Builder.Fluent;

/// <summary>
/// Internal implementation of <see cref="IPhaseRuleBuilder"/> that translates lambda-based
/// fluent API calls into the traditional builder structure.
/// </summary>
internal sealed class PhaseRuleBuilder : IPhaseRuleBuilder
{
    private readonly GameBuilder _gameBuilder;
    private readonly GamePhaseDefinition _phaseDefinition;
    private readonly IGameEventRuleDefinitions _ruleDefinitions;
    private readonly Game? _game;

    public PhaseRuleBuilder(GameBuilder gameBuilder, GamePhaseDefinition phaseDefinition, IGameEventRuleDefinitions ruleDefinitions, Game? game = null)
    {
        _gameBuilder = gameBuilder ?? throw new ArgumentNullException(nameof(gameBuilder));
        _phaseDefinition = phaseDefinition ?? throw new ArgumentNullException(nameof(phaseDefinition));
        _ruleDefinitions = ruleDefinitions ?? throw new ArgumentNullException(nameof(ruleDefinitions));
        _game = game;
    }

    public IPhaseRuleBuilder On<TEvent>(Action<IEventRuleBuilder<TEvent>> configure) where TEvent : IGameEvent
    {
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        // Start a new event rule definition using the traditional API
        var eventRuleDef = _ruleDefinitions.ForEvent<TEvent>();

        // Create the scoped event rule builder and let the lambda configure it
        var eventBuilder = new EventRuleBuilder<TEvent>(_gameBuilder, eventRuleDef, _game);
        configure(eventBuilder);

        return this;
    }

    public IPhaseRuleBuilder When(bool condition, Action<IPhaseRuleBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        if (condition)
        {
            configure(this);
        }

        return this;
    }
}
