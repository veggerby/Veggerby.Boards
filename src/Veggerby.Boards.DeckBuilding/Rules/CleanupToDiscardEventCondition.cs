using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Validates cleanup prerequisites: deck exists and required piles are present.
/// </summary>
public sealed class CleanupToDiscardEventCondition : IGameEventCondition<CleanupToDiscardEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, CleanupToDiscardEvent @event)
    {
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
        {
            return ConditionResponse.NotApplicable;
        }
        if (!ds.Piles.ContainsKey(DeckBuildingGameBuilder.Piles.Hand) ||
            !ds.Piles.ContainsKey(DeckBuildingGameBuilder.Piles.InPlay) ||
            !ds.Piles.ContainsKey(DeckBuildingGameBuilder.Piles.Discard))
        {
            return ConditionResponse.Fail("Required piles missing");
        }
        return ConditionResponse.Valid;
    }
}