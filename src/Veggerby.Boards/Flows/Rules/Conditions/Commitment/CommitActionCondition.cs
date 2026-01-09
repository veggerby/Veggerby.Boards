using System.Linq;

using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions.Commitment;

/// <summary>
/// Condition validating that a <see cref="CommitActionEvent"/> is applicable in the current game state.
/// </summary>
/// <remarks>
/// Requirements:
/// 1. A <see cref="StagedEventsState"/> must exist (commitment phase active).
/// 2. The committing player must be in the pending set (not already committed).
/// </remarks>
internal sealed class CommitActionCondition : IGameEventCondition<CommitActionEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, CommitActionEvent @event)
    {
        var stagedState = state.GetStates<StagedEventsState>().FirstOrDefault();
        if (stagedState is null)
        {
            return ConditionResponse.NotApplicable;
        }

        if (!stagedState.PendingPlayers.Contains(@event.Player))
        {
            return ConditionResponse.Invalid;
        }

        return ConditionResponse.Valid;
    }
}
