using System;
using System.Linq;

using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that checks if the game has ended (only one non-bankrupt player remaining).
/// </summary>
public class MonopolyGameEndedCondition : IGameStateCondition
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        // Check if GameEndedState already exists
        if (state.GetStates<GameEndedState>().Any())
        {
            return ConditionResponse.Ignore("Game has already ended");
        }

        var playerStates = state.GetStates<MonopolyPlayerState>().ToList();
        if (playerStates.Count == 0)
        {
            return ConditionResponse.Ignore("No player states found");
        }

        var activePlayers = playerStates.Count(ps => !ps.IsBankrupt);

        // Game ends when only one player remains
        if (activePlayers <= 1)
        {
            return ConditionResponse.Valid;
        }

        return ConditionResponse.Ignore($"{activePlayers} players still active");
    }
}
