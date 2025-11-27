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
                return ApplyGoToJail(engine, state, player, playerState);

            case MonopolyCardEffect.GetOutOfJailFree:
                return state.Next([playerState.WithGetOutOfJailCard(true)]);

            case MonopolyCardEffect.AdvanceToPosition:
                return ApplyMoveToPosition(engine, state, player, playerState, card.Value, collectGo: true);

            case MonopolyCardEffect.MoveToPosition:
                return ApplyMoveToPosition(engine, state, player, playerState, card.Value, collectGo: false);

            case MonopolyCardEffect.AdvanceToNearestRailroad:
                return ApplyAdvanceToNearestRailroad(engine, state, player, playerState);

            case MonopolyCardEffect.AdvanceToNearestUtility:
                return ApplyAdvanceToNearestUtility(engine, state, player, playerState);

            case MonopolyCardEffect.MoveForward:
                return ApplyMoveRelative(engine, state, player, playerState, card.Value);

            case MonopolyCardEffect.MoveBackward:
                return ApplyMoveRelative(engine, state, player, playerState, -card.Value);

            case MonopolyCardEffect.PropertyRepairs:
                return ApplyPropertyRepairs(state, player, playerState, card.Value, card.SecondaryValue);

            default:
                return state;
        }
    }

    private GameState ApplyGoToJail(
        GameEngine engine,
        GameState state,
        Player player,
        MonopolyPlayerState playerState)
    {
        // Move piece to jail (position 10)
        var piece = engine.Game.Artifacts.OfType<Piece>()
            .FirstOrDefault(p => string.Equals(p.Owner?.Id, player.Id, StringComparison.Ordinal));

        if (piece is null)
        {
            return state.Next([playerState.GoToJail()]);
        }

        var jailTileId = MonopolyBoardConfiguration.GetTileId(10);
        var jailTile = engine.Game.Board.GetTile(jailTileId);

        if (jailTile is null)
        {
            return state.Next([playerState.GoToJail()]);
        }

        var newPieceState = new PieceState(piece, jailTile);

        return state.Next([playerState.GoToJail(), newPieceState]);
    }

    private GameState ApplyMoveToPosition(
        GameEngine engine,
        GameState state,
        Player player,
        MonopolyPlayerState playerState,
        int targetPosition,
        bool collectGo)
    {
        var piece = engine.Game.Artifacts.OfType<Piece>()
            .FirstOrDefault(p => string.Equals(p.Owner?.Id, player.Id, StringComparison.Ordinal));

        if (piece is null)
        {
            return state;
        }

        var currentPieceState = state.GetState<PieceState>(piece);
        if (currentPieceState?.CurrentTile is null)
        {
            return state;
        }

        var currentPosition = MonopolyBoardConfiguration.GetPosition(currentPieceState.CurrentTile.Id);

        // Check if passing Go (target position is less than current, or moving to Go from any position)
        var passesGo = collectGo && (targetPosition < currentPosition || (targetPosition == 0 && currentPosition > 0));

        var targetTileId = MonopolyBoardConfiguration.GetTileId(targetPosition);
        var targetTile = engine.Game.Board.GetTile(targetTileId);

        if (targetTile is null)
        {
            return state;
        }

        var newPieceState = new PieceState(piece, targetTile);
        var updates = new List<IArtifactState> { newPieceState };

        if (passesGo)
        {
            updates.Add(playerState.AdjustCash(200));
        }

        return state.Next(updates);
    }

    private GameState ApplyAdvanceToNearestRailroad(
        GameEngine engine,
        GameState state,
        Player player,
        MonopolyPlayerState playerState)
    {
        var piece = engine.Game.Artifacts.OfType<Piece>()
            .FirstOrDefault(p => string.Equals(p.Owner?.Id, player.Id, StringComparison.Ordinal));

        if (piece is null)
        {
            return state;
        }

        var currentPieceState = state.GetState<PieceState>(piece);
        if (currentPieceState?.CurrentTile is null)
        {
            return state;
        }

        var currentPosition = MonopolyBoardConfiguration.GetPosition(currentPieceState.CurrentTile.Id);

        // Railroad positions: 5 (Reading), 15 (Pennsylvania), 25 (B&O), 35 (Short Line)
        var railroadPositions = new[] { 5, 15, 25, 35 };
        var nearestRailroad = FindNearestPosition(currentPosition, railroadPositions);

        return ApplyMoveToPosition(engine, state, player, playerState, nearestRailroad, collectGo: true);
    }

    private GameState ApplyAdvanceToNearestUtility(
        GameEngine engine,
        GameState state,
        Player player,
        MonopolyPlayerState playerState)
    {
        var piece = engine.Game.Artifacts.OfType<Piece>()
            .FirstOrDefault(p => string.Equals(p.Owner?.Id, player.Id, StringComparison.Ordinal));

        if (piece is null)
        {
            return state;
        }

        var currentPieceState = state.GetState<PieceState>(piece);
        if (currentPieceState?.CurrentTile is null)
        {
            return state;
        }

        var currentPosition = MonopolyBoardConfiguration.GetPosition(currentPieceState.CurrentTile.Id);

        // Utility positions: 12 (Electric Company), 28 (Water Works)
        var utilityPositions = new[] { 12, 28 };
        var nearestUtility = FindNearestPosition(currentPosition, utilityPositions);

        return ApplyMoveToPosition(engine, state, player, playerState, nearestUtility, collectGo: true);
    }

    private static int FindNearestPosition(int currentPosition, int[] targetPositions)
    {
        // Find the next position forward from current position
        foreach (var pos in targetPositions)
        {
            if (pos > currentPosition)
            {
                return pos;
            }
        }

        // Wrap around to the first position
        return targetPositions[0];
    }

    private GameState ApplyMoveRelative(
        GameEngine engine,
        GameState state,
        Player player,
        MonopolyPlayerState playerState,
        int spaces)
    {
        var piece = engine.Game.Artifacts.OfType<Piece>()
            .FirstOrDefault(p => string.Equals(p.Owner?.Id, player.Id, StringComparison.Ordinal));

        if (piece is null)
        {
            return state;
        }

        var currentPieceState = state.GetState<PieceState>(piece);
        if (currentPieceState?.CurrentTile is null)
        {
            return state;
        }

        var currentPosition = MonopolyBoardConfiguration.GetPosition(currentPieceState.CurrentTile.Id);
        var newPosition = (currentPosition + spaces + 40) % 40; // Handle negative values

        // Do not collect Go when moving backward
        var collectGo = spaces > 0 && newPosition < currentPosition;

        var targetTileId = MonopolyBoardConfiguration.GetTileId(newPosition);
        var targetTile = engine.Game.Board.GetTile(targetTileId);

        if (targetTile is null)
        {
            return state;
        }

        var newPieceState = new PieceState(piece, targetTile);
        var updates = new List<IArtifactState> { newPieceState };

        if (collectGo)
        {
            updates.Add(playerState.AdjustCash(200));
        }

        return state.Next(updates);
    }

    private GameState ApplyPropertyRepairs(
        GameState state,
        Player player,
        MonopolyPlayerState playerState,
        int houseCost,
        int hotelCost)
    {
        var ownership = state.GetExtras<PropertyOwnershipState>();
        if (ownership is null)
        {
            return state;
        }

        int totalCost = 0;

        // Count houses and hotels owned by the player
        foreach (var position in ownership.GetPropertiesOwnedBy(player.Id))
        {
            var houseCount = ownership.GetHouseCount(position);

            if (houseCount == PropertyOwnershipState.HotelValue)
            {
                totalCost += hotelCost;
            }
            else if (houseCount > 0)
            {
                totalCost += houseCount * houseCost;
            }
        }

        return state.Next([playerState.AdjustCash(-totalCost)]);
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
