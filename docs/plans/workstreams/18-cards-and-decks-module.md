---
id: 18
slug: cards-and-decks-module
name: "Cards & Decks Module"
status: done
last_updated: 2025-10-05
owner: core
summary: >-
  Deterministic cards/decks capability: card & deck artifacts with named piles, immutable DeckState, and events
  for create, shuffle, draw, move, and discard, wired via DecisionPlan. Shuffles use seeded RNG for reproducibility.
acceptance:
  - Card and Deck artifacts created via GameBuilder.
  - DeckState models ordered piles and optional supply counts, immutable snapshots.
  - Events implemented: CreateDeck, ShuffleDeck, DrawCards, MoveCards, DiscardCards.
  - Deterministic shuffle uses GameState.Random seeded via GameBuilder.WithSeed.
  - Rules/conditions gate invalid operations; mutators are pure and allocation-conscious.
  - Tests prove deterministic shuffle and basic flows; invalid operations rejected.
open_followups:
  - Add docs page under /docs/cards with quick start and determinism notes.
  - Optional: Peek/Reveal top-N, Gain from supply, Reshuffle-on-empty policy as explicit event.
  - Validate allocation profile for large piles; consider spans where beneficial without complexity.
---

# Cards & Decks Module (WS-CARDS-018)

## Goal

Provide a deterministic, reusable foundation for card-based games by introducing card/deck artifacts, ordered piles, and essential transitions (create, shuffle, draw, move, discard), aligned with engine immutability and determinism.

## Current State Snapshot

Delivered in `src/Veggerby.Boards.Cards` with builder wiring, events, conditions, mutators, and tests. README aligned with other modules.

## Outcomes

- Deterministic shuffle via `GameState.Random` with seed control.
- Explicit event + rules gating; pure mutators returning new `GameState` snapshots.
- Minimal builder (`CardsGameBuilder`) with helper `CreateInitialDeckEvent()`; invariant-satisfying topology and players.
- Tests cover create+draw, shuffle parity, and invalid draws.

## Next Steps

- Author module documentation under `/docs/cards`.
- Add optional events (peek/reveal, gain, reshuffle policy) as v1 extensions.
- Evaluate integration in Monopoly and Deck-building workstreams (14, 17).

## Risks & Considerations

- Avoid overcomplicating supply/zone mechanics (defer to Workstream 17).
- Keep mutators allocation-conscious; avoid LINQ in hot paths if extended.

---
_End of workstream 18._
