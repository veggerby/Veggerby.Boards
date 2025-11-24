using System;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

using Veggerby.Boards.DeckBuilding.Events;
using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.States;
namespace Veggerby.Boards.DeckBuilding.Rules;

/// <summary>
/// Condition gating gain-from-supply operations: requires DeckState present, target pile exists, and supply contains the card id.
/// </summary>
public sealed class GainFromSupplyEventCondition : IGameEventCondition<GainFromSupplyEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, GainFromSupplyEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var deckState = state.GetState<DeckState>(@event.Deck);
        if (deckState is null)
        {
            return ConditionResponse.NotApplicable;
        }

        if (!deckState.Piles.ContainsKey(@event.TargetPileId))
        {
            return ConditionResponse.Fail("Unknown pile");
        }

        if (!deckState.Supply.TryGetValue(@event.CardId, out var count) || count <= 0)
        {
            return ConditionResponse.Fail("Insufficient supply");
        }

        // Ensure the card artifact exists in the game
        var card = engine.Game.GetArtifact<Card>(@event.CardId);
        if (card is null)
        {
            return ConditionResponse.Fail("Unknown card id");
        }

        return ConditionResponse.Valid;
    }
}