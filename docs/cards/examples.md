---
slug: cards-examples
name: "Cards Module Examples"
last_updated: 2025-11-14
owner: core
summary: >-
  Practical examples demonstrating card module usage in different game scenarios.
---

# Cards Module Examples

This document provides practical examples of using the Cards module for different game types.

## Example 1: Simple War Card Game

A minimal implementation of the card game War, demonstrating basic draw and compare mechanics.

```csharp
using Veggerby.Boards.Cards;

// Setup
var builder = new CardsGameBuilder();
builder.WithSeed(42UL); // Deterministic shuffle
var progress = builder.Compile();

// Initialize deck
progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
var deck = builder.CreateInitialDeckEvent().Deck;

// Shuffle the deck
var shuffleEvt = new ShuffleDeckEvent(deck!, CardsGameBuilder.Piles.Draw);
progress = progress.HandleEvent(shuffleEvt);

// Deal cards to players (2 each for demo)
for (int i = 0; i < 2; i++)
{
    var drawEvt = new DrawCardsEvent(deck!, 
        CardsGameBuilder.Piles.Draw, 
        CardsGameBuilder.Piles.Hand, 
        1);
    progress = progress.HandleEvent(drawEvt);
}

// Inspect hands
var state = progress.State.GetState<DeckState>(deck!);
var handCards = state!.Piles[CardsGameBuilder.Piles.Hand];
Console.WriteLine($"Hand has {handCards.Count} cards");

// Play cards to in-play area
var moveEvt = new MoveCardsEvent(deck!, 
    CardsGameBuilder.Piles.Hand, 
    CardsGameBuilder.Piles.InPlay, 
    handCards);
progress = progress.HandleEvent(moveEvt);

// Winner takes all - move to discard
state = progress.State.GetState<DeckState>(deck!);
var playedCards = state!.Piles[CardsGameBuilder.Piles.InPlay].ToList();
var discardEvt = new DiscardCardsEvent(deck!, 
    CardsGameBuilder.Piles.Discard, 
    playedCards);
progress = progress.HandleEvent(discardEvt);
```

## Example 2: Deck-Building Integration (Dominion-style)

Demonstrates integration with the DeckBuilding module for supply-based card acquisition.

```csharp
using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;

// Setup deck-building game
var builder = new DeckBuildingGameBuilder();
builder.WithSeed(123UL);

// Configure supply with card definitions
var configurator = new DeckBuildingSupplyConfigurator(builder);
configurator
    .WithCard("copper", "Copper", cost: 0, vp: 0, supply: 60)
    .WithCard("silver", "Silver", cost: 3, vp: 0, supply: 40)
    .WithCard("gold", "Gold", cost: 6, vp: 0, supply: 30)
    .WithCard("estate", "Estate", cost: 2, vp: 1, supply: 24)
    .WithCard("duchy", "Duchy", cost: 5, vp: 3, supply: 12)
    .WithCard("province", "Province", cost: 8, vp: 6, supply: 12);

var progress = builder.CompileWithSupply(configurator);

// Draw cards from deck to hand
var deck = builder.GetDeck();
var drawEvt = new DrawWithReshuffleEvent(deck!, 5);
progress = progress.HandleEvent(drawEvt);

// Gain card from supply (deck-building specific event)
var gainEvt = new GainFromSupplyEvent(deck!, "silver", CardsGameBuilder.Piles.Discard);
progress = progress.HandleEvent(gainEvt);

// End turn - cleanup to discard
var cleanupEvt = new CleanupToDiscardEvent(deck!);
progress = progress.HandleEvent(cleanupEvt);

// Check state
var state = progress.State.GetState<DeckState>(deck!);
Console.WriteLine($"Discard has {state!.Piles[CardsGameBuilder.Piles.Discard].Count} cards");
```

## Example 3: Hidden Information Game (Poker Hand Management)

Shows peek and reveal mechanics for managing hidden and public card information.

```csharp
using Veggerby.Boards.Cards;

var builder = new CardsGameBuilder();
builder.WithSeed(789UL);
var progress = builder.Compile();

// Initialize and shuffle
progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
var deck = builder.CreateInitialDeckEvent().Deck;
progress = progress.HandleEvent(new ShuffleDeckEvent(deck!, CardsGameBuilder.Piles.Draw));

// Deal hole cards (private to player)
var dealEvt = new DrawCardsEvent(deck!, 
    CardsGameBuilder.Piles.Draw, 
    CardsGameBuilder.Piles.Hand, 
    2);
progress = progress.HandleEvent(dealEvt);

// Peek at top 3 cards (e.g., for decision making) - doesn't change state
var peekEvt = new PeekCardsEvent(deck!, CardsGameBuilder.Piles.Draw, 3);
progress = progress.HandleEvent(peekEvt);

var state = progress.State.GetState<DeckState>(deck!);
var topCards = state!.Piles[CardsGameBuilder.Piles.Draw].Take(3).ToList();
Console.WriteLine($"Top 3 cards: {string.Join(", ", topCards.Select(c => c.Id))}");

// Flop - reveal community cards
var flopEvt = new MoveCardsEvent(deck!, 
    CardsGameBuilder.Piles.Draw, 
    CardsGameBuilder.Piles.InPlay, 
    3);
progress = progress.HandleEvent(flopEvt);

state = progress.State.GetState<DeckState>(deck!);
var communityCards = state!.Piles[CardsGameBuilder.Piles.InPlay].ToList();

// Reveal community cards explicitly (marks as public info)
var revealEvt = new RevealCardsEvent(deck!, 
    CardsGameBuilder.Piles.InPlay, 
    communityCards);
progress = progress.HandleEvent(revealEvt);

// At showdown, reveal player hand
var handCards = state!.Piles[CardsGameBuilder.Piles.Hand].ToList();
var showdownEvt = new RevealCardsEvent(deck!, 
    CardsGameBuilder.Piles.Hand, 
    handCards);
progress = progress.HandleEvent(showdownEvt);
```

## Example 4: Reshuffle Mechanics (Continuous Play Loop)

Demonstrates automatic reshuffle when draw pile is empty for long-running games.

```csharp
using Veggerby.Boards.Cards;

var builder = new CardsGameBuilder();
builder.WithSeed(999UL);
var progress = builder.Compile();

// Initialize with small deck
progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
var deck = builder.CreateInitialDeckEvent().Deck;
progress = progress.HandleEvent(new ShuffleDeckEvent(deck!, CardsGameBuilder.Piles.Draw));

// Game loop - draw and discard
for (int round = 0; round < 3; round++)
{
    Console.WriteLine($"\n=== Round {round + 1} ===");
    
    // Draw hand (5 cards)
    var state = progress.State.GetState<DeckState>(deck!);
    var drawCount = Math.Min(5, state!.Piles[CardsGameBuilder.Piles.Draw].Count);
    
    if (drawCount > 0)
    {
        var drawEvt = new DrawCardsEvent(deck!, 
            CardsGameBuilder.Piles.Draw, 
            CardsGameBuilder.Piles.Hand, 
            drawCount);
        progress = progress.HandleEvent(drawEvt);
    }
    
    // Check if we need to reshuffle
    state = progress.State.GetState<DeckState>(deck!);
    if (state!.Piles[CardsGameBuilder.Piles.Draw].Count == 0 && 
        state!.Piles[CardsGameBuilder.Piles.Discard].Count > 0)
    {
        Console.WriteLine("Draw pile empty - reshuffling discard into draw");
        var reshuffleEvt = new ReshuffleEvent(deck!, 
            CardsGameBuilder.Piles.Discard, 
            CardsGameBuilder.Piles.Draw);
        progress = progress.HandleEvent(reshuffleEvt);
        
        // Draw remaining cards after reshuffle
        var remainingToDraw = 5 - drawCount;
        if (remainingToDraw > 0)
        {
            state = progress.State.GetState<DeckState>(deck!);
            drawCount = Math.Min(remainingToDraw, state!.Piles[CardsGameBuilder.Piles.Draw].Count);
            if (drawCount > 0)
            {
                drawEvt = new DrawCardsEvent(deck!, 
                    CardsGameBuilder.Piles.Draw, 
                    CardsGameBuilder.Piles.Hand, 
                    drawCount);
                progress = progress.HandleEvent(drawEvt);
            }
        }
    }
    
    // Play turn (simplified - just discard all)
    state = progress.State.GetState<DeckState>(deck!);
    var handCards = state!.Piles[CardsGameBuilder.Piles.Hand].ToList();
    if (handCards.Count > 0)
    {
        var discardEvt = new DiscardCardsEvent(deck!, 
            CardsGameBuilder.Piles.Discard, 
            handCards);
        progress = progress.HandleEvent(discardEvt);
    }
    
    // Print state
    state = progress.State.GetState<DeckState>(deck!);
    Console.WriteLine($"Draw: {state!.Piles[CardsGameBuilder.Piles.Draw].Count}, " +
                     $"Discard: {state!.Piles[CardsGameBuilder.Piles.Discard].Count}, " +
                     $"Hand: {state!.Piles[CardsGameBuilder.Piles.Hand].Count}");
}
```

## Common Patterns

### Checking Pile Counts

```csharp
var state = progress.State.GetState<DeckState>(deck);
var drawCount = state?.Piles[CardsGameBuilder.Piles.Draw].Count ?? 0;
```

### Conditional Reshuffle

```csharp
var state = progress.State.GetState<DeckState>(deck);
var needsReshuffle = state?.Piles[CardsGameBuilder.Piles.Draw].Count == 0;
if (needsReshuffle)
{
    var reshuffleEvt = new ReshuffleEvent(deck, 
        CardsGameBuilder.Piles.Discard, 
        CardsGameBuilder.Piles.Draw);
    progress = progress.HandleEvent(reshuffleEvt);
}
```

### Moving Specific Cards

```csharp
var state = progress.State.GetState<DeckState>(deck);
var selectedCards = state?.Piles[CardsGameBuilder.Piles.Hand]
    .Where(c => c.Id.StartsWith("copper"))
    .ToList();
if (selectedCards?.Count > 0)
{
    var moveEvt = new MoveCardsEvent(deck, 
        CardsGameBuilder.Piles.Hand, 
        CardsGameBuilder.Piles.InPlay, 
        selectedCards);
    progress = progress.HandleEvent(moveEvt);
}
```

## See Also

- [Cards Module Overview](index.md) - Core concepts and event catalog
- [API Reference](api-reference.md) - Complete API documentation
- [Deck-Building Module](/docs/deck-building.md) - Supply and acquisition mechanics
