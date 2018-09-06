using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Flows.Events;

namespace Veggerby.Boards.Core.Builder
{
    public class GameEventRuleDefinitions : DefinitionBase
    {
        public GameEventRuleDefinitions(GameEngineBuilder builder, GamePhaseDefinition gamePhaseDefinitionSettings) : base(builder)
        {
            GamePhaseDefinitionSettings = gamePhaseDefinitionSettings;
        }

        public GamePhaseDefinition GamePhaseDefinitionSettings { get; }

        public IEnumerable<IGameEventRuleDefinition> Definitions { get; private set; }

        public CompositeMode? EventRuleCompositeMode { get; private set; }

        public GameEventRuleDefinition<T> ForEvent<T>() where T : IGameEvent
        {
            EventRuleCompositeMode = null;
            var rule = new GameEventRuleDefinition<T>(Builder, this);
            Definitions = new [] { rule };
            return rule;
        }

        public GameEventRuleDefinition<T> AndEvent<T>() where T : IGameEvent
        {
            if ((EventRuleCompositeMode ?? CompositeMode.All) != CompositeMode.All)
            {
                throw new ArgumentException("Incorrect composition mode");
            }

            EventRuleCompositeMode = CompositeMode.All;
            var rule = new GameEventRuleDefinition<T>(Builder, this);
            Definitions = (Definitions ?? Enumerable.Empty<IGameEventRuleDefinition>()).Concat(new [] { rule });
            return rule;
        }

        public GameEventRuleDefinition<T> OrEvent<T>() where T : IGameEvent
        {
            if ((EventRuleCompositeMode ?? CompositeMode.Any) != CompositeMode.Any)
            {
                throw new ArgumentException("Incorrect composition mode");
            }

            EventRuleCompositeMode = CompositeMode.Any;
            var rule = new GameEventRuleDefinition<T>(Builder, this);
            Definitions = (Definitions ?? Enumerable.Empty<IGameEventRuleDefinition>()).Concat(new [] { rule });
            return rule;
        }
    }
}
