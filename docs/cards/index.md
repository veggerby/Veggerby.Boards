---
slug: cards
name: "Cards & Decks"
last_updated: 2025-11-14
owner: core
summary: >-
  Deterministic cards/decks capability for Veggerby.Boards with card/deck artifacts, immutable DeckState, and
  comprehensive events for create, shuffle, draw, move, discard, peek, reveal, and reshuffle operations.
  Shuffles are reproducible via seeded RNG.
---

# Cards & Decks

Deterministic cards and decks module for Veggerby.Boards. It provides:

- **Artifacts**: `Card`, `Deck` (with named piles)
- **State**: `DeckState` (ordered piles, optional supply counts)
- **Events**: `CreateDeckEvent`, `ShuffleDeckEvent`, `DrawCardsEvent`, `MoveCardsEvent`, `DiscardCardsEvent`, `PeekCardsEvent`, `RevealCardsEvent`, `ReshuffleEvent`
- **Builder**: `CardsGameBuilder` wiring a minimal open phase and helper `CreateInitialDeckEvent()`

**Determinism**: Shuffles use the engine's `GameState.Random`. Seeding via `GameBuilder.WithSeed(ulong)` yields fully reproducible results.

## Core Concepts

### Piles

Piles are ordered lists of cards identified by string names (e.g., "draw", "discard", "hand", "inplay"). Each `Deck` defines which piles it supports. The `DeckState` maintains the current cards in each pile as immutable snapshots.

### Shuffle Determinism

All shuffle operations (including `ReshuffleEvent`) use the Fisher-Yates algorithm with the engine's deterministic `GameState.Random`. Given the same seed and event sequence, card order is guaranteed to be identical across runs.

### Event Lifecycle

Events are declarative intentions that must pass validation conditions before their mutators produce new state snapshots. Invalid operations (e.g., drawing more cards than available) are rejected with `InvalidGameEventException`.

## Quick Start

```csharp
using Veggerby.Boards.Cards;

var builder = new CardsGameBuilder();
builder.WithSeed(42UL); // optional, for deterministic shuffle
var progress = builder.Compile();

// Initialize deck state (piles)
progress = progress.HandleEvent(builder.CreateInitialDeckEvent());

// Shuffle draw pile deterministically
var deck = progress.Game.GetArtifact<Deck>("deck-1");
progress = progress.HandleEvent(new ShuffleDeckEvent(deck, CardsGameBuilder.Piles.Draw));

// Draw two cards to hand
progress = progress.HandleEvent(new DrawCardsEvent(deck, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 2));

// Inspect state
var ds = progress.State.GetState<DeckState>(deck);
var handCount = ds.Piles[CardsGameBuilder.Piles.Hand].Count; // 2
```

## Event Catalog

All events are wired through the `CardsGameBuilder` and follow the engine's rule/phase gating pattern.

### CreateDeckEvent
Initialize a deck's piles (ordered lists of `Card`), with optional supply counts.

**Parameters**: `Deck`, `IDictionary<string, IList<Card>> Piles`, optional `IDictionary<string, int> Supply`

**Use case**: Initial deck setup at game start.

### ShuffleDeckEvent
Deterministically shuffles a specified pile using Fisher-Yates algorithm with `GameState.Random`.

**Parameters**: `Deck`, `string PileId`

**Use case**: Randomizing draw pile order.

### DrawCardsEvent
Draw N cards from a source pile and append to a destination pile.

**Parameters**: `Deck`, `string FromPileId`, `string ToPileId`, `int Count`

**Use case**: Drawing cards from deck to hand.

### MoveCardsEvent
Move by count (top-N) or explicit cards from source to destination, preserving order.

**Parameters**: `Deck`, `string FromPileId`, `string ToPileId`, `int Count` OR `IReadOnlyList<Card> Cards`

**Use case**: Playing cards from hand to in-play area.

### DiscardCardsEvent
Move specific cards (from any piles) to a destination pile (e.g., discard), preserving provided order.

**Parameters**: `Deck`, `string ToPileId`, `IReadOnlyList<Card> Cards`

**Use case**: Discarding selected cards from hand.

### PeekCardsEvent
View top N cards from a pile without removing them (read-only operation).

**Parameters**: `Deck`, `string PileId`, `int Count`

**Use case**: Deck inspection, scrying effects, hand selection preview.

**Note**: Does not modify state; optional visibility tracking could be added via extras.

### RevealCardsEvent
Make specific cards visible to all players (optional visibility tracking).

**Parameters**: `Deck`, `string PileId`, `IReadOnlyList<Card> Cards`

**Use case**: Showing cards in hand, mandatory reveals, shared information.

**Note**: Basic implementation is no-op; visibility tracking can be added via DeckState extras if needed.

### ReshuffleEvent
Move cards from source pile (typically discard) to destination pile (typically draw) and shuffle.

**Parameters**: `Deck`, `string FromPileId`, `string ToPileId`

**Use case**: Discard-to-deck recycling for continuous play (deck-building games, long sessions).

## Integration Guide

### Wiring Cards into Custom Modules

1. **Define your deck structure**: Create `Deck` artifact with required pile names
2. **Register artifacts**: Use `AddArtifact()` with factory for each `Card` and the `Deck`
3. **Add card phase**: Wire events through `AddGamePhase()` with conditions and mutators
4. **Initialize deck state**: Send `CreateDeckEvent` after `Compile()`

Example:
```csharp
var myPiles = new[] { "draw", "discard", "hand" };
var deck = new Deck("my-deck", myPiles);
AddArtifact(deck.Id).WithFactory(id => deck);

// Add cards...
var card1 = new Card("ace-of-spades");
AddArtifact(card1.Id).WithFactory(id => card1);

// Wire phase (or extend existing phase)
AddGamePhase("card-phase")
    .ForEvent<DrawCardsEvent>().If<DrawCardsEventCondition>().Then().Do<DrawCardsStateMutator>()
    // ... other events
```

### Best Practices

**When to use Peek vs Reveal vs Draw:**
- **Peek**: Temporary inspection without state change (scrying, previewing next cards)
- **Reveal**: Make information publicly visible (mandatory reveals, showing hand)
- **Draw**: Move cards to destination pile (actual card acquisition)

**Reshuffle vs Shuffle:**
- **Shuffle**: Randomize existing pile in-place
- **Reshuffle**: Move pile (typically discard) into another (typically draw) and shuffle combined cards

**Supply management:**
- For deck-building supply mechanics, see the `Veggerby.Boards.DeckBuilding` module which provides `GainFromSupplyEvent` and supply tracking

## Extending

- **Additional piles**: Declare custom pile names (e.g., "exile", "tableau", "revealed") when creating `Deck`
- **Visibility tracking**: Extend `DeckState` with extras dictionary to track revealed/visible cards
- **Auto-reshuffle policy**: Add condition checking draw pile emptiness and trigger `ReshuffleEvent` automatically
- **Card metadata**: Extend `Card` with properties (suit, rank, cost) or use separate metadata artifacts

## Notes

- Cards & Decks obey engine invariants: immutable snapshots, rules/phase gating, no hidden randomness
- Minimal board topology and two players are included in `CardsGameBuilder` to satisfy core invariants
- All shuffle operations are deterministic; use `WithSeed()` for reproducible randomness
- See `/docs/cards/examples.md` for complete game scenarios
- See `/docs/cards/api-reference.md` for full API documentation
