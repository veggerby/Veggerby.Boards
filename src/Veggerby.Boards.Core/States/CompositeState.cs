using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public class CompositeState<T> : State<T>
        where T : Artifact
    {
        private readonly IEnumerable<IState> _childStates;

        public CompositeState(T artifact, IEnumerable<IState> childStates) : base(artifact)
        {
            _childStates = (childStates ?? Enumerable.Empty<IState>()).ToList();
        }
        
        public IEnumerable<State<TChild>> GetStates<TChild>() where TChild : Artifact
        {
            return _childStates
                .OfType<State<TChild>>()
                .ToList();
        }

        public State<TChild> GetState<TChild>(TChild artifact) where TChild : Artifact
        {
            return GetStates<TChild>()
                .SingleOrDefault(x => x.Artifact.Equals(artifact));
        }

        public CompositeState<T> RemoveState<TChild>(TChild artifact) where TChild : Artifact
        {
            var state = GetState(artifact);
            return new CompositeState<T>(Artifact, _childStates.Except(new IState[] { state }));
        }

        public CompositeState<T> ApplyState<TChild>(State<TChild> newState) where TChild : Artifact
        {
            var currentState = GetState(newState.Artifact);

            return new CompositeState<T>(Artifact, _childStates.Except(new IState[] { currentState }).Concat(new IState[] { newState }));
        }
    }
}