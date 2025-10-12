---
slug: performance-summary
name: "Benchmark & Performance Summary"
last_updated: 2025-10-12
owner: core
summary: >-
  Consolidated snapshot of key benchmark baselines (time + allocation) for critical engine paths and recent deck-building optimizations.
---

# Benchmark & Performance Summary

This document consolidates representative benchmark results to provide a stable reference point for future regression checks. All numbers captured on:

* Host: Linux (Debian bookworm, dev container)
* CPU: Intel Core i9-14900KF (reported by container)
* Runtime: .NET 9.0.9 (RyuJIT x86-64-v3)
* BenchmarkDotNet: v0.15.4 (DefaultJob, Release configuration, MemoryDiagnoser)

> Note: Benchmarks are indicative, not contractual. Always re-run locally after significant rule/mutator changes. Use relative deltas rather than absolute nanoseconds for gating.

## Deck-building Module (Post Optimization – 2025-10-12)

| Scenario | Mean | Allocated | Notes |
|----------|------|----------|-------|
| GainFromSupply (full event) | 2.94 µs | 7.37 KB | Includes condition evaluation, selective pile cloning, DeckState freeze, GameState snapshot, supply stats update. |
| GainFromSupply (condition only) | 97.6 ns | 176 B | Isolated `GainFromSupplyEventCondition` Evaluate path (no mutator). |

### Interpretation

* Gating overhead constitutes ~3.3% of the full validated event cost (≈98 ns / 2940 ns), indicating current focus should remain on DeckState freezing / GameState snapshot costs for further optimization.
* Selective cloning reduced intermediate list duplication; remaining allocations come chiefly from:
  * New `List<Card>` for mutated pile.
  * Re-wrapping piles into read-only lists inside `DeckState`.
  * New supply dictionary + history chain node.
  * Extras replacement (DeckSupplyStats) – minimal.

### Potential Next Optimizations (Deferred)

1. DeckState Freeze Micro-Optimization
   * Option: Internal fast path when non-target piles already materialized as read-only lists → skip ToList() clone.
   * Risk: Must maintain immutability contract; requires proof reused lists are not externally mutated.
2. Supply Dictionary Delta Encoding
   * Replace full dictionary copy with copy-on-write for single-entry decrement.
   * Complexity vs marginal gain trade-off (dictionary small in typical configurations).
3. Pile Storage Representation
   * Use struct-backed small vector (span-friendly) for frequent short piles (e.g., discard early game) to reduce per-pile list wrapper overhead.
4. Batch Event Application (Speculative)
   * Amortize GameState snapshot chaining for sequences of deterministic, non-observable internal transitions (would require explicit batching seam to preserve determinism & observability). Not currently prioritized.

## Core Engine – Sliding Path Resolution (Initial Capture – 2025-10-12)

Benchmark: `SlidingPathResolutionBenchmark` (rook/bishop/queen representative rays across board with varying blocker density). Each benchmark resolves a fixed small set of source→target queries.

| Variant | Density | Mean | Allocated | Notes |
|---------|---------|------|-----------|-------|
| Compiled | empty | 760.7 µs | 36.09 KB | Baseline compiled resolver (no bitboards) with acceleration context minimal. |
| CompiledWithBitboardsNoFastPath | empty | 644.3 µs | 42.13 KB | Bitboards + compiled; sliding fast-path disabled (measures overhead neutrality). |
| CompiledNoBitboards | empty | 891.2 µs | 36.09 KB | Explicit compiled-only path without bitboards context. |
| Compiled | quarter | 790.3 µs | 36.09 KB | Light occupancy (≈25% blockers). |
| CompiledWithBitboardsNoFastPath | quarter | 615.2 µs | 48.45 KB | Bitboards overhead increases allocation due to occupancy + snapshots. |
| CompiledNoBitboards | quarter | 796.3 µs | 36.09 KB | Comparable to empty; pruning cost modest. |
| Compiled | half | 824.1 µs | 36.09 KB | Denser blockers reduce some ray traversal but add occupancy checks. |
| CompiledWithBitboardsNoFastPath | half | 636.1 µs | 53.77 KB | Allocation rises with additional bitboard player maps; still faster mean. |
| CompiledNoBitboards | half | 755.1 µs | 36.09 KB | Slight speed advantage vs quarter due to earlier pruning on many rays. |

### Interpretation (Sliding)

* Bitboards (even without sliding fast-path) deliver a 15–20% runtime reduction in these micro workloads, at the cost of additional one-time snapshot allocations per operation (because benchmark rebuilds contexts per density scenario). In steady-state engine progression those allocations amortize across many path resolutions.
* Allocation deltas (≈+6–18 KB) stem from bitboard and piece map snapshot structures; optimization headroom: pooling or incremental diff snapshots (deferred – would require mutation-safe layering to preserve immutability semantics).
* High relative Error/CI in some rows due to very low iteration count (3) chosen for quick capture. For gating regressions increase IterationCount (>= 9) and add InvocationCount batching to push per-iteration >100ms (BenchmarkDotNet warning currently emitted).

### Next Core Metrics (Planned)

* SlidingFastPath micro (empty vs blocked vs capture vs off-ray) – quantify fast-path delta & allocation profile.
* Bitboard snapshot build cost (initial vs incremental after single move) – baseline layout scaling behavior.
* DecisionPlan evaluation overhead (phase scanning vs grouped) – requires adding grouping flag variants.
* Hashing / identity cost for artifact lookups (piece/tile) – micro to ensure no accidental regression.

Planned methodology: run with higher iteration counts and aggregate percent deltas relative to Compiled baseline.

## Methodology & Conventions

* All benchmarks executed with `dotnet run --project benchmarks -c Release -m --filter "<pattern>"`.
* Memory values (Allocated) are per-operation inclusive managed allocations (BenchmarkDotNet MemoryDiagnoser).
* For comparisons, prefer relative % delta over absolute nanosecond differences (hardware variance). Target ±5% stability for non-hot-path changes.
* Treat Gen0 count (per 1000 ops) > 1 as signal to assess allocation sources; aim to keep hot events < 1 Gen0 / 1000 ops when feasible.

## Core Engine – Bitboard Snapshot Build (Initial vs Incremental – 2025-10-12)

Benchmark: `BitboardSnapshotBenchmark` (build `BitboardSnapshot` + `PieceMapSnapshot` over 8x8 board with 32 pieces) comparing initial construction vs after a single piece move.

| Scenario | Mean | Allocated | Gen0/1k Ops | Notes |
|----------|------|----------|------------|-------|
| InitialBuild | 4.514 µs | 3.06 KB | 0.1602 | Baseline full snapshot/layout population. |
| PostMoveBuild | 4.658 µs | 3.06 KB | 0.1602 | Rebuild after one piece advanced one rank (≈ +3.2% over initial). |

### Interpretation (Snapshot)

* Rebuild cost is symmetric with initial (no incremental diff yet) – small +3% variance reflects normal noise; treat >10% increase as regression signal.
* Allocation flat between initial and post-move indicating no hidden per-move growth; dominated by immutable snapshot structures (bitboards + piece maps).
* Opportunity (deferred): introduce structural sharing for unchanged segments or diff-based update to shave ~2–3 µs and halve allocations if proven hot in aggregate turn sequencing.

## Core Engine – Segmented Bitboard Snapshot Variants (Incidental Capture – 2025-10-12)

Benchmark: `SegmentedBitboardSnapshotBenchmark` (existing) surfaced in same run; included here for completeness.

| Tiles | Segmented | Mean | Allocated | Notes |
|-------|-----------|------|-----------|-------|
| 64 | Disabled | 15.23 µs | 936 B | Baseline 64 tile non-segmented. |
| 64 | Enabled | 12.14 µs | 1.19 KB | Segmentation reduces work, modest allocation increase. |
| 128 | Disabled | 11.76 µs | 952 B | Larger board but favorable layout density (fewer segments) lowers mean. |
| 128 | Enabled | 13.80 µs | 1.23 KB | Overhead exceeds benefit at this size (cache / branch effects). |

### Interpretation (Segmented)

* Segmentation helps at 64 tiles (≈20% faster) but regresses at 128 in this synthetic; suggests crossover point where segment management overhead outweighs traversal savings.
* Future work: add breakpoint heuristic (enable segmentation only below threshold) – defer until broader board shapes profiled.

## Core Engine – Hashing & Identity Lookups (2025-10-12)

Benchmark: `HashingLookupBenchmark` – micro for artifact identity & enumeration costs vs increasing piece counts. Board is linear chain; relations present only to satisfy construction invariants.

| Method | Pieces | Mean | Allocated | Ratio (to Enumerate @ same count) | Notes |
|--------|--------|------|-----------|-----------------------------------|-------|
| EnumeratePieceStates | 32 | 448.5 ns | 760 B | 1.00 | Iterate `PieceState` list, filter by owner. |
| LookupPiecesById | 32 | 3.981 µs | 2.75 KB | 8.88 | Repeated dictionary lookups via `Game.GetPiece`. |
| ResolveTilesSequentially | 32 | 19.42 µs | 8.00 KB | 43.31 | Linear tile id lookups (twice tile count vs pieces). |
| EnumeratePieceStates | 64 | 779.6 ns | 1.27 KB | 1.00 | Enumeration scales ~linearly (sub‑µs). |
| LookupPiecesById | 64 | 14.48 µs | 5.50 KB | 18.57 | Hash lookups scale ~O(n) because we perform n independent id resolutions (each O(1)). |
| ResolveTilesSequentially | 64 | 72.18 µs | 16.00 KB | 92.59 | Tile lookup cost grows with doubled tile set. |
| EnumeratePieceStates | 96 | 1.095 µs | 2.29 KB | 1.00 | Still sub‑1.2 µs enumeration for 96 pieces. |
| LookupPiecesById | 96 | 28.11 µs | 8.25 KB | 25.68 | Linear in piece iterations; per-lookup stable. |
| ResolveTilesSequentially | 96 | 154.63 µs | 24.00 KB | 141.24 | Sequential tile resolution worst-case path. |

### Interpretation (Hashing / Identity)

* Enumeration remains the fastest path for owner-filter style queries; prefer enumerate-then-filter for batch logic instead of repeated `GetPiece` calls when possible.
* `GetPiece` remains effectively O(1) per lookup; aggregate growth is due to performing more independent lookups, not degradation in hashing – per-call ~4 µs (32 pieces) → ~14 µs (64) → ~28 µs (96) for batched loops of n lookups.
* Tile resolution sequential approach intentionally worst-case; typical engines hold a dictionary for tiles—consider adding direct tile map if tile lookup becomes hot (not currently on any critical path).
* Allocation ratios scale proportionally with number of artifacts traversed (enumeration) or number of intermediate iterations (loop scaffolding). No unexpected allocation spikes observed.
* Regression watch: >15% change in enumeration mean or unexpected Gen0 jump indicates possible equality/hash alteration or hidden allocation introduced in state iteration.

### Potential Follow-ups (Deferred)

1. Add benchmark including mixed piece + tile lookup interleaving to emulate rule evaluation patterns.
2. Introduce cached tile index map to compare sequential vs indexed resolution (expect O(n) → O(1) per lookup speedup for large boards).
3. Parameterize benchmark with invocation batching to reduce measurement noise on sub‑µs enumerations (improve CI width).

## Change Log Integration

Relevant CHANGELOG entries (Unreleased):

* Added: DeckSupplyStats, deck-building benchmarks, structural sharing & supply stats tests.
* Changed: GainFromSupplyStateMutator selective cloning; EndGameEventCondition fast path via supply stats.

## Maintenance Checklist

When updating this document:

1. Capture command + commit hash (append near top).
2. Update dates in front matter `last_updated`.
3. Ensure new benchmarks include Mean + Allocated + brief note.
4. Cross-link new optimization rationale back into corresponding workstream doc.

---
_End of summary._
