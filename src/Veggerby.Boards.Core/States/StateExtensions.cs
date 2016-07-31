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
            if (newState == null)
            {
                return state.Remove<TState>(artifact);
            }

            return state.Update(new IState[] { newState });
        }
    }
}