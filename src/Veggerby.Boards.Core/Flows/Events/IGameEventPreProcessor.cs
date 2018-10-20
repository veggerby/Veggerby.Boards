using System.Collections.Generic;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Events
{
    public interface IGameEventPreProcessor
    {
        IEnumerable<IGameEvent> ProcessEvent(GameProgress progress, IGameEvent @event);
    }
}