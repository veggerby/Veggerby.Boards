using System.Linq;

using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions.Turn;

/// <summary>
/// Condition validating that an <see cref="EndTurnSegmentEvent"/> is applicable in the current game state.
/// </summary>
/// <remarks>
/// Requirements:
/// 1. Feature flag EnableTurnSequencing must be enabled.
/// 2. A current TurnState must exist.
/// 3. TurnState.Segment must equal event.Segment.
/// </remarks>
internal sealed class EndTurnSegmentCondition : IGameEventCondition<EndTurnSegmentEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, EndTurnSegmentEvent @event)
    {
        if (!Internal.FeatureFlags.EnableTurnSequencing)
        {
            return ConditionResponse.NotApplicable;
        }

        var turnState = state.GetStates<TurnState>().FirstOrDefault();
        if (turnState is null)
        {
            return ConditionResponse.NotApplicable;
        }

        if (turnState.Segment != @event.Segment)
        {
            return ConditionResponse.Invalid;
        }

        return ConditionResponse.Valid;
    }
}