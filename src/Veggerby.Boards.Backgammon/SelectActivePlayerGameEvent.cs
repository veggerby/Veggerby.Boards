using System;

using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Event representing the selection of the starting active player after the initial dice comparison.
/// </summary>
/// <remarks>
/// This is emitted conceptually after the opening dice roll has produced distinct values.
/// It is classified as <c>EventKind.State</c> (via <see cref="IStateMutationGameEvent"/>) to exercise
/// the DecisionPlan event kind filtering pipeline.
/// </remarks>
public sealed class SelectActivePlayerGameEvent : IGameEvent, IStateMutationGameEvent
{
    /// <summary>
    /// Gets the identifier of the active player (white/black by Backgammon module convention).
    /// </summary>
    public string ActivePlayerId
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectActivePlayerGameEvent"/> class.
    /// </summary>
    /// <param name="activePlayerId">Artifact id of the player to become active.</param>
    public SelectActivePlayerGameEvent(string activePlayerId)
    {
        ArgumentException.ThrowIfNullOrEmpty(activePlayerId);
        ActivePlayerId = activePlayerId;
    }
}