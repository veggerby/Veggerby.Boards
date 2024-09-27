using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Builder.Rules;
using Veggerby.Boards.Core.Flows.Phases;

namespace Veggerby.Boards.Core.Builder.Phases
{
    internal class GamePhaseDefinition : DefinitionBase, IGamePhaseDefinition, IThenGameEventRule
    {
        public GamePhaseDefinition(GameBuilder builder, string label) : base(builder)
        {
            _label = label;
        }

        private int? _number;
        private CompositeGamePhaseConditionDefinition _conditionDefinition;
        private GameEventRuleDefinitions _ruleDefinitions;
        private IList<GameEventPreProcessorDefinition> _preProcessorDefinitions = new List<GameEventPreProcessorDefinition>();
        private readonly string _label;

        internal void Add(GameEventPreProcessorDefinition preProcessorDefinition)
        {
            _preProcessorDefinitions.Add(preProcessorDefinition);
        }

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

        IGameEventRuleDefinitionsWithOption IThenGameEventRule.Then()
        {
            _ruleDefinitions = new GameEventRuleDefinitions(Builder, this);
            return _ruleDefinitions;
        }

        internal GamePhase Build(int number, Game game, CompositeGamePhase parent = null)
        {
            if (_conditionDefinition == null || _ruleDefinitions == null)
            {
                throw new BoardException("Incomplete game phase");
            }

            var condition = _conditionDefinition.Build(game);
            var rule = _ruleDefinitions.Build(game);
            var preprocessors = _preProcessorDefinitions.Select(x => x.Build(game)).ToList();
            return GamePhase.New(_number ?? number, _label, condition, rule, parent, preprocessors);
        }
    }
}
