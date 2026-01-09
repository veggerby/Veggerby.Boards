using System;
using System.Linq;

namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Checks that a commitment phase is active (a <see cref="StagedEventsState"/> exists and is not yet complete).
/// </summary>
/// <remarks>
/// This condition is used to ensure that commitment-related events can only be processed during
/// an active commitment phase. If no staged events state exists or all players have already committed,
/// this condition returns NotApplicable or Ignore.
/// </remarks>
public sealed class CommitmentPhaseActiveCondition : IGameStateCondition
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

        if (stagedState.IsComplete)
        {
            return ConditionResponse.Ignore("All players have already committed");
        }

        return ConditionResponse.Valid;
    }
}
