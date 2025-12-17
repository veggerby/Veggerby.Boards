---
id: 13
slug: checkers-game-module
name: "Checkers / Draughts Game Module"
status: partial
last_updated: 2025-12-16
owner: games
summary: >-
	Vanilla English/American Checkers: 8x8 dark-squares graph, forward diagonal moves, mandatory capture chain,
	kinging on back rank, deterministic multi-jump resolution ordering, and win on opponent immobilized or eliminated.
acceptance:
	- Dark-square board graph (32 playable tiles) generated deterministically. ✅ DONE
	- Initial piece placement (12 per side) correct and reproducible. ✅ DONE
	- Forward-only movement for men; bidirectional for kings enforced by conditions. ✅ DONE
	- Mandatory capture rule applied (non-capturing move rejected if capture exists). ⚠️ PARTIAL (scaffolded but not enforced)
	- Multi-jump capture sequence resolution deterministic (stable ordering of branching paths). ⚠️ PARTIAL (single jumps work, multi-jump >2 relations not handled)
	- Kinging mutator on back rank creates new king piece state (no mutation of existing piece artifact). ✅ DONE
	- Win detection when opponent has zero pieces OR no legal moves. ⚠️ PARTIAL (zero pieces works, no legal moves detection not implemented)
	- Tests: simple move, forced capture rejection, single capture, multi-jump chain, kinging, stalemate/no-move win. ⚠️ PARTIAL (28 tests pass, some are placeholders)
open_followups:
	- Complete mandatory capture enforcement logic
	- Implement no-legal-moves detection for endgame
	- Handle multi-jump sequences beyond 2 relations
	- Draw rules (repetition / move-count) tracking
	- Variants: flying kings, international (10x10), huffing rule toggle
	- Performance optimization for large branching capture searches
	- Heuristic ordering strategies (outside core; maybe benchmark only)
---

# Checkers Game Module (WS-CHK-001)

## Goal

Provide a deterministic implementation of standard Checkers emphasizing capture-chain legality resolution and immutability-friendly kinging, showcasing path enumeration without side effects.

## Current State Snapshot

**Status: Partially Complete (2025-12-16)**

The Checkers module is implemented and functional for basic gameplay, with 28 passing tests. However, several core features remain incomplete:

**Completed Features:**
- 8×8 dark-square board topology (32 playable tiles)
- Piece movement (forward-only for regular pieces, all directions for kings)
- King promotion on back rank
- Single-jump capture mechanics
- Zero-pieces endgame detection
- Basic game flow and turn rotation

**Incomplete Features (documented in README Known Limitations):**
- Mandatory capture enforcement (currently allows all moves)
- No-legal-moves endgame detection
- Multi-jump sequences beyond 2 relations

**Test Coverage:**
- 28/28 tests passing
- Some tests are placeholder implementations for incomplete features

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

**Partial Implementation (60% Complete)**

The module is functional for basic checkers gameplay but lacks full mandatory capture enforcement and comprehensive endgame detection. All incomplete features are documented in the README Known Limitations section.

---
_End of workstream 13._
