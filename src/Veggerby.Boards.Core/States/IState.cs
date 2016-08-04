using Veggerby.Boards.Core.Events;

namespace Veggerby.Boards.Core.States
{
    public interface IState
    {
        IState OnBeforeEvent(IGameEvent @event);
        IState OnAfterEvent(IGameEvent @event);
    }
}