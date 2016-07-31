using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public static class StateExtensions
    {
        public static GameState Set<T>(this GameState state, T artifact, Func<T, State<T>, State<T>> stateFunc) where T : Artifact
        {
            var currentState = state.GetState(artifact);
            var newState = stateFunc(artifact, currentState);
            if (newState == null)
            {
                return state.Remove(artifact);
            }

            return state.Update(new[] { newState });
        }
    }
}