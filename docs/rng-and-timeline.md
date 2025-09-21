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

## Planned

Timeline zipper, 128-bit hashing upgrade, bug report capture & replay harness.
