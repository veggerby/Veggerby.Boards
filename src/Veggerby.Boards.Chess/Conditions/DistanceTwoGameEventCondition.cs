using System;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess.Conditions;

/// <summary>
/// Condition valid only when the move path length is exactly two relations.
/// </summary>
public sealed class DistanceTwoGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <summary>
    /// Returns Valid only if the supplied move event path length equals two; otherwise Ignore.
    /// </summary>
    /// <param name="engine">Game engine (unused).</param>
    /// <param name="state">Current immutable state (unused).</param>
    /// <param name="moveEvent">Move event under evaluation.</param>
    /// <returns>ConditionResponse.Valid when distance is two, else Ignore.</returns>
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent moveEvent)
    {
        ArgumentNullException.ThrowIfNull(engine, nameof(engine));
        ArgumentNullException.ThrowIfNull(state, nameof(state));
        ArgumentNullException.ThrowIfNull(moveEvent, nameof(moveEvent));

        return moveEvent.Distance == 2
            ? ConditionResponse.Valid
            : ConditionResponse.Ignore("Distance not two");
    }
}