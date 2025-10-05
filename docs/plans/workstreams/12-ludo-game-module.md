---
id: 12
slug: ludo-game-module
name: "Ludo / Parcheesi Game Module"
status: planned
last_updated: 2025-09-30
owner: games
summary: >-
	Vanilla Ludo (Parcheesi variant): race track + home stretches, safe squares, entry rule (roll 6), capture reset,
	deterministic dice integration, and win on all tokens home. Variants provided via builder extensions.
acceptance:
	- Track + home stretches graph constructed deterministically.
	- Deterministic single d6 dice artifact.
	- Entry rule requiring 6 (no extra-turn baseline) enforced.
	- Capture returns opponent token to start unless on safe square.
	- Safe squares immune to capture.
	- Win when a player's all tokens reach home.
	- Tests: entry legality, capture vs safe square, blocked move ignored, exact finish roll, win detection.
open_followups:
	- Extra-turn on 6 & chained rolls.
	- Stacked token blocking & multi-capture variants.
	- Alternate dice configurations.
	- Cosmetic/animation hints (out of engine scope).
---

# Ludo Game Module (WS-LUDO-001)

## Goal

Deliver a deterministic race game showcasing dice events, conditional movement gating, capture reset semantics, and simple win detection.

## Current State Snapshot

Planned only.

## Success Criteria

- Same seed produces identical dice + movement acceptance sequence.
- Movement legality isolated in conditions (entry, blocked, safe square) — mutators stay pure.
- Capture logic allocation-light (no large per-move structures).

## Deliverables

1. Graph Builder (track + home stretches).
2. Dice Artifact & Roll Event.
3. Movement & Capture Mutators.
4. Safe / Entry / Block Conditions.
5. Win Detection Condition + terminal flag.
6. Tests (entry, capture, safe, finish, blocked, win).
7. Benchmarks (movement throughput, capture frequency simulation).
8. Docs (track topology, variants via builder).

## Risks

- Variant creep expanding baseline complexity.
- Over-generalizing home stretch modeling harming clarity/performance.

## Extension Strategy

Builder exposes toggles for: extra-turn on 6, stacked blocking, alternative dice count. Core baseline remains minimal & deterministic.

## Status Summary

Not started.

---
_End of workstream 12._
