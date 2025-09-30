---
id: 11
slug: go-game-module
name: "Go Game Module"
status: partial
last_updated: 2025-09-30
owner: go
summary: >-
  Introduce an initial Go (Weiqi, Baduk) module: board topology, stone placement, pass turns, capture/ko/liberty rules,
  scoring (area/territory variants), and end-of-game detection while preserving engine determinism & immutability.
acceptance:
  - Board builder supports 9x9 / 13x13 / 19x19 sizes (default 19) with orthogonal liberty relations.
  - Stone placement enforces emptiness, suicide prohibition (unless capturing), and simple ko rule.
  - Capture resolution removes opponent groups with zero liberties.
  - Consecutive pass rule ends game after two passes (state flagged as complete / terminal).
  - Scoring routine (area mode initial) computes territory + captures deterministically with tests.
  - Nomenclature renders placements in standard coordinate form (e.g., 'D4'), pass as 'pass'.
  - Comprehensive tests: capture chains, snapback, ko denial, suicide rejection, multi-group capture, end-on-two-passes.
open_followups:
  - Territory vs area scoring mode toggle.
  - Handicap stone placement helper.
  - Japanese vs Chinese rules nuance (ko / seki) beyond simple ko (no superko in v1).
  - Dead group adjudication workflow (manual marking) – out of scope initial.
---

# Go Game Module (WS-GO-001)

## Goal

Provide a deterministic, testable Go implementation leveraging the existing engine primitives (immutable state snapshots, explicit events, mutators) while ensuring capture & ko logic remains pure and auditable.

## Current State Snapshot

- Implemented: `GoGameBuilder` (board topology + players + stone pools), `PlaceStoneGameEvent` + mutator (emptiness only), `PassTurnGameEvent` + mutator (increments pass count), `GoStateExtras` (ko placeholder, pass counter, board size), minimal `GoNomenclature`.
- Missing: capture/group + liberty evaluation, suicide rule enforcement, ko detection & marking, end-of-game transition on double pass (terminal flag), scoring algorithm, richer nomenclature (coordinates), property tests & benchmarks.

## Success Criteria

- Deterministic group + capture evaluation (same placement => identical captured set) with no reliance on global caching.
- Complexity: Placement capture resolution executes in O(group size + adjacent groups) without quadratic scanning.
- Ko: Simple ko enforced (immediate recapture to prior board state intersection denied) recorded via `KoTileId`.

## Deliverables

1. Group & Liberty Resolver (iterative flood-fill; no recursion to avoid stack risk on 19x19).
2. Capture Resolution Mutator augmentation (remove zero-liberty opponent groups, then own-group suicide check).
3. Ko Detection & Marking (store last taken single-stone position where applicable).
4. Pass Handling Termination Rule (two consecutive passes -> terminal state snapshot or phase closure).
5. Scoring (area mode) + helper to compute territory/captures.
6. Nomenclature enhancement (map numeric coordinates to letters skipping 'I').
7. Comprehensive Test Suite (captures, multi-group, ko, suicide, snapback, pass termination, scoring).
8. Benchmarks (19x19 capture stress, repeated ko cycle denial cost, full-board flood-fill worst case).
9. Documentation (workstream, scoring semantics, ko limitations).

## Detailed Task Breakdown

| Seq | Task | Description | Output | Risk |
|-----|------|-------------|--------|------|
| 1 | Group Data Structure | Represent groups (stones + liberties) | `GroupInfo` struct | Low |
| 2 | Liberty Scan | Flood-fill to collect group stones & liberties | Scanner method | Medium |
| 3 | Capture Logic | Remove opponent zero-liberty groups on placement | Updated mutator | Medium |
| 4 | Suicide Rule | Reject placements leaving own group at zero liberties (unless capturing) | Validation branch | Medium |
| 5 | Ko Tracking | Detect immediate repetition & set `KoTileId` | Extras update | Medium |
| 6 | Double Pass Termination | Create terminal snapshot or phase closure | Mutator + flag | Low |
| 7 | Scoring Algorithm | Area scoring computation | Score service | High |
| 8 | Nomenclature Refinement | Coordinate translation + tests | Updated nomenclature | Low |
| 9 | Test Matrix | Edge/property tests (snapback, ko, seki-like stable shapes) | Test files | High |
| 10 | Benchmarks | Capture & group evaluation perf | Benchmark suite | Medium |
| 11 | Docs | Scoring & ko docs | Markdown pages | Low |

## Data Points / Invariants

- Group liberties recomputed per placement (no incremental caching in v1).
- Ko only if a single-stone capture and resulting shape identical potential immediate recapture.
- Terminal state flagged solely by two consecutive passes (extras `ConsecutivePasses == 2`).

## Testing Matrix (Sampling)

| Scenario | Expectation |
|----------|-------------|
| Single-stone capture | Removed; liberties freed for adjacent friendly group |
| Multi-group capture | All adjacent opponent groups with zero liberties removed |
| Suicide attempt | Move rejected (state unchanged) unless capture occurs simultaneously |
| Simple ko recapture | Denied next move on ko position |
| Snapback (non-ko) | Allowed if not immediate repetition shape |
| Double pass | Game enters terminal state |
| Large flood-fill | Completes under allocation threshold |

## Performance Considerations

- Use stack-allocated buffers or pooled structures at most; start with simple List-based flood-fill then optimize.
- Avoid LINQ in inner scanning loops.

## Open Questions

- Whether to embed scoring in mutator vs separate scoring pass after terminal state.
- Future superko extension hook shape.

## Status Summary

Initial scaffolding complete; core rules pending. Workstream now tracked as `partial`.

---
_End of workstream 11._
