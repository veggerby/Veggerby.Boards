using Veggerby.Boards.Cards;
using Veggerby.Boards.Cards.States;
using Veggerby.Boards.DeckBuilding.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;
namespace Veggerby.Boards.DeckBuilding.Rules;

/// <summary>
/// Validates that a draw can be performed possibly after reshuffling the discard into draw.
/// </summary>
public sealed class DrawWithReshuffleEventCondition : IGameEventCondition<DrawWithReshuffleEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, DrawWithReshuffleEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        var ds = state.GetState<DeckState>(@event.Deck);
        if (ds is null)
        {
            return ConditionResponse.NotApplicable;
        }
        if (@event.Count < 0)
        {
            return ConditionResponse.Fail("Negative count");
        }
        if (@event.Count == 0)
        {
            return ConditionResponse.Valid;
        }
        if (!ds.Piles.ContainsKey(DeckBuildingGameBuilder.Piles.Draw) ||
            !ds.Piles.ContainsKey(DeckBuildingGameBuilder.Piles.Discard) ||
            !ds.Piles.ContainsKey(DeckBuildingGameBuilder.Piles.Hand))
        {
            return ConditionResponse.Fail("Required piles missing");
        }
        var available = ds.Piles[DeckBuildingGameBuilder.Piles.Draw].Count + ds.Piles[DeckBuildingGameBuilder.Piles.Discard].Count;
        return available >= @event.Count ? ConditionResponse.Valid : ConditionResponse.Fail("Insufficient cards");
    }
}