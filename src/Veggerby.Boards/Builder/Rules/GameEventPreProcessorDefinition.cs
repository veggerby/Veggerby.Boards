using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Builder.Rules;

internal class GameEventPreProcessorDefinition
{
    private readonly GameBuilder _builder;
    private readonly IGameEventRuleDefinitions _parent;
    private readonly GameEventPreProcessorFactory _factory;

    public GameEventPreProcessorDefinition(GameBuilder builder, IGameEventRuleDefinitions parent, GameEventPreProcessorFactory factory)
    {
        ArgumentNullException.ThrowIfNull(builder);

        ArgumentNullException.ThrowIfNull(parent);

        ArgumentNullException.ThrowIfNull(factory);

        _builder = builder;
        _parent = parent;
        _factory = factory;
    }

    public IGameEventPreProcessor Build(Game game)
    {
        return _factory(game);
    }
}