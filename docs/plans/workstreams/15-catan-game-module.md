---
id: 15
slug: catan-game-module
name: "Settlers of Catan Game Module"
status: planned
last_updated: 2025-09-30
owner: games
summary: >-
  Implement vanilla Settlers of Catan: hex map generation (randomized terrain + number tokens), resource production via dice,
  trading (player & bank), building (roads, settlements, cities), longest road / largest army awards, robber movement, and victory detection.
acceptance:
  - Deterministic hex layout builder with seeded shuffle for terrain + numbered chits (omitting expansions, ports baseline only).
  - Resource production on 2d6 roll distributes correct resources to owning settlements/cities (city double production).
  - Build costs enforced (roads, settlements adjacency & distance rule, city upgrade replacing settlement) with pure mutators.
  - Player trade offer + acceptance model and bank maritime trade (4:1 baseline) supported.
  - Robber movement & blocking: move robber to tile, steal single random (deterministic selection via RNG) resource from eligible player.
  - Longest road & largest army awards tracked and re-assigned deterministically.
  - Victory detection at 10 points including awards.
  - Tests: road continuity, settlement distance rule enforcement, resource distribution parity, robber steal, award reassignment, victory trigger.
open_followups:
  - Port ratios (3:1, 2:1) & placement nuance.
  - Development card deck (knight, year of plenty, monopoly, road building) and largest army point integration.
  - Expanded seafarers / cities & knights rules (out of scope initial).
---

# Settlers of Catan Game Module (WS-CATAN-001)

## Goal

Provide a deterministic vanilla Catan base game focusing on core production, building, and award logic.

## Current State Snapshot

Planned only.

## Success Criteria

- Seeded map + number token generation reproducible.
- Resource production stable; same dice series => identical resource flows.
- Award reassignment (longest road) efficient (incremental rather than full recompute where feasible).

## Deliverables

1. Hex Map & Port Builder (seeded generator).
2. Resource Production Mutator (post dice roll event).
3. Building Mutators & Validation Conditions (roads, settlements, cities).
4. Robber Movement & Steal Event.
5. Trade Offer / Acceptance Events (player ↔ player, player ↔ bank 4:1).
6. Award Tracking Service (longest road / largest army placeholders; largest army pending dev card implementation).
7. Victory Detection Condition.
8. Tests (production, building validity, distance rule, robber steal, longest road transitions, victory case).
9. Benchmarks (resource distribution at scale, longest road recalculation stress).
10. Docs (map generation, building rules, award invariants, extension hooks).

## Risks

- Longest road detection complexity/performance.
- Trade model over-engineering (keep minimal events + acceptance handshake).
- Robber randomness requiring deterministic tie-breaking.

## Extension Strategy

Builder exposes hooks to swap map generator, introduce dev card deck, advanced ports, or expansion modules (Seafarers, Cities & Knights) without modifying core mutators.

## Status Summary

Not started.

---
_End of workstream 15._
