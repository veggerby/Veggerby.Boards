# Veggerby.Boards.DeckBuilding

Deck-building core scaffolding built on Veggerby.Boards and Veggerby.Boards.Cards. Provides deterministic supply and player zone foundations (Action/Buy/Cleanup phases to be wired in WS-17).

## Install

Package publishes with the rest of the suite. Until then, reference the project directly.

## Scope (MVP)

- CardDefinition metadata (name, types, cost, victory points)
- Deterministic supply concept built from Cards piles (future PR)
- Player zones (deck/hand/discard/in-play) atop Cards piles (future PR)
- Turn phases: Action → Buy → Cleanup (scaffolded)

## Determinism

All shuffles and draws are deterministic via the engine RNG. Seed with `GameBuilder.WithSeed(ulong)` to reproduce sequences.

## Status

Scaffold only in this commit. Follow Workstream 17 for incremental integration.
