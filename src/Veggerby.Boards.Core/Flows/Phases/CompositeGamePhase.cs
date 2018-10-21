using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Phases
{
    public class CompositeGamePhase : GamePhase
    {
        private readonly IList<GamePhase> _childPhases;

        public IEnumerable<GamePhase> ChildPhases => _childPhases.ToList().AsReadOnly();

        internal CompositeGamePhase(int number, string label, IGameStateCondition condition, CompositeGamePhase parent, IEnumerable<IGameEventPreProcessor> preProcessors)
            : base(number, label, condition, GameEventRule<IGameEvent>.Null, parent, preProcessors)
        {
            _childPhases = new List<GamePhase>();
        }

        public override GamePhase GetActiveGamePhase(GameState gameState)
        {
            if (!Condition.Evaluate(gameState).Equals(ConditionResponse.Valid))
            {
                return null;
            }

            return _childPhases
                .Select(x => x.GetActiveGamePhase(gameState))
                .FirstOrDefault(x => x != null);
        }

        internal void Add(GamePhase phase)
        {
            _childPhases.Add(phase);
        }
    }
}