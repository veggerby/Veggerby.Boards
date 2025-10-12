---
slug: workstreams
name: "Workstreams Overview"
last_updated: 2025-10-12
status_index:
  done: [1,2,3,17,18]
  partial: [4,5,6,7,8,9,10,11]
  planned: [12,13,14,16]
---

# Workstreams Overview

| ID | Workstream | Status | Summary |
|----|------------|--------|---------|
| 1 | Rule Evaluation Engine Modernization | done | Unified decision plan, grouping, filtering, exclusivity masks, predicate hoisting. |
| 2 | Deterministic RNG & History | done | Stable RNG abstraction, timeline zipper scaffolding, dual hashing placeholders. |
| 3 | Movement & Pattern Compilation | closed | Compiled movement patterns + sliding fast-path (≤64 tiles). |
| 4 | Performance Data Layout & Hot Paths | partial | Sliding fast-path, bitboards (incremental soak + 128-bit scaffolding), added BitboardSnapshot coverage, parity & benchmarks. |
| 5 | Concurrency & Simulation | partial | Deterministic playouts + metrics, parallel path flagged. |
| 6 | Observability & Diagnostics | partial | EventResult taxonomy, grouping, kind filtering, trace prototype. |
| 7 | Developer Experience & Quality Gates | partial | Style charter, doc overhaul, benchmark categorization, CHANGELOG condensation. |
| 8 | Structural Refactors | partial | Timeline zipper + hashing scaffolding (flag-gated). |
| 9 | Turn & Round Sequencing | partial | Default-on sequencing; mutators + rotation helper; docs helper guidance delivered; Go two-pass & hash parity pending. |
| 10 | Chess Full Move Legality | partial | Castling rights + safety, metadata predicates, ids normalized; generation & endgame pending. |
| 11 | Go Game Module | partial | Board + placement/pass scaffolding; capture/ko/scoring pending. |
| 12 | Ludo / Parcheesi Game Module | planned | Race track, entry on 6, capture reset, safe squares baseline. |
| 13 | Checkers / Draughts Game Module | planned | Dark-square graph, mandatory capture, multi-jump deterministic. |
| 14 | Monopoly Game Module | planned | Track, property ownership, rent, jail & deck (simplified baseline). |
| 16 | Risk Game Module | planned | Territory graph, reinforcement calc, combat dice, conquest. |
| 17 | Deck-building Core Module | done | Zone mechanics, Action/Buy split, supply configurator, scoring & termination, docs page; benchmarks & alt end-trigger optional/deferred. |
| 18 | Cards & Decks Module | done | Deterministic cards/decks with piles, shuffle, draw/move/discard, builder wiring. |

Legend: done = acceptance met; partial = some acceptance outstanding; closed = intentionally deferred; planned = not yet started.
