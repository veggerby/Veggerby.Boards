---
id: 11
slug: go-game-module
name: "Go Game Module"
status: done
last_updated: 2025-11-13
owner: go
summary: >-
  Complete Go (Weiqi, Baduk) module implementation with board topology, stone placement, pass turns, capture/ko/liberty rules,
  scoring (area), and end-of-game detection while preserving engine determinism & immutability.
acceptance:
  - ✅ Board builder supports 9x9 / 13x13 / 19x19 sizes with orthogonal liberty relations.
  - ✅ Stone placement enforces emptiness, suicide prohibition (unless capturing), and simple ko rule.
  - ✅ Capture resolution removes opponent groups with zero liberties.
  - ✅ Consecutive pass rule ends game after two passes (terminal state flagged).
  - ✅ Scoring routine (area mode) computes territory + captures deterministically with tests.
  - ✅ Nomenclature renders placements in standard coordinate form, pass as 'pass'.
  - ✅ Comprehensive tests: capture chains, suicide rejection, multi-group capture, ko rule validation, end-on-two-passes (29/29 passing, 100% success rate).
open_followups:
  - Territory vs area scoring mode toggle.
  - Handicap stone placement helper.
  - Japanese vs Chinese rules nuance (superko).
  - Dead group adjudication workflow.
  - Ko and snapback test pattern refinement.
---

# Go Game Module (WS-GO-001)

## Goal

Provide a deterministic, testable Go implementation leveraging the existing engine primitives (immutable state snapshots, explicit events, mutators) while ensuring capture & ko logic remains pure and auditable.

## Current State Snapshot

**COMPLETED (2025-11-13)**: Full Go implementation now functional and playable!

- ✅ `GoGameBuilder` - Board topology for all standard sizes (9x9, 13x13, 19x19)
- ✅ `GroupScanner` - Iterative flood-fill algorithm for group + liberty detection
- ✅ `PlaceStoneStateMutator` - Complete capture logic, suicide rule, ko detection
- ✅ `PassTurnStateMutator` - Pass counting and double-pass game termination
- ✅ `GoScoring` - Area scoring algorithm (stones + territory)
- ✅ `GameEndedState` - Terminal state marker
- ✅ `GoNomenclature` - Standard coordinate notation

**Test Coverage**: 29/29 tests passing (100% success rate) ✅

**What Works**:
- Stone placement with all validations
- Capture mechanics (single and multi-stone groups)
- Suicide rule enforcement (with and without capture)
- Ko rule detection and enforcement (immediate recapture prevention)
- Ko clearing (via pass and via playing elsewhere)
- Snapback distinction (multi-stone captures don't trigger ko)
- Pass events with consecutive tracking
- Game termination on double-pass
- Area scoring computation
- All board sizes functional

## Status Summary

**Workstream status changed from `partial` to `done` on 2025-11-13.**

Go module is fully playable with all core mechanics implemented and thoroughly tested. All 29 tests passing (100% success rate) including complex ko rule scenarios and snapback distinction. The game supports complete play from opening moves through capture sequences to game termination and scoring. Ko detection, enforcement, and clearing are all validated through comprehensive tests.

---
_End of workstream 11._
