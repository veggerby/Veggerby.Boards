---
id: 13
slug: checkers-game-module
name: "Checkers / Draughts Game Module"
status: planned
last_updated: 2025-09-30
owner: games
summary: >-
	Vanilla English/American Checkers: 8x8 dark-squares graph, forward diagonal moves, mandatory capture chain,
	kinging on back rank, deterministic multi-jump resolution ordering, and win on opponent immobilized or eliminated.
acceptance:
	- Dark-square board graph (32 playable tiles) generated deterministically.
	- Initial piece placement (12 per side) correct and reproducible.
	- Forward-only movement for men; bidirectional for kings enforced by conditions.
	- Mandatory capture rule applied (non-capturing move rejected if capture exists).
	- Multi-jump capture sequence resolution deterministic (stable ordering of branching paths).
	- Kinging mutator on back rank creates new king piece state (no mutation of existing piece artifact).
	- Win detection when opponent has zero pieces OR no legal moves.
	- Tests: simple move, forced capture rejection, single capture, multi-jump chain, kinging, stalemate/no-move win.
open_followups:
	- Draw rules (repetition / move-count) tracking.
	- Variants: flying kings, international (10x10), huffing rule toggle.
	- Performance optimization for large branching capture searches.
	- Heuristic ordering strategies (outside core; maybe benchmark only).
---

# Checkers Game Module (WS-CHK-001)

## Goal

Provide a deterministic implementation of standard Checkers emphasizing capture-chain legality resolution and immutability-friendly kinging, showcasing path enumeration without side effects.

## Current State Snapshot

Planned only.

## Success Criteria

- Capture availability checking allocation-light (single pass generation + reuse of path structures where feasible).
- No LINQ in hot capture chain enumeration loops.
- Deterministic ordering: identical state yields identical preferred multi-jump path choice.
- Kinging implemented via new state snapshot, not mutation in place.

## Deliverables

1. Board Graph Builder (8x8 dark-square connectivity, diagonal edges only).
2. Piece Artifacts (men, kings identified via metadata or role flag) + Kinging Mutator.
3. Movement & Capture Events (Move, CaptureSequence) + associated pure mutators.
4. Capture Chain Resolver (enumerates all legal multi-jump sequences, deterministic ordering).
5. Conditions: ForwardMoveCondition, MandatoryCaptureCondition, KingMoveCondition, NoMoveLossCondition.
6. Win Detection Condition (elimination or immobilization) with terminal state flag.
7. Tests (baseline + edge cases: forced capture, multi-jump branching, kinging, immobilization win).
8. Benchmarks (capture chain enumeration throughput, branching factor stress).
9. Documentation (graph layout diagram, variant extension guidance).

## Risks

- Over-complicating path enumeration harming clarity.
- Variant divergence (international rules) creeping into baseline.
- Performance regressions if capture search allocates excessively.

## Extension Strategy

Builder toggles / extensions for: flying kings, optional huffing rule, variant board sizes (10x10), multi-capture ordering strategies, draw rule modules (repetition / 40-move), heuristic move scoring (external consumer, not core).

## Status Summary

Not started.

---
_End of workstream 13._
