using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;

namespace Veggerby.Boards.Core.Builder.Rules
{
    internal class GameEventPreProcessorDefinition
    {
        private readonly GameBuilder _builder;
        private readonly IGameEventRuleDefinitions _parent;
        private readonly GameEventPreProcessorFactory _factory;

        public GameEventPreProcessorDefinition(GameBuilder builder, IGameEventRuleDefinitions parent, GameEventPreProcessorFactory factory)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _builder = builder;
            _parent = parent;
            _factory = factory;
        }

        public IGameEventPreProcessor Build(Game game)
        {
            return _factory(game);
        }
    }
}