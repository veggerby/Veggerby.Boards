---
id: 2
slug: deterministic-rng-and-history
name: "Deterministic Randomness & State History"
status: done
last_updated: 2025-09-26
owner: core
flags:
  - EnableStateHashing (exp)
  - EnableTimelineZipper (exp)
summary: >-
  Provides deterministic RNG abstraction, dual (64/128-bit) state hashing, canonical serialization ordering,
  and an immutable timeline zipper with undo/redo invariants and replay determinism tests.
acceptance:
  - Replay determinism test passes (seed + event sequence -> identical hashes & RNG state).
  - Undo/redo invariants hold (hash stability, idempotent redo).
  - Canonical RNG serialization documented (Seed, Peek[0], Peek[1]).
open_followups:
  - External reproduction envelope tooling.
  - Hash interning map for deduplication.
  - Timeline diff utilities & hash evolution reporting.
---

# Deterministic Randomness & State History

## Delivered

- `IRandomSource` + XorShift implementation
- GameState RNG snapshot cloning
- Dual hashing (FNV-1a 64-bit + xxHash128)
- Canonical serialization ordering doc
- Timeline zipper (undo/redo) + invariants tests
- Replay determinism acceptance tests

## Risks

Collision probability low but dual hash retained until further analysis; reproduction tooling deferred.

## Next Steps

See open_followups.
