using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Builder.Rules;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;

namespace Veggerby.Boards.Core.Builder.Phases
{
    public class GamePhaseDefinition : DefinitionBase
    {
        public GamePhaseDefinition(GameEngineBuilder builder) : base(builder)
        {
            RuleDefinitions = new GameEventRuleDefinitions(Builder, this);
        }

        public int? Number { get; private set; }
        public GamePhaseConditionDefinition ConditionDefinition { get; private set; }
        public GameEventRuleDefinitions RuleDefinitions { get; }

        public GamePhaseDefinition WithNumber(int number)
        {
            Number = number;
            return this;
        }

        public GamePhaseConditionDefinition If(GameStateConditionFactory factory)
        {
            ConditionDefinition = new GamePhaseConditionDefinition(Builder, this).If(factory);
            return ConditionDefinition;
        }

        public GamePhaseConditionDefinition If<T>() where T : IGameStateCondition, new()
        {
            ConditionDefinition = new GamePhaseConditionDefinition(Builder, this).If<T>();
            return ConditionDefinition;
        }

        public GameEventRuleDefinition<T> ForEvent<T>() where T : IGameEvent
        {
            return RuleDefinitions.ForEvent<T>();
        }

        public GamePhase Build(int number, Game game, CompositeGamePhase parent = null)
        {
            var condition = ConditionDefinition?.Build(game);
            var rule = RuleDefinitions?.Build(game);
            return GamePhase.New(Number ?? number, condition ?? new NullGameStateCondition(), rule, parent);
        }
    }
}
