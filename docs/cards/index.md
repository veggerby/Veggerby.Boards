---
slug: cards
name: "Cards & Decks"
last_updated: 2025-10-07
owner: core
summary: >-
  Deterministic cards/decks capability for Veggerby.Boards with card/deck artifacts, immutable DeckState, and
  events for create, shuffle, draw, move, and discard. Shuffles are reproducible via seeded RNG.
---

# Cards & Decks

Deterministic cards and decks module for Veggerby.Boards. It provides:

- Artifacts: `Card`, `Deck` (with named piles)
- State: `DeckState` (ordered piles, optional supply counts)
- Events: `CreateDeckEvent`, `ShuffleDeckEvent`, `DrawCardsEvent`, `MoveCardsEvent`, `DiscardCardsEvent`
- Builder: `CardsGameBuilder` wiring a minimal open phase and helper `CreateInitialDeckEvent()`

Determinism: Shuffles use the engine's `GameState.Random`. Seeding via `GameBuilder.WithSeed(ulong)` yields fully reproducible results.

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

## Event Glossary

- CreateDeckEvent: Initialize a deck's piles (ordered lists of `Card`), with optional supply counts.
- ShuffleDeckEvent: Deterministically shuffles a specified pile (Fisher–Yates using `GameState.Random`).
- DrawCardsEvent: Draw N cards from a source pile and append to a destination pile.
- MoveCardsEvent: Move by count (top-N) or explicit cards from source to destination, preserving order.
- DiscardCardsEvent: Move specific cards (from any piles) to a destination pile (e.g., discard), preserving provided order.

## Deterministic Shuffle

- Driven by `GameState.Random`; identical seed and history → identical results.
- If a pile has ≤ 1 card, shuffle is a no-op.

## Extending

- Additional piles (e.g., exile, tableau) by declaring them on the `Deck`.
- Supply/gain semantics via a new event (peek/reveal/gain from supply).
- Reshuffle policy when draw is empty (event to move discard → draw + shuffle).

## Notes

- Cards & Decks obey engine invariants: immutable snapshots, rules/phase gating, no hidden randomness.
- Minimal board topology and two players are included in `CardsGameBuilder` to satisfy core invariants.
