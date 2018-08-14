using System;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Phases
{
    public abstract class GamePhase
    {
        public int Number { get; }
        public CompositeGamePhase Parent { get; }
        public IGameStateCondition Condition { get; }

        protected GamePhase(int number, CompositeGamePhase parent, IGameStateCondition condition)
        {
            if (number <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Round number must be positive and non-zero");
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            Number = number;
            Parent = parent;
            Condition = condition;

            parent.Add(this);
        }

        public virtual GamePhase GetActiveGamePhase(GameEngine engine)
        {
            var gamePhaseState = Condition.Evaluate(engine.GameState);

            if (gamePhaseState)
            {
                return this;
            }

            return null;
        }
    }
}