using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public abstract class State<T> : IState
        where T : Artifact
    {
        public T Artifact { get; }

        private readonly GameEventHandler<T> _eventHandler;

        public State(T artifact, GameEventHandler<T> eventHandler = null)
        {
            Artifact = artifact;
            _eventHandler = eventHandler;
        }

        public State<T> OnBeforeEvent(IGameEvent @event) 
        {
            var newState = _eventHandler?.OnBeforeEvent(this, @event);

            if (newState == null || newState == this)
            {
                return null;
            }

            return newState;
        }

        public State<T> OnAfterEvent(IGameEvent @event) 
        {
            var newState = _eventHandler?.OnAfterEvent(this, @event);

            if (newState == null || newState == this)
            {
                return null;
            }

            return newState;
        }

        Artifact IState.Artifact => Artifact;

        IState IState.OnBeforeEvent(IGameEvent @event)
        {
            return OnBeforeEvent(@event);
        }

        IState IState.OnAfterEvent(IGameEvent @event)
        {
            return OnAfterEvent(@event);
        }
    }
}