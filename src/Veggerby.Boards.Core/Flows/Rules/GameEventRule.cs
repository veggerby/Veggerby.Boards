using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public abstract class GameEventRule<T> : IGameEventRule where T : IGameEvent
    {
        public static IGameEventRule Null = SimpleGameEventRule<T>.New(new SimpleGameEventCondition<T>((s, e) => ConditionResponse.Valid));

        private readonly IStateMutator<T> _onBeforeEvent;
        private readonly IStateMutator<T> _onAfterEvent;

        public GameEventRule(IStateMutator<T> onBeforeEvent, IStateMutator<T> onAfterEvent)
        {
            _onBeforeEvent = onBeforeEvent;
            _onAfterEvent = onAfterEvent;
        }

        protected abstract ConditionResponse Check(GameState gameState, T @event);

        private GameState MutateState(IStateMutator<T> eventMutator, GameState gameState, T @event)
        {
            return eventMutator != null ? eventMutator.MutateState(gameState, @event) : gameState;
        }

        protected GameState HandleEvent(GameState gameState, T @event)
        {
            var newState = MutateState(_onBeforeEvent, gameState, @event);

            var check = Check(newState, @event);

            if (check.Result == ConditionResult.Valid)
            {
                return MutateState(_onAfterEvent, newState, @event);
            }
            else if (check.Result == ConditionResult.Ignore)
            {
                // do nothing, return original state
                return gameState;
            }

            throw new BoardException("Invalid game event");
        }

        ConditionResponse IGameEventRule.Check(GameState gameState, IGameEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            if (@event is T)
            {
                return Check(gameState, (T)@event);
            }

            return ConditionResponse.NotApplicable;
        }

        GameState IGameEventRule.HandleEvent(GameState gameState, IGameEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            if (@event is T)
            {
                return HandleEvent(gameState, (T)@event);
            }

            return gameState;
        }
    }
}