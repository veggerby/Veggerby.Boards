using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public interface IState
    {
        Artifact Artifact { get; }
        IState OnBeforeEvent(IGameEvent @event);
        IState OnAfterEvent(IGameEvent @event);
    }
}