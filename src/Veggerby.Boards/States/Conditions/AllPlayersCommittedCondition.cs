using System;
using System.Linq;

namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Checks that all players have committed their actions (a <see cref="StagedEventsState"/> exists and is complete).
/// </summary>
/// <remarks>
/// This condition is used to enable the reveal phase where all committed actions are resolved
/// simultaneously. If no staged events state exists or not all players have committed, this
/// condition returns NotApplicable or Ignore.
/// </remarks>
public sealed class AllPlayersCommittedCondition : IGameStateCondition
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var stagedState = state.GetStates<StagedEventsState>().FirstOrDefault();
        if (stagedState is null)
        {
            return ConditionResponse.NotApplicable;
        }

        if (!stagedState.IsComplete)
        {
            return ConditionResponse.Ignore("Not all players have committed");
        }

        return ConditionResponse.Valid;
    }
}
