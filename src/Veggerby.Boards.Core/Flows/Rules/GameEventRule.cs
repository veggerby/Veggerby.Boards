using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public abstract class GameEventRule<T> : IGameEventRule where T : IGameEvent
    {
        public static IGameEventRule Null = SimpleGameEventRule<T>.New(new SimpleGameEventCondition<T>((engine, state, @event) => ConditionResponse.Valid));

        private readonly IStateMutator<T> _onBeforeEvent;
        private readonly IStateMutator<T> _onAfterEvent;

        public GameEventRule(IStateMutator<T> onBeforeEvent, IStateMutator<T> onAfterEvent)
        {
            _onBeforeEvent = onBeforeEvent;
            _onAfterEvent = onAfterEvent;
        }

        protected abstract ConditionResponse Check(GameEngine engine, GameState gameState, T @event);

        private GameState MutateState(IStateMutator<T> eventMutator, GameEngine engine, GameState gameState, T @event)
        {
            return eventMutator != null ? eventMutator.MutateState(engine, gameState, @event) : gameState;
        }

        protected GameState HandleEvent(GameEngine engine, GameState gameState, T @event)
        {
            var newState = MutateState(_onBeforeEvent, engine, gameState, @event);

            var check = Check(engine, newState, @event);

            if (check.Result == ConditionResult.Valid)
            {
                return MutateState(_onAfterEvent, engine, newState, @event);
            }
            else if (check.Result == ConditionResult.Ignore)
            {
                // do nothing, return original state
                return gameState;
            }

            throw new BoardException("Invalid game event");
        }

        ConditionResponse IGameEventRule.Check(GameEngine engine, GameState gameState, IGameEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            if (@event is T)
            {
                return Check(engine, gameState, (T)@event);
            }

            return ConditionResponse.NotApplicable;
        }

        GameState IGameEventRule.HandleEvent(GameEngine engine, GameState gameState, IGameEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            if (@event is T)
            {
                return HandleEvent(engine, gameState, (T)@event);
            }

            return gameState;
        }
    }
}