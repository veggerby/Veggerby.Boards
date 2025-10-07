---
id: 17
slug: deck-building-core
name: "Deck-building Core Module"
status: partial
last_updated: 2025-10-07
owner: games
summary: >-
  Dominion-like baseline: deterministic supply piles, player deck/discard/hand zones, draw/shuffle cycle with seeded RNG,
  action/treasure/buy phases sequencing, and win by total victory point card value at game end.
acceptance:
  - Supply piles constructed deterministically (configured card definitions + counts) with stable ordering.
  - Player zones (deck, hand, discard, in-play) modeled as immutable states with explicit transitions (draw, discard, gain, trash).
  - Deterministic draw: when deck exhausted, discard reshuffle uses seeded shuffle artifact producing reproducible order.
  - Turn phases enforced: Action → Buy → Cleanup baseline; out-of-sequence events rejected.
  - Victory points tallied deterministically at termination (no hidden state sources).
  - Tests: draw & reshuffle determinism, action play modifies state (single sample action), buy gain to discard, cleanup cycle, end-game scoring.
open_followups:
  - Attack / reaction card types.
  - Duration / reserve / night phase variants.
  - Trashing & cost-reduction mechanics expansion.
  - Multi-card action resolution ordering benchmarking.
  - Optimized zone indexing (avoiding repeated array copies).
---

# Deck-building Core Module (WS-DBLD-001)

## Goal

Deliver a deterministic foundation for deck-building games capturing zone transitions, phase sequencing, and reproducible shuffles without overcommitting to expansive card taxonomy.

## Current State Snapshot

Scaffolding in place:

- New project `Veggerby.Boards.DeckBuilding` added to the solution.
- `DeckBuildingGameBuilder` establishes minimal topology and players (no-op phases will be added alongside rules in subsequent changes).
- `CardDefinition` artifact introduced (name, types, cost, victory points) with XML docs.

Foundation to build on: `Veggerby.Boards.Cards` (artifacts, piles, deterministic shuffle, create/draw/move/discard).

## Success Criteria

- Shuffles reproducible with same seed across entire game history.
- No mutation of zone collections; transitions allocate minimal new arrays/spans.
- Phase enforcement conditions separate from card effect mutators (purity preserved).
- Card definitions static & metadata-driven (no runtime reflective behaviors).

## Deliverables

1. Card Definition Model (id, types, cost, victory value, effect hook placeholder).
2. Supply Pile Builder (ordered deterministic collection with counts).
3. Player Zone State (deck, hand, discard, in-play) + Draw / Discard / Gain / Trash Mutators.
4. Shuffle Artifact & Deterministic Shuffle Mutator (seeded RNG state captured explicitly).
5. Turn Phase Sequencer (Action, Buy, Cleanup) Conditions.
6. Scoring Aggregator (victory points sum) + Game End Condition (supply depletion threshold).
7. Tests (draw cycle determinism, play action modifies state, buy adds to discard, cleanup resets hand, scoring computation).
8. Benchmarks (shuffle throughput, draw cycle cost, zone transition overhead).
9. Documentation (zone lifecycle diagram, shuffle reproducibility, extension toggle examples).

## Risks

- Overengineering effect system early (keep placeholder minimal).
- Shuffle allocation overhead if naive copying each transition.
- Variant phase additions increasing complexity prematurely.

## Extension Strategy

Builder toggles / extension points: additional phases (Night, Duration), card type registries, attack/reaction modules, custom shuffle algorithms, cost modifiers, alternative end-game triggers.

## Status Summary

Initial scaffolding completed. Next up:

- Add supply model (counts per CardDefinition id) and builder wiring.
- Introduce player zone state and deterministic reshuffle-on-empty behavior.
- Wire Action/Buy/Cleanup phases with event-specific conditions and mutators (gain, trash, cleanup, reshuffle).
- Add MVP tests for gain from supply, reshuffle determinism, and cleanup cycle.

---
_End of workstream 17._
