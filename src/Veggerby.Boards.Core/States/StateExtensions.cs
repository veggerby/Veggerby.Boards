using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public static class StateExtensions
    {
        public static GameState Set<T, TState>(this GameState state, T artifact, Func<IArtifactState, TState> stateFunc) 
            where T : Artifact
            where TState : IArtifactState
        {
            var currentState = state.GetState<TState>(artifact);
            var newState = stateFunc(currentState);
            return state.Set(artifact, newState);
        }

        public static GameState Set<T, TState>(this GameState state, T artifact, TState newState) 
            where T : Artifact
            where TState : IArtifactState
        {
            if (artifact == null)
            {
                throw new ArgumentNullException(nameof(artifact));
            }

            if (newState == null)
            {
                return state.Remove<TState>(artifact);
            }

            if (!artifact.Equals(newState.Artifact))
            {
                throw new ArgumentException("Invalid artifact", nameof(artifact));
            }

            return state.Set(newState);
        }

        public static GameState Set<TState>(this GameState state, TState newState) 
            where TState : IArtifactState
        {
            if (newState == null)
            {
                throw new ArgumentNullException(nameof(newState));
            }

            return state.Update(new IState[] { newState });
        }
    }
}