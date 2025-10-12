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

## Core Engine (Existing References)

(Placeholder section – integrate selected existing benchmark summaries in a later pass: sliding attack generation, pattern compilation, bitboard snapshot build, hashing overhead, observer overhead.)

Planned actions:

* Aggregate top N (time + alloc) regression-sensitive benchmarks.
* Record commit hash per capture.
* Provide quick command snippet for targeted re-run.

## Methodology & Conventions

* All benchmarks executed with `dotnet run --project benchmarks -c Release -m --filter "<pattern>"`.
* Memory values (Allocated) are per-operation inclusive managed allocations (BenchmarkDotNet MemoryDiagnoser).
* For comparisons, prefer relative % delta over absolute nanosecond differences (hardware variance). Target ±5% stability for non-hot-path changes.
* Treat Gen0 count (per 1000 ops) > 1 as signal to assess allocation sources; aim to keep hot events < 1 Gen0 / 1000 ops when feasible.

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
