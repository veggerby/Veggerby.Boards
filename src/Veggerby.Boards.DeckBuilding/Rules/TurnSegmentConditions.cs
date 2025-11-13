using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Condition requiring the turn segment to be Start (used for setup events).
/// </summary>
public sealed class TurnSegmentStartCondition : IGameStateCondition
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        TurnState? turn = null;
        foreach (var child in state.ChildStates)
        {
            if (child is TurnState ts)
            {
                turn = ts;
                break;
            }
        }
        if (turn is null)
        {
            // When turn sequencing feature flag is disabled, no TurnState exists yet; treat as Start to allow setup.
            return ConditionResponse.Valid;
        }
        return turn.Segment == TurnSegment.Start
            ? ConditionResponse.Valid
            : ConditionResponse.Ignore("Different segment"); // ignore so other segment phases can still activate
    }
}

/// <summary>
/// Condition requiring the turn segment to be Main (used for action and buy events for now).
/// </summary>
public sealed class TurnSegmentMainCondition : IGameStateCondition
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        TurnState? turn = null;
        foreach (var child in state.ChildStates)
        {
            if (child is TurnState ts)
            {
                turn = ts;
                break;
            }
        }
        if (turn is null)
        {
            return ConditionResponse.NotApplicable;
        }
        return turn.Segment == TurnSegment.Main
            ? ConditionResponse.Valid
            : ConditionResponse.Ignore("Different segment");
    }
}

/// <summary>
/// Condition requiring the turn segment to be End (used for cleanup events).
/// </summary>
public sealed class TurnSegmentEndCondition : IGameStateCondition
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        TurnState? turn = null;
        foreach (var child in state.ChildStates)
        {
            if (child is TurnState ts)
            {
                turn = ts;
                break;
            }
        }
        if (turn is null)
        {
            return ConditionResponse.NotApplicable;
        }
        return turn.Segment == TurnSegment.End
            ? ConditionResponse.Valid
            : ConditionResponse.Ignore("Different segment");
    }
}