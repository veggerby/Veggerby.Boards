using System;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Phases
{
    public class GamePhase
    {
        public int Number { get; }
        public CompositeGamePhase Parent { get; }
        public IGameStateCondition Condition { get; }

        protected GamePhase(int number, IGameStateCondition condition, CompositeGamePhase parent)
        {
            if (number <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Round number must be positive and non-zero");
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            Number = number;
            Condition = condition;

            Parent = parent;
            parent?.Add(this);
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

        public static GamePhase New(int number, IGameStateCondition condition, CompositeGamePhase parent = null)
        {
            return new GamePhase(number, condition, parent);
        }
    }
}