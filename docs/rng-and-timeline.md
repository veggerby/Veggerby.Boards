# Deterministic RNG & State Hashing (Phase 1 Scaffold)

Status: RNG abstraction integrated; feature-flagged state hashing scaffold implemented; timeline zipper pending.

Components:

1. `IRandomSource` + `XorShiftRandomSource`
2. `GameState.Random` snapshot (cloned on `Next`)
3. Optional state hashing (FNV-1a 64-bit) behind `EnableStateHashing`

Usage:

```csharp
var rng = XorShiftRandomSource.Create(123UL);
var initial = GameState.New(initialStates, rng);
var next = initial.Next(changes); // RNG cloned
```

## State Hashing

When `FeatureFlags.EnableStateHashing` is enabled, each `GameState` computes a 64-bit FNV-1a hash covering:

1. Sorted artifact states (by artifact Id stable ordering)
2. For each artifact state: artifact id bytes, artifact type name, state type name, canonical binary serialization of the state (reflection over public readable properties, name-ordered, typed value tags, little-endian primitives, UTF-8 strings)
3. RNG fingerprint: seed (UInt64 LE) + two peeked UInt32 values to capture near-future entropy without mutating the RNG

Rationale:

- Provides early collision-resistant (not cryptographic) identity for parity tests, replay validation, and upcoming timeline zipper.
- FNV-1a chosen for simplicity and speed; will upgrade to xxHash128 (or BLAKE3-128) once canonical binary serialization is in place.

Observer Integration:

- `IEvaluationObserver.OnStateHashed(GameState state, ulong hash)` fires after each successful rule application when hashing enabled.

Limitations / Next Steps:

- Replaces earlier temporary `ToString()` approach with a canonical binary layout (property-name ordered, typed tags) to stabilize cross-platform equality.
- Transition to 128-bit hash to further reduce collision probability and enable Merkle tree / timeline node dedup.
- Introduce `GameTimeline` zipper with undo/redo; hash becomes node identity key.
- Add `BugReport` envelope capturing initial seed + event list + final hash.

## Timeline Zipper (Experimental)

When `FeatureFlags.EnableTimelineZipper` (naming placeholder) is activated, an immutable zipper structure (`GameTimeline`) can be used to navigate history:

```csharp
var initial = GameState.New(initialStates, rng);
var timeline = GameTimeline.Create(initial);
timeline = timeline.Push(nextState);      // advance
timeline = timeline.Undo();               // step back
timeline = timeline.Redo();               // step forward
timeline = timeline.Push(branchState);    // creates new branch, clearing redo stack
```

Characteristics:

1. Immutable: every operation returns a new `GameTimeline` reusing existing `GameState` instances.
2. Deterministic: no mutation, states already hashable when hashing flag enabled.
3. Branch Semantics: pushing after an `Undo` discards `Future` (redo) states.
4. Memory Footprint: references only—no deep copies; `GameState` chain already shares structure.

Future Enhancements:

- Hash‑interning map keyed by 128-bit hash (post upgrade) to deduplicate identical states across divergent branches.
- Timeline diff utilities (first divergence, merge attempt) for simulation tooling.

## Planned

128-bit hashing upgrade (xxHash128), bug report capture & replay harness, hash interning, and timeline diff utilities.
