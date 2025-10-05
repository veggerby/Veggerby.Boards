---
id: 4
slug: performance-data-layout-hot-paths
name: "Performance Data Layout & Hot Paths"
status: partial
last_updated: 2025-10-05
owner: core
flags:
  - EnableBitboards (exp)
  - EnableSlidingFastPath (graduated)
  - EnablePerPieceMasks (exp)
summary: >-
  Establishes structural acceleration layers (BoardShape, PieceMap, Bitboards, Sliding Attack Generator, Sliding Fast-Path)
  with parity suites and microbenchmarks showing up to 4.66× speedups; additional heuristics & >64 tile support pending.
acceptance:
  - Sliding fast-path parity (empty + blockers + captures + edge cases) and benchmark speedups achieved.
  - PieceMap & bitboard occupancy snapshots integrated.
  - Allocation-free fast-path hits validated (allocation probe benchmark).
open_followups:
  - Re-enable incremental bitboard updates after soak parity window.
  - Bitboard128 for >64 tile boards (scaffolding landed; coverage expanded via `BitboardSnapshot` tests).
  - Heuristic pruning via per-piece masks + topology.
  - Mobility evaluator extensions (leapers, pawns, weighting).
  - LINQ legacy sweep & hot-path audit.
---

# Performance Data Layout & Hot Paths

## Delivered

- BoardShape adjacency layout
- PieceMap incremental snapshot
- Bitboard snapshot (rebuild path) + occupancy index
- Sliding attack generator (ray precompute)
- Sliding fast-path (reconstruction) enabled by default (≤64 tiles)
- Parity V2 tests (blockers, captures, multi-block, immobile, zero-length)
- Micro + scenario benchmarks (empty / blocked / capture / off-ray)
- Allocation probe + per-piece masks flag
- Mobility evaluator prototype

## Pending / Deferred

See open_followups in front matter.

## Risks

Incremental bitboard path disabled—performance opportunity cost; heuristic complexity could erode clarity.

## Next Steps

Prioritize incremental bitboard reinstatement + pruning heuristics before expanding scope (Bitboard128).
