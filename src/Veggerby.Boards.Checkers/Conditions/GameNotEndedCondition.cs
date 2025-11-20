using System;
using System.Linq;

using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Checkers.Conditions;

/// <summary>
/// Checks that the game has not ended (no <see cref="GameEndedState"/> present).
/// </summary>
/// <remarks>
/// This condition is used to prevent moves from being processed after the game has reached a terminal state.
/// </remarks>
public sealed class GameNotEndedCondition : IGameStateCondition
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.GetStates<GameEndedState>().Any())
        {
            return ConditionResponse.Ignore("Game has ended");
        }

        return ConditionResponse.Valid;
    }
}
