﻿using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public abstract class GameEventRule<T> : IGameEventRule where T : IGameEvent
    {
        private readonly IStateMutator<T> _onBeforeEvent;
        private readonly IStateMutator<T> _onAfterEvent;

        public GameEventRule(IStateMutator<T> onBeforeEvent, IStateMutator<T> onAfterEvent)
        {
            _onBeforeEvent = onBeforeEvent;
            _onAfterEvent = onAfterEvent;
        }

        protected abstract RuleCheckState Check(GameState gameState, T @event);

        private GameState MutateState(IStateMutator<T> eventMutator, GameState gameState, T @event)
        {
            return eventMutator != null ? eventMutator.MutateState(gameState, @event) : gameState;
        }

        protected GameState HandleEvent(GameState gameState, T @event)
        {
            var newState = MutateState(_onBeforeEvent, gameState, @event);

            var check = Check(newState, @event);

            if (check.Result == RuleCheckResult.Valid)
            {
                return MutateState(_onAfterEvent, newState, @event);
            }
            else if (check.Result == RuleCheckResult.Ignore)
            {
                // do nothing, return original state
                return gameState;
            }

            throw new BoardException("Invalid game event");
        }

        RuleCheckState IGameEventRule.Check(GameState gameState, IGameEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            if (@event is T)
            {
                return Check(gameState, (T)@event);
            }

            return RuleCheckState.NotApplicable;
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