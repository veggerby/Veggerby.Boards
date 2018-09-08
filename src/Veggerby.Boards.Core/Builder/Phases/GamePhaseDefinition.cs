using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Builder.Rules;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;

namespace Veggerby.Boards.Core.Builder.Phases
{
    internal class GamePhaseDefinition : DefinitionBase, IGamePhaseDefinition
    {
        public GamePhaseDefinition(GameEngineBuilder builder) : base(builder)
        {
            _ruleDefinitions = new GameEventRuleDefinitions(Builder, this);
        }

        private int? _number;
        private CompositeGamePhaseConditionDefinition _conditionDefinition;
        private GameEventRuleDefinitions _ruleDefinitions;

        public GamePhaseDefinition WithNumber(int number)
        {
            _number = number;
            return this;
        }

        IGamePhaseConditionDefinition IGamePhaseDefinition.If(GameStateConditionFactory factory)
        {
            _conditionDefinition = new CompositeGamePhaseConditionDefinition(Builder, this).Add(new GamePhaseConditionDefinition(Builder, factory, this), null);
            return _conditionDefinition;
        }

        IGamePhaseConditionDefinition IGamePhaseDefinition.If<T>()
        {
            _conditionDefinition = new CompositeGamePhaseConditionDefinition(Builder, this).Add(new GamePhaseConditionDefinition(Builder, game => new T(), this), null);
            return _conditionDefinition;
        }

        IGameEventRuleDefinition<T> IForGameEventRule.ForEvent<T>()
        {
            return ((IForGameEventRule)_ruleDefinitions).ForEvent<T>();
        }

        internal GamePhase Build(int number, Game game, CompositeGamePhase parent = null)
        {
            var condition = _conditionDefinition?.Build(game);
            var rule = _ruleDefinitions?.Build(game);
            return GamePhase.New(_number ?? number, condition ?? new NullGameStateCondition(), rule, parent);
        }
    }
}
