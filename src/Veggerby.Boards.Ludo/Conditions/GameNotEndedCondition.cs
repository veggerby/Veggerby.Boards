using System;
using System.Linq;

using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Ludo.Conditions;

/// <summary>
/// Checks that the game has not ended (no <see cref="GameEndedState"/> present).
/// </summary>
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
