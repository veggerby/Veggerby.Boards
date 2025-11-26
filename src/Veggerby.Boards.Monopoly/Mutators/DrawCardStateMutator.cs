using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Cards;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that processes drawing a card and applying its effect.
/// </summary>
public class DrawCardStateMutator : IStateMutator<DrawCardGameEvent>
{
    private readonly int _shuffleSeed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DrawCardStateMutator"/> class.
    /// </summary>
    /// <param name="shuffleSeed">Seed for deterministic shuffling when reshuffling is needed.</param>
    public DrawCardStateMutator(int shuffleSeed = 42)
    {
        _shuffleSeed = shuffleSeed;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, DrawCardGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Get the cards state
        var cardsState = state.GetExtras<MonopolyCardsState>();
        if (cardsState is null)
        {
            return state;
        }

        // Get the specific deck
        var deckState = cardsState.GetDeck(@event.DeckId);
        if (deckState is null)
        {
            return state;
        }

        // Check if we need to reshuffle
        if (deckState.DrawPile.Count == 0)
        {
            deckState = deckState.Reshuffle(_shuffleSeed);
        }

        // Draw the card
        var (newDeckState, drawnCard) = deckState.DrawCard();

        if (drawnCard is null)
        {
            return state;
        }

        // Apply the card effect
        var playerState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => ps.Player.Equals(@event.Player));

        if (playerState is null)
        {
            return state;
        }

        // Update the cards state
        var newCardsState = cardsState.WithUpdatedDeck(newDeckState);
        var newState = state.ReplaceExtras(newCardsState);
        newState = ApplyCardEffect(engine, newState, @event.Player, playerState, drawnCard);

        // Handle Get Out of Jail Free card - keep it with the player
        if (drawnCard.Effect == MonopolyCardEffect.GetOutOfJailFree)
        {
            var currentCardsState = newState.GetExtras<MonopolyCardsState>();
            if (currentCardsState is not null)
            {
                var currentDeck = currentCardsState.GetDeck(@event.DeckId);
                if (currentDeck is not null)
                {
                    var updatedDeck = currentDeck.RemoveGetOutOfJailCard();
                    var updatedCardsState = currentCardsState.WithUpdatedDeck(updatedDeck);
                    newState = newState.ReplaceExtras(updatedCardsState);
                }
            }
        }

        return newState;
    }

    private GameState ApplyCardEffect(
        GameEngine engine,
        GameState state,
        Player player,
        MonopolyPlayerState playerState,
        MonopolyCardDefinition card)
    {
        switch (card.Effect)
        {
            case MonopolyCardEffect.CollectFromBank:
                return state.Next([playerState.AdjustCash(card.Value)]);

            case MonopolyCardEffect.PayToBank:
                return state.Next([playerState.AdjustCash(-card.Value)]);

            case MonopolyCardEffect.CollectFromPlayers:
                return ApplyCollectFromPlayers(state, player, playerState, card.Value);

            case MonopolyCardEffect.PayToPlayers:
                return ApplyPayToPlayers(state, player, playerState, card.Value);

            case MonopolyCardEffect.GoToJail:
                return state.Next([playerState.GoToJail()]);

            case MonopolyCardEffect.GetOutOfJailFree:
                return state.Next([playerState.WithGetOutOfJailCard(true)]);

            case MonopolyCardEffect.AdvanceToPosition:
            case MonopolyCardEffect.MoveToPosition:
            case MonopolyCardEffect.AdvanceToNearestRailroad:
            case MonopolyCardEffect.AdvanceToNearestUtility:
            case MonopolyCardEffect.MoveForward:
            case MonopolyCardEffect.MoveBackward:
                // Movement effects require additional game logic
                // and are handled by the game flow (not the mutator directly)
                // For now, store the pending movement in a separate state
                return state;

            case MonopolyCardEffect.PropertyRepairs:
                // Deferred - no houses/hotels implemented yet
                return state;

            default:
                return state;
        }
    }

    private GameState ApplyCollectFromPlayers(
        GameState state,
        Player collector,
        MonopolyPlayerState collectorState,
        int amountPerPlayer)
    {
        var playerStates = state.GetStates<MonopolyPlayerState>().ToList();
        var otherPlayers = playerStates.Where(ps => !ps.Player.Equals(collector) && !ps.IsBankrupt);

        int totalCollected = 0;
        var updates = new List<IArtifactState>();

        foreach (var otherPlayerState in otherPlayers)
        {
            updates.Add(otherPlayerState.AdjustCash(-amountPerPlayer));
            totalCollected += amountPerPlayer;
        }

        updates.Add(collectorState.AdjustCash(totalCollected));
        return state.Next(updates);
    }

    private GameState ApplyPayToPlayers(
        GameState state,
        Player payer,
        MonopolyPlayerState payerState,
        int amountPerPlayer)
    {
        var playerStates = state.GetStates<MonopolyPlayerState>().ToList();
        var otherPlayers = playerStates.Where(ps => !ps.Player.Equals(payer) && !ps.IsBankrupt).ToList();

        int totalPaid = amountPerPlayer * otherPlayers.Count;
        var updates = new List<IArtifactState>();

        foreach (var otherPlayerState in otherPlayers)
        {
            updates.Add(otherPlayerState.AdjustCash(amountPerPlayer));
        }

        updates.Add(payerState.AdjustCash(-totalPaid));
        return state.Next(updates);
    }
}
