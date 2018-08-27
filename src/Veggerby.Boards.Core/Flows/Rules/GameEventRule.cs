using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public abstract class GameEventRule : IGameEventRule
    {
        private readonly IStateMutator<IGameEvent> _onBeforeEvent;
        private readonly IStateMutator<IGameEvent> _onAfterEvent;

        public GameEventRule(IStateMutator<IGameEvent> onBeforeEvent, IStateMutator<IGameEvent> onAfterEvent)
        {
            if (_onBeforeEvent == null && _onAfterEvent == null)
            {
                throw new ArgumentNullException(nameof(_onAfterEvent), "Both onBefore and onAfter cannot be null");
            }

            _onBeforeEvent = onBeforeEvent;
            _onAfterEvent = onAfterEvent;
        }

        public abstract RuleCheckState Check(GameState gameState, IGameEvent @event);

        private GameState MutateState(IStateMutator<IGameEvent> eventMutator, GameState gameState, IGameEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            return eventMutator != null ? eventMutator.MutateState(gameState, @event) : gameState;
        }

        public GameState HandleEvent(GameState gameState, IGameEvent @event)
        {
            var newState = MutateState(_onBeforeEvent, gameState, @event);

            var check = Check(newState, @event);

            switch (check.Result)
            {
                case RuleCheckResult.Invalid:
                    throw new BoardException("Invalid game event");
                case RuleCheckResult.Ignore:
                    return gameState;
                case RuleCheckResult.Valid:
                    return MutateState(_onAfterEvent, newState, @event);
                default:
                    throw new BoardException("Invalid rule check result");
            }
        }
    }
}