using System.Collections.Generic;

using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Events;

/// <summary>
/// Transforms an incoming event into zero, one or many derived events prior to rule evaluation.
/// </summary>
public interface IGameEventPreProcessor
{
    /// <summary>
    /// Processes an event producing replacement events (or the original if unchanged).
    /// </summary>
    /// <param name="progress">Current game progress.</param>
    /// <param name="event">Original event.</param>
    /// <returns>Derived events sequence.</returns>
    IEnumerable<IGameEvent> ProcessEvent(GameProgress progress, IGameEvent @event);
}