using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Events
{
    public class GameEventHandler<T> where T : Artifact
    {
        private readonly Func<State<T>, IGameEvent, State<T>> _onBeforeEvent;
        private readonly Func<State<T>, IGameEvent, State<T>> _onAfterEvent;

        public GameEventHandler(Func<State<T>, IGameEvent, State<T>> onBeforeEvent, Func<State<T>, IGameEvent, State<T>> onAfterEvent)
        {
            if (onBeforeEvent == null && onAfterEvent == null)
            {
                throw new ArgumentNullException(nameof(onAfterEvent), "Both before and after event are null");
            }

            _onBeforeEvent = onBeforeEvent;
            _onAfterEvent = onAfterEvent;
        }

        private State<T> HandleEvent(Func<State<T>, IGameEvent, State<T>> eventMutator, State<T> currentState, IGameEvent @event)
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

        public State<T> OnBeforeEvent(State<T> currentState, IGameEvent @event)
        {
            return HandleEvent(_onBeforeEvent, currentState, @event);
        }

        public State<T> OnAfterEvent(State<T> currentState, IGameEvent @event)
        {
            return HandleEvent(_onAfterEvent, currentState, @event);
        }
    }
}