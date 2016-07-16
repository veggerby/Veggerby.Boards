using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public class GameEventHandler<T> where T : Artifact
    {
        private readonly Func<State<T>, IGameEvent, State<T>> _onBeforeEvent;
        private readonly Func<State<T>, IGameEvent, State<T>> _onAfterEvent;

        public GameEventHandler(Func<State<T>, IGameEvent, State<T>> onBeforeEvent, Func<State<T>, IGameEvent, State<T>> onAfterEvent)
        {
            _onBeforeEvent = onBeforeEvent;
            _onAfterEvent = onAfterEvent;
        }

        private State<T> HandleEvent(Func<State<T>, IGameEvent, State<T>> eventMutator, State<T> currentState, IGameEvent @event)
        {
            var newState = eventMutator != null ? eventMutator(currentState, @event) : null;

            if (newState == null) 
            {
                return currentState;
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