using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Builder
{
    public class GamePhaseConditionDefinition : DefinitionBase
    {
        public GamePhaseConditionDefinition(GameEngineBuilder builder, GamePhaseDefinition gamePhaseDefinitionSettings) : base(builder)
        {
            GamePhaseDefinitionSettings = gamePhaseDefinitionSettings;
            ConditionCompositeMode = null;
        }

        public GamePhaseDefinition GamePhaseDefinitionSettings { get; }
        public IEnumerable<Func<Game, IGameStateCondition>> ConditionFactories { get; private set; }
        public CompositeMode? ConditionCompositeMode { get; internal set; }

        private void AddConditionFactory(params Func<Game, IGameStateCondition>[] factories)
        {
            ConditionFactories = (ConditionFactories ?? Enumerable.Empty<Func<Game, IGameStateCondition>>()).Concat(factories).ToList();
        }

        public GamePhaseConditionDefinition If(Func<Game, IGameStateCondition> factory)
        {
            ConditionCompositeMode = null;
            AddConditionFactory(factory);
            return this;
        }

        public GamePhaseConditionDefinition And(Func<Game, IGameStateCondition> factory)
        {
            if ((ConditionCompositeMode ?? CompositeMode.All) != CompositeMode.All)
            {
                throw new ArgumentException("Incorrect composition mode", nameof(factory));
            }

            ConditionCompositeMode = CompositeMode.All;
            AddConditionFactory(factory);

            return this;
        }

        public GamePhaseConditionDefinition Or(Func<Game, IGameStateCondition> factory)
        {
            if ((ConditionCompositeMode ?? CompositeMode.Any) != CompositeMode.Any)
            {
                throw new ArgumentException("Incorrect composition mode", nameof(factory));
            }

            ConditionCompositeMode = CompositeMode.Any;
            AddConditionFactory(factory);

            return this;
        }

        public GameEventRuleDefinition Then()
        {
            return GamePhaseDefinitionSettings.Then();
        }
    }
}
