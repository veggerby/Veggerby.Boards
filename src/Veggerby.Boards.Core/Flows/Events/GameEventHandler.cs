using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Events
{
    public class GameEventHandler<T> where T : Artifact
    {
        private readonly Func<ArtifactState, IGameEvent, ArtifactState> _onBeforeEvent;
        private readonly Func<ArtifactState, IGameEvent, ArtifactState> _onAfterEvent;

        public GameEventHandler(Func<ArtifactState, IGameEvent, ArtifactState> onBeforeEvent, Func<ArtifactState, IGameEvent, ArtifactState> onAfterEvent)
        {
            if (onBeforeEvent == null && onAfterEvent == null)
            {
                throw new ArgumentNullException(nameof(onAfterEvent), "Both before and after event are null");
            }

            _onBeforeEvent = onBeforeEvent;
            _onAfterEvent = onAfterEvent;
        }

        private ArtifactState HandleEvent(Func<ArtifactState, IGameEvent, ArtifactState> eventMutator, ArtifactState currentState, IGameEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            var newState = eventMutator != null ? eventMutator(currentState, @event) : null;

            if (newState == null)
            {
                return currentState;
            }

            if (currentState != null && !newState.Artifact.Equals(currentState.Artifact))
            {
                throw new BoardException("Invalid artifact on state change");
            }

            return newState;
        }

        public ArtifactState OnBeforeEvent(ArtifactState currentState, IGameEvent @event)
        {
            return HandleEvent(_onBeforeEvent, currentState, @event);
        }

        public ArtifactState OnAfterEvent(ArtifactState currentState, IGameEvent @event)
        {
            return HandleEvent(_onAfterEvent, currentState, @event);
        }
    }
}