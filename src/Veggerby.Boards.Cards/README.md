# Veggerby.Boards.Cards

Deterministic cards and decks module for Veggerby.Boards. Provides card and deck artifacts, ordered piles, and events for create, shuffle, draw, move, and discard. Shuffles use the engine's RNG snapshot for full replay determinism.

> Depends on `Veggerby.Boards`. Use when you need card mechanics (decks, hands, discard piles) with immutable, event-driven semantics.

## Install

```bash
 dotnet add package Veggerby.Boards.Cards
```

## Scope

Adds:

- Artifacts: `Card`, `Deck` with named piles
- State: `DeckState` with ordered piles and optional supply counts
- Events: `CreateDeckEvent`, `ShuffleDeckEvent`, `DrawCardsEvent`, `MoveCardsEvent`, `DiscardCardsEvent`
- Builder: `CardsGameBuilder` wiring a minimal open phase and helper `CreateInitialDeckEvent()`

Not included: UI/AI, network, or game-specific victory logic.

## Quick Start

```csharp
using Veggerby.Boards.Cards;

// Build a minimal cards-capable game
var builder = new CardsGameBuilder();
builder.WithSeed(42UL); // optional: seed RNG for deterministic shuffle
var progress = builder.Compile();

// Initialize deck state (piles)
progress = progress.HandleEvent(builder.CreateInitialDeckEvent());

// Shuffle the draw pile deterministically
var deck = progress.Game.GetArtifact<Deck>("deck-1");
progress = progress.HandleEvent(new ShuffleDeckEvent(deck, CardsGameBuilder.Piles.Draw));

// Draw two cards to hand
progress = progress.HandleEvent(new DrawCardsEvent(deck, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 2));

// Inspect state
var ds = progress.State.GetState<DeckState>(deck);
var handCount = ds.Piles[CardsGameBuilder.Piles.Hand].Count; // 2
```

## Key Concepts (Cards Layer)

- Named piles: decks are composed of ordered piles (e.g., draw, hand, discard, in-play). Order is preserved; appends add to the end.
- Deterministic shuffle: uses `GameState.Random` (seed via `GameBuilder.WithSeed`) so the same inputs produce the same shuffled order.
- Immutable snapshots: events never mutate in place; rules gate validity and mutators return a new `GameState`.
- Declarative intent: events express “what” (draw 2 from draw → hand), rules ensure “can”, mutators implement “how”.

## Extending This Module

Examples:

- Multiple decks per game (register additional `Deck` artifacts)
- Additional piles (e.g., exile, tableau) by declaring them on the `Deck`
- Supply/gain semantics via a new event (peek, reveal, gain from supply)
- Reshuffle policy when draw is empty (explicit event to move discard → draw and shuffle)

Keep extensions pure and deterministic.

## Interop With Core

All transitions are immutable and deterministic. Cards logic is layered on core primitives (events, rules, phases) and does not introduce side effects.

## Versioning

Semantic versioning aligned with repository releases. Breaking behavior/API changes bump MAJOR.

## Contributing

Open issues & PRs at <https://github.com/veggerby/Veggerby.Boards>. Follow contribution guidelines and code style.

## License

MIT License. See root `LICENSE`.
