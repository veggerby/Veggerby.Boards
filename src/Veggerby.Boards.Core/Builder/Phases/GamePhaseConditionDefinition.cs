using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Builder.Rules;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Builder.Phases
{
    public class GamePhaseConditionDefinition : DefinitionBase
    {
        public GamePhaseConditionDefinition(GameEngineBuilder builder, GamePhaseDefinition gamePhaseDefinition) : base(builder)
        {
            GamePhaseDefinition = gamePhaseDefinition;
            ConditionCompositeMode = null;
        }

        public GamePhaseDefinition GamePhaseDefinition { get; }
        public IEnumerable<GameStateConditionFactory> ConditionFactories { get; private set; }
        public CompositeMode? ConditionCompositeMode { get; internal set; }

        private void AddConditionFactory(params GameStateConditionFactory[] factories)
        {
            ConditionFactories = (ConditionFactories ?? Enumerable.Empty<GameStateConditionFactory>()).Concat(factories).ToList();
        }

        public GamePhaseConditionDefinition If(GameStateConditionFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            ConditionCompositeMode = null;
            ConditionFactories = new [] { factory };
            return this;
        }

        public GamePhaseConditionDefinition If<T>() where T : IGameStateCondition, new()
        {
            ConditionCompositeMode = null;
            ConditionFactories = new GameStateConditionFactory[] { x => new T() };
            return this;
        }


        public GamePhaseConditionDefinition And(GameStateConditionFactory factory)
        {
            if ((ConditionCompositeMode ?? CompositeMode.All) != CompositeMode.All)
            {
                throw new ArgumentException("Incorrect composition mode", nameof(factory));
            }

            ConditionCompositeMode = CompositeMode.All;
            AddConditionFactory(factory);

            return this;
        }

        public GamePhaseConditionDefinition And<T>() where T : IGameStateCondition, new()
        {
            if ((ConditionCompositeMode ?? CompositeMode.All) != CompositeMode.All)
            {
                throw new ArgumentException("Incorrect composition mode");
            }

            ConditionCompositeMode = CompositeMode.All;
            AddConditionFactory(x => new T());

            return this;
        }

        public GamePhaseConditionDefinition Or(GameStateConditionFactory factory)
        {
            if ((ConditionCompositeMode ?? CompositeMode.Any) != CompositeMode.Any)
            {
                throw new ArgumentException("Incorrect composition mode", nameof(factory));
            }

            ConditionCompositeMode = CompositeMode.Any;
            AddConditionFactory(factory);

            return this;
        }


        public GamePhaseConditionDefinition Or<T>() where T : IGameStateCondition, new()
        {
            if ((ConditionCompositeMode ?? CompositeMode.Any) != CompositeMode.Any)
            {
                throw new ArgumentException("Incorrect composition mode");
            }

            ConditionCompositeMode = CompositeMode.Any;
            AddConditionFactory(x => new T());

            return this;
        }

        public GameEventRuleDefinition<T> ForEvent<T>() where T : IGameEvent
        {
            return GamePhaseDefinition.ForEvent<T>();
        }
    }
}
