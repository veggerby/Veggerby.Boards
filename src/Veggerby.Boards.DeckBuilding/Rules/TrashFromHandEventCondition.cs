using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Ensures all specified cards are present in the Hand pile for trashing.
/// </summary>
public sealed class TrashFromHandEventCondition : IGameEventCondition<TrashFromHandEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, TrashFromHandEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
        {
            return ConditionResponse.NotApplicable;
        }
        if (!ds.Piles.ContainsKey(DeckBuildingGameBuilder.Piles.Hand))
        {
            return ConditionResponse.Fail("Hand pile missing");
        }
        var hand = ds.Piles[DeckBuildingGameBuilder.Piles.Hand];
        foreach (var c in @event.Cards)
        {
            var present = false;
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i].Equals(c))
                {
                    present = true;
                    break;
                }
            }
            if (!present)
            {
                return ConditionResponse.Fail("Card not in hand");
            }
        }
        return ConditionResponse.Valid;
    }
}