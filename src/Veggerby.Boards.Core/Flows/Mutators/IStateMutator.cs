using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public interface IStateMutator<T> where T : IGameEvent
    {
        IArtifactState MutateState(IArtifactState currentState, T @event);
    }
}