using System.Linq;

using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions.Commitment;

/// <summary>
/// Condition validating that a <see cref="RevealCommitmentsEvent"/> is applicable in the current game state.
/// </summary>
/// <remarks>
/// Requirements:
/// 1. A <see cref="StagedEventsState"/> must exist (commitment phase active).
/// 2. All required players must have committed (pending set is empty).
/// </remarks>
internal sealed class RevealCommitmentsCondition : IGameEventCondition<RevealCommitmentsEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, RevealCommitmentsEvent @event)
    {
        var stagedState = state.GetStates<StagedEventsState>().FirstOrDefault();
        if (stagedState is null)
        {
            return ConditionResponse.NotApplicable;
        }

        if (!stagedState.IsComplete)
        {
            return ConditionResponse.Invalid;
        }

        return ConditionResponse.Valid;
    }
}
