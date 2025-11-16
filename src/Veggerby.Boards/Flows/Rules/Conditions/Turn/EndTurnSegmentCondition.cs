using System.Linq;

using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions.Turn;

/// <summary>
/// Condition validating that an <see cref="EndTurnSegmentEvent"/> is applicable in the current game state.
/// </summary>
/// <remarks>
/// Requirements:
/// 1. A current TurnState must exist.
/// 2. TurnState.Segment must equal event.Segment.
/// </remarks>
internal sealed class EndTurnSegmentCondition : IGameEventCondition<EndTurnSegmentEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, EndTurnSegmentEvent @event)
    {
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