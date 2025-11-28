using System;
using System.Linq;

using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Risk.Conditions;

/// <summary>
/// Condition that evaluates to Valid when a single player controls all territories.
/// </summary>
public sealed class WorldDominationCondition : IGameStateCondition
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var territoryStates = state.GetStates<TerritoryState>().ToList();

        if (territoryStates.Count == 0)
        {
            return ConditionResponse.Ignore("No territories found.");
        }

        var firstOwner = territoryStates[0].Owner;

        for (int i = 1; i < territoryStates.Count; i++)
        {
            if (!territoryStates[i].Owner.Equals(firstOwner))
            {
                return ConditionResponse.Ignore("Multiple players still have territories.");
            }
        }

        return ConditionResponse.Valid;
    }
}
