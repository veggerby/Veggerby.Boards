---
id: 14
slug: monopoly-game-module
name: "Monopoly Game Module"
status: planned
last_updated: 2025-09-30
owner: games
summary: >-
	Vanilla Monopoly core loop: deterministic board track graph, property acquisition, rent payments, simple jail logic,
	deterministic dice integration (pair of d6), and win by bankrupting all opponents; house/hotel builds excluded initially.
acceptance:
	- Linear cyclical board track (40 spaces) constructed deterministically (Go, Properties, Chance, Community Chest, Jail, etc.).
	- Two deterministic d6 dice artifacts (captured in DiceState) with doubles tracking (3 doubles -> jail) logic.
	- Property purchase & ownership state transitions (no auctions in baseline: unowned skipped if insufficient cash).
	- Rent payment transfers cash; bankrupt player removed from turn order with asset liquidation simplified (properties revert to bank baseline).
	- Jail state + leave mechanisms: pay fixed fee next turn OR roll doubles within 3 attempts.
	- Chance / Community Chest event deck deterministic order (seeded shuffle) with effect subset (move, gain/pay cash, jail send, get out of jail card tracked).
	- Win detection when only one solvent player remains.
	- Tests: passing Go credit, rent payment, doubles jail send, jail exit via doubles, deck draw determinism, bankruptcy elimination, win condition.
open_followups:
	- Auctions, houses/hotels, mortgages.
	- Trading between players.
	- Full card set coverage + configurable rule variants.
	- Luxury/income tax formula variations.
	- Speed die / house rules.
---

# Monopoly Game Module (WS-MPLY-001)

## Goal

Provide a deterministic baseline economic track game emphasizing stateful ownership, cash flow events, and limited random sources (dice + deck) modeled via explicit artifacts and reproducible seeds.

## Current State Snapshot

Planned only.

## Success Criteria

- Dice + deck sequences reproducible under identical seed.
- Ownership & cash changes expressed only via explicit mutators (no hidden side effects within event evaluation).
- Bankruptcy resolution allocates minimal intermediate objects.
- Turn flow integration of doubles + jail logic remains deterministic and pure.

## Deliverables

1. Board Track Graph (ordered nodes with wrap-around semantics).
2. Dice Pair Artifacts + Roll Event (captures doubles + count toward jail rule).
3. Deck Artifacts (Chance, Community Chest) + Draw Event + effect resolution mutators.
4. Property Ownership & Rent Mutators (purchase, rent payment, bankruptcy elimination simplification).
5. Jail State Handling (enter, attempt exit, pay to leave) conditions + mutators.
6. Turn Progression Conditions (handle consecutive doubles, jail attempt sequencing).
7. Win Detection Condition (last solvent player) with terminal state.
8. Tests (rent, pass Go, doubles to jail, jail exit, bankruptcy, deck determinism, win).
9. Benchmarks (rent payment throughput, deck draw overhead, turn sequencing overhead with dice/doubles logic).
10. Documentation (track layout, simplified economics, extension toggles).

## Risks

- Economic complexity creep (adding houses/hotels too early) obscuring baseline clarity.
- Deck effect scope expansion increasing mutator surface unnecessarily.
- Performance overhead in repeated dice + deck events without pooling (must stay lean).

## Extension Strategy

Builder toggles for: auctions, houses/hotels build rules, mortgages, trading, tax variants, speed die, full card sets, configurable salary on Go, custom starting cash. Baseline keeps only core deterministic essentials.

## Status Summary

Not started.

---
_End of workstream 14._
