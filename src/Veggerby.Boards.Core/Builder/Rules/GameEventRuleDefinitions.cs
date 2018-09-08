using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Builder.Phases;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules;

namespace Veggerby.Boards.Core.Builder.Rules
{
    internal class GameEventRuleDefinitions : DefinitionBase, IForGameEventRule
    {
        public GameEventRuleDefinitions(GameEngineBuilder builder, GamePhaseDefinition parent) : base(builder)
        {
            _parent = parent;
        }

        private readonly GamePhaseDefinition _parent;
        private IList<IGameEventRuleDefinition> _ruleDefinitions = new List<IGameEventRuleDefinition>();
        private CompositeMode _eventRuleCompositeMode;

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

        IGameEventRuleDefinition<T> IForGameEventRule.ForEvent<T>()
        {
             _eventRuleCompositeMode = CompositeMode.Any;
            var rule = new GameEventRuleDefinition<T>(Builder, this);
            _ruleDefinitions.Add(rule);
            return rule;
        }
    }
}
