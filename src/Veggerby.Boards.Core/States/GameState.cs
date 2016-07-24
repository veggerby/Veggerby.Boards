using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public class GameState : State<Game>
    {
        private readonly IEnumerable<IState> _childStates;

        public IEnumerable<IState> ChildStates => _childStates.ToList().AsReadOnly();

        public GameState(Game game, IEnumerable<IState> childStates) : base(game)
        {
            _childStates = (childStates ?? Enumerable.Empty<IState>()).ToList();
        }

        public IEnumerable<State<T>> GetStates<T>() where T : Artifact
        {
            return _childStates
                .OfType<State<T>>()
                .ToList();
        }

        public State<T> GetState<T>(T artifact) where T : Artifact
        {
            return GetState(artifact as Artifact) as State<T>;
        }

        public IState GetState(Artifact artifact)
        {
            return _childStates
                .SingleOrDefault(x => x.Artifact.Equals(artifact));
        }

        public GameState Update(IEnumerable<IState> newStates)
        {
            var currentStates = newStates.Select(x => GetState(x.Artifact)).ToList();
            var states = _childStates.Except(currentStates).Concat(newStates).ToList();
            return new GameState(Artifact, states);
        }

        public new GameState OnBeforeEvent(IGameEvent @event)
        {
            var states = _childStates
                .Select(x => x.OnBeforeEvent(@event))
                .Where(x => x != null)
                .ToList();

            if (!states.Any())
            {
                return this;
            }
            
            return Update(states);
        }

        public new GameState OnAfterEvent(IGameEvent @event)
        {
             var states = _childStates
                .Select(x => x.OnAfterEvent(@event))
                .Where(x => x != null)
                .ToList();
            
            if (!states.Any())
            {
                return this;
            }
            
            return Update(states);
        }
    }
}