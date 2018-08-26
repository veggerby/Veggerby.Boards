using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Phases
{
    public class CompositeGamePhase : GamePhase
    {
        private readonly IList<GamePhase> _childPhases;

        public CompositeGamePhase(int number, IGameStateCondition condition, CompositeGamePhase parent) : base(number, condition, parent)
        {
            _childPhases = new List<GamePhase>();
        }

        public override GamePhase GetActiveGamePhase(GameEngine engine)
        {
            if (!Condition.Evaluate(engine.GameState))
            {
                return null;
            }

            return _childPhases.FirstOrDefault(x => x.GetActiveGamePhase(engine) != null);
        }

        internal void Add(GamePhase phase)
        {
            _childPhases.Add(phase);
        }
    }
}