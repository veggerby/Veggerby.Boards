using System;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;

namespace Veggerby.Boards.Core.Flows.Phases
{
    public class GamePhase
    {
        public int Number { get; }
        public CompositeGamePhase Parent { get; }
        public IGameStateCondition Condition { get; }
        public IGameEventRule Rule { get; }

        protected GamePhase(int number, IGameStateCondition condition, IGameEventRule rule, CompositeGamePhase parent)
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
            Rule = rule ?? SimpleGameEventRule<IGameEvent>.New(() => RuleCheckState.Valid);

            Parent = parent;
            parent?.Add(this);
        }

        public virtual GamePhase GetActiveGamePhase(GameState gameState)
        {
            return Condition.Evaluate(gameState) ? this : null;
        }

        public static GamePhase New(int number, IGameStateCondition condition, IGameEventRule rule = null, CompositeGamePhase parent = null)
        {
            return new GamePhase(number, condition, rule, parent);
        }

        public static CompositeGamePhase NewParent(int number, IGameStateCondition condition = null, CompositeGamePhase parent = null)
        {
            return new CompositeGamePhase(number, condition ?? new NullGameStateCondition(), parent);
        }
    }
}