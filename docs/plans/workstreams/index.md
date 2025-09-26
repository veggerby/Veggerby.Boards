---
slug: workstreams
name: "Workstreams Overview"
last_updated: 2025-09-26
status_index:
  done: [1,2,3]
  partial: [4,5,6,7,8,9]
  planned: []
---

# Workstreams Overview

| ID | Workstream | Status | Summary |
|----|------------|--------|---------|
| 1 | Rule Evaluation Engine Modernization | done | Unified decision plan, grouping, filtering, exclusivity masks, predicate hoisting. |
| 2 | Deterministic RNG & History | done | Stable RNG abstraction, timeline zipper scaffolding, dual hashing placeholders. |
| 3 | Movement & Pattern Compilation | closed | Compiled movement patterns + sliding fast-path (≤64 tiles). |
| 4 | Performance Data Layout & Hot Paths | partial | Sliding fast-path, bitboards snapshot path, parity & benchmark coverage. |
| 5 | Concurrency & Simulation | partial | Deterministic playouts + metrics, parallel path flagged. |
| 6 | Observability & Diagnostics | partial | EventResult taxonomy, grouping, kind filtering, trace prototype. |
| 7 | Developer Experience & Quality Gates | partial | Style charter, doc overhaul, benchmark categorization, CHANGELOG condensation. |
| 8 | Structural Refactors | partial | Timeline zipper + hashing scaffolding (flag-gated). |
| 9 | Turn & Round Sequencing | partial | TurnState segmentation + overhead benchmark baseline. |

Legend: done = acceptance met; partial = some acceptance outstanding; closed = intentionally deferred; planned = not yet started.
