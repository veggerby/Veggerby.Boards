using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Builder.Phases;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules;

namespace Veggerby.Boards.Core.Builder.Rules
{
    internal class GameEventRuleDefinitions : DefinitionBase, IGameEventRuleDefinitionsWithOption
    {
        public GameEventRuleDefinitions(GameBuilder builder, GamePhaseDefinition parent) : base(builder)
        {
            _parent = parent;
            _eventRuleCompositeMode = CompositeMode.Any;
        }

        private readonly GamePhaseDefinition _parent;
        private IList<IGameEventRuleDefinition> _ruleDefinitions = new List<IGameEventRuleDefinition>();
        private CompositeMode _eventRuleCompositeMode;

        private void SetCompositeMode(CompositeMode mode)
        {
            if (_ruleDefinitions.Any())
            {
                throw new ArgumentException("Can only set composite mode when no rules are added");
            }

            _eventRuleCompositeMode = mode;
        }

        public IGameEventRule Build(Game game)
        {
            if (!(_ruleDefinitions?.Any() ?? false))
            {
                return null;
            }

            if (_ruleDefinitions.Count() == 1)
            {
                return _ruleDefinitions.Single().Build(game);
            }

            var rules = _ruleDefinitions.Select(x => x.Build(game)).ToArray();
            return CompositeGameEventRule.CreateCompositeRule(
                _eventRuleCompositeMode,
                rules);
        }

        IGameEventRuleDefinitions IGameEventRuleDefinitionsWithOption.All()
        {
            SetCompositeMode(CompositeMode.All);
            return this;
        }

        IGameEventRuleDefinitions IGameEventRuleDefinitionsWithOption.First()
        {
            SetCompositeMode(CompositeMode.Any);
            return this;
        }

        IGameEventRuleDefinition<T> IGameEventRuleDefinitions.ForEvent<T>()
        {
            var rule = new GameEventRuleDefinition<T>(Builder, this);
            _ruleDefinitions.Add(rule);
            return rule;
        }

        IGameEventRuleDefinitionsWithOption IGameEventRuleDefinitionsWithOption.PreProcessEvent(GameEventPreProcessorFactory factory)
        {
            var preprocessor = new GameEventPreProcessorDefinition(Builder, this, factory);
            _parent.Add(preprocessor);
            return this;
        }
    }
}
