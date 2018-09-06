using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Builder
{
    public class GamePhaseDefinition : DefinitionBase
    {
        public GamePhaseDefinition(GameEngineBuilder builder) : base(builder)
        {
        }

        public int? Number { get; private set; }
        public GamePhaseConditionDefinition ConditionDefinition { get; private set; }
        public GameEventRuleDefinition RuleDefinition { get; private set; }

        public GamePhaseDefinition WithNumber(int number)
        {
            Number = number;
            return this;
        }

        public GamePhaseConditionDefinition If(Func<Game, IGameStateCondition> factory)
        {
            ConditionDefinition = new GamePhaseConditionDefinition(Builder, this).If(factory);
            return ConditionDefinition;
        }

        public GameEventRuleDefinition Then()
        {
            RuleDefinition = new GameEventRuleDefinition(Builder, this);
            return RuleDefinition;
        }
    }
}
