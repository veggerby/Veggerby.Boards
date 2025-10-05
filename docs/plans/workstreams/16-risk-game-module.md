---
id: 16
slug: risk-game-module
name: "Risk Game Module"
status: planned
last_updated: 2025-09-30
owner: games
summary: >-
  Vanilla Risk: fixed world territory graph, deterministic reinforcement calculation (territory + continent bonuses),
  attack/defend dice resolution with ordered comparisons, occupation & elimination handling, and win by world domination.
acceptance:
  - Territory adjacency graph built deterministically (canonical ordering and labeling) with continent grouping metadata.
  - Reinforcement calculation: max(3, territories/3) + continent bonuses where player controls all member territories.
  - Attack dice (up to 3) vs defend dice (up to 2) resolution: roll ordering & pairwise highest comparisons deterministic (stable sort of ties).
  - Territory conquest transfers ownership and moves at least one (configurable baseline: all but one attacking dice losses) unit into captured territory.
  - Player elimination when no territories remain; cards/assets (if modeled) deferred from baseline.
  - Win detection when single player controls all territories.
  - Tests: reinforcement minimum, continent bonus, attack resolution single/multi dice, conquest ownership transfer, elimination, win detection.
open_followups:
  - Card set & turn-in bonus scaling.
  - Fog of war / hidden armies (out of baseline scope).
  - Alternate maps and dynamic map loader.
  - Fortification / move phase variants.
  - AI strategy hooks (external to core).
---

# Risk Game Module (WS-RISK-001)

## Goal

Deliver a deterministic territory control and combat module emphasizing reproducible dice resolution and transparent reinforcement computation without premature card or variant complexity.

## Current State Snapshot

Planned only.

## Success Criteria

- Reinforcement calculation free of floating arithmetic (integer safe operations only).
- Dice resolution uses pre-sized arrays / stack spans (no LINQ) for speed.
- Combat outcome independent of iteration order aside from deterministic die sort.
- Ownership transitions create new state snapshots without mutating prior territory states.

## Deliverables

1. Territory Graph Builder (nodes + adjacency + continent metadata).
2. Reinforcement Calculator Condition (baseline: territories/3 rounded down with min 3 + continent bonus table).
3. Dice Artifacts (d6 pool) + Combat Resolution Event & Mutator (attack vs defend comparison ordering).
4. Ownership Transfer Mutator (post-conquest movement rule baseline: move number of dice used to win or configurable constant).
5. Player Elimination & Win Detection Conditions.
6. Tests (reinforcement min, continent bonus, combat 1v1 / 3v2 cases, conquest transfer, elimination, world domination win).
7. Benchmarks (combat resolution throughput, reinforcement calculation cost).
8. Documentation (map diagram, reinforcement formula, extension hooks).

## Risks

- Over-scoping with cards early increases surface area.
- Combat resolution branch complexity harming clarity.
- Map generalization premature (focus on canonical map first).

## Extension Strategy

Builder toggles for: card mechanics, escalating trade bonus schedules, alternate maps, fortification phase variants, fog-of-war modules, custom reinforcement formulas.

## Status Summary

Not started.

---
_End of workstream 16._
