using System;
using System.Linq;

using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if the game has NOT ended yet.
/// </summary>
public class MonopolyGameNotEndedCondition : IGameStateCondition
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
