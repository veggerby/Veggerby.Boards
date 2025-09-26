using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Simulation;

/// <summary>
/// Strategy interface for selecting the next <see cref="IGameEvent"/> to attempt during a simulation playout.
/// </summary>
/// <remarks>
/// Implementations must be pure and deterministic for a given input <see cref="GameProgress"/> unless explicitly
/// using a deterministic random value within <see cref="GameState"/> (e.g., seeded dice state) to drive stochastic policies.
/// Implementations must never mutate the provided progress/state; they may inspect it and produce candidate events.
/// Returning an empty enumeration signals no further events are available (terminal playout state).
/// </remarks>
public interface IPlayoutPolicy
{
    /// <summary>
    /// Enumerates candidate events to attempt for the given progress snapshot in preferred order.
    /// </summary>
    /// <param name="progress">Current immutable progress snapshot.</param>
    /// <returns>Sequence of candidate events; empty when terminal.</returns>
    IEnumerable<IGameEvent> GetCandidateEvents(GameProgress progress);
}