using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that validates a draw card event.
/// </summary>
public class CanDrawCardCondition : IGameEventCondition<DrawCardGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, DrawCardGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Check if the cards state exists
        var cardsState = state.GetExtras<MonopolyCardsState>();
        if (cardsState is null)
        {
            return ConditionResponse.Ignore("Cards state not found");
        }

        // Check if the deck exists
        var deckState = cardsState.GetDeck(@event.DeckId);
        if (deckState is null)
        {
            return ConditionResponse.Ignore($"Deck '{@event.DeckId}' not found");
        }

        // Check if player is bankrupt
        var playerState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => ps.Player.Equals(@event.Player));

        if (playerState is not null && playerState.IsBankrupt)
        {
            return ConditionResponse.Fail("Bankrupt players cannot draw cards");
        }

        return ConditionResponse.Valid;
    }
}
