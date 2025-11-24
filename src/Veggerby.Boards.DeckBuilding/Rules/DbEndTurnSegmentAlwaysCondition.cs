using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding.Rules;

/// <summary>
/// Permissive condition allowing any EndTurnSegmentEvent to advance during deck-building (future: restrict by active player, segment order).
/// </summary>
public sealed class DbEndTurnSegmentAlwaysCondition : IGameEventCondition<EndTurnSegmentEvent>
{
    /// <summary>
    /// Always returns <see cref="ConditionResponse.Valid"/> permitting the <see cref="EndTurnSegmentEvent"/> to be processed.
    /// </summary>
    /// <param name="engine">Engine (unused).</param>
    /// <param name="state">Current game state (unused).</param>
    /// <param name="event">The end-turn-segment event.</param>
    /// <returns><see cref="ConditionResponse.Valid"/> unconditionally.</returns>
    public ConditionResponse Evaluate(GameEngine engine, GameState state, EndTurnSegmentEvent @event)
    {
        return ConditionResponse.Valid;
    }
}