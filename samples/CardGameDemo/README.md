# Card Game Demo Sample

This sample application demonstrates a simplified **Blackjack-style card game** using the Veggerby.Boards Cards module. It showcases all major card operations including shuffle, draw, peek, reveal, discard, and reshuffle mechanics.

## What This Demo Shows

- **Deterministic Shuffling**: Cards are shuffled using a seeded RNG for reproducible results
- **Complete Card Lifecycle**: Create, shuffle, draw, peek, reveal, discard, and reshuffle operations
- **State Inspection**: Demonstrates how to query deck state at various points
- **Reshuffle Mechanics**: Shows automatic discard-to-draw recycling for continuous play
- **Multi-Round Play**: Plays multiple rounds with automatic deck management

## Game Flow

The demo simulates a simplified Blackjack variant:

1. **Setup**: Initialize deck with 5 cards and shuffle
2. **Deal Phase**: Deal 2 cards to player hand
3. **Peek Phase**: Preview top 3 cards (decision making)
4. **Play Phase**: Draw additional cards or stand
5. **Reveal Phase**: Show player's hand
6. **Cleanup**: Discard all cards
7. **Reshuffle**: When draw pile is empty, move discard to draw and shuffle

## Running the Demo

```bash
dotnet run --project samples/CardGameDemo
```

## What You'll See

The program will:
1. Show the initial deck setup with deterministic shuffle
2. Play through 3 rounds of card operations
3. Display pile counts after each operation
4. Demonstrate peek without state change
5. Show reshuffle mechanics when draw pile empties
6. Prove determinism with identical results on re-run (same seed)

## Key Features Demonstrated

### Deterministic Shuffle
```csharp
builder.WithSeed(42UL);
var shuffleEvt = new ShuffleDeckEvent(deck, "draw");
```

### Peek (No State Change)
```csharp
var peekEvt = new PeekCardsEvent(deck, "draw", 3);
// State unchanged - can inspect without modifying
```

### Conditional Reshuffle
```csharp
if (drawPile.Count == 0 && discardPile.Count > 0)
{
    var reshuffleEvt = new ReshuffleEvent(deck, "discard", "draw");
}
```

This demonstrates that the Veggerby.Boards Cards module is fully functional and can handle complete card game scenarios with all supported operations.
