---
id: 17
slug: deck-building-core
name: "Deck-building Core Module"
status: partial
last_updated: 2025-10-10
owner: games
summary: >-
  Dominion-like baseline: deterministic supply piles, player deck/discard/hand zones, draw/shuffle cycle with seeded RNG,
  action/buy/cleanup phases sequencing, deterministic victory point scoring, and win by total VP card value at game end.
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

Delivered:

- New project `Veggerby.Boards.DeckBuilding` with `DeckBuildingGameBuilder` (minimal topology/players) and `CardDefinition` artifact (name/types/cost/VP).
- Zone mechanics built atop `Veggerby.Boards.Cards` (piles, deterministic shuffle/draw, move/discard):
  - `CreateDeckEvent` initializes piles (+optional supply snapshot).
  - `GainFromSupplyEvent` decrements supply and appends to a target pile.
  - `DrawWithReshuffleEvent` reshuffles Discard deterministically into Draw when needed and draws to Hand.
  - `TrashFromHandEvent` removes specified cards from Hand.
  - `CleanupToDiscardEvent` moves all cards from Hand and InPlay to Discard.
- Tests cover gain from supply (happy/insufficient), reshuffle determinism, trash validation, cleanup behavior, scoring aggregation idempotency, and termination gating.
- Deterministic DecisionPlan baseline locked (ordered phase:event list + signature) with guard + diff test preventing accidental drift.
- Structural invariants test asserting presence of core events across phases.
- Feature flag guard + sequential test collection eliminated prior sequencing flag race flakiness.
- Action/Buy phase split completed: former unified main phase separated into `db-action` (draw, trash) and `db-buy` (gain) phases with updated baseline and invariants.
- Scoring + Termination delivered: `RegisterCardDefinitionEvent` & `CardDefinitionState`, `ComputeScoresEvent` & `ScoreState`, `EndGameEvent` & `GameEndedState` wired in cleanup; ordering invariant and baseline signature locked.
- Supply configurator scaffold: `DeckBuildingSupplyConfigurator` fluent helper enabling ordered card definition registration + supply counts and deterministic startup events (`RegisterCardDefinitionEvent`s then single `CreateDeckEvent`). Tests cover insertion ordering, duplicate definition rejection, undefined supply guard, and integration with `GainFromSupplyEvent`.
- Dedicated module documentation page (`docs/deck-building.md`) authored (phases table, zones, shuffling determinism, supply configurator usage, end-to-end example, error modes, extension points).

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
5. Turn Phase Sequencer (Action, Buy, Cleanup) Conditions. (Action/Buy split completed.)
6. Scoring Aggregator (victory points sum) + Game End Condition (supply depletion threshold).
7. Tests (draw cycle determinism, play action modifies state, buy adds to discard, cleanup resets hand, scoring computation).
8. Benchmarks (shuffle throughput, draw cycle cost, zone transition overhead).
9. Documentation (zone lifecycle diagram, shuffle reproducibility, extension toggle examples).

## Risks

- Overengineering effect system early (keep placeholder minimal).
- Shuffle allocation overhead if naive copying each transition.
- Variant phase additions increasing complexity prematurely.
- Baseline regeneration discipline required during phase expansion (avoid ad-hoc modifications bypassing signature test update).

## Extension Strategy

Builder toggles / extension points: additional phases (Night, Duration), card type registries, attack/reaction modules, custom shuffle algorithms, cost modifiers, alternative end-game triggers.

## Status Summary

Core zone mechanics, Action/Buy phase split, deterministic baseline/invariant hardening, scoring & termination, supply configurator scaffold, and dedicated module docs page landed. Next up:

- Bulk card registration convenience method (batch add) – optional (evaluate demand; current fluent per-card API deterministic).
- Benchmarks (shuffle throughput, draw cycle cost, zone transition overhead, scoring cost).
- Alternate end-game trigger (e.g., supply depletion threshold) + invariant tests.

---
_End of workstream 17._
