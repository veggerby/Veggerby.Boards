using System;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public class SimpleGameStateMutator<T> : IStateMutator<T> where T : IGameEvent
    {
        private readonly Func<T, IArtifactState> _stateFunc;

        public SimpleGameStateMutator(Func<T, IArtifactState> stateFunc)
        {
            if (stateFunc == null)
            {
                throw new ArgumentNullException(nameof(stateFunc));
            }

            _stateFunc = stateFunc;
        }

        public GameState MutateState(GameEngine engine, GameState gameState, T @event)
        {
            var artifactState = _stateFunc(@event);
            if (artifactState == null)
            {
                return gameState;
            }

            return gameState.Next([artifactState]);
        }
    }
}