# Determinism, RNG & Timeline

Deterministic RNG + state hashing + history zipper ensure reproducible simulations & replay validation.

## RNG Abstraction

`IRandomSource` (e.g., XorShift) carried inside `GameState.Random`. Cloned on each state transition (immutability + deterministic progression).

## Hashing (Graduated)

`EnableStateHashing` (default: ON) computes 64-bit (FNV-1a) and 128-bit (xxHash128) hashes over canonical serialized artifact states + RNG fingerprint (Seed + two peek values). Identical initial state + event list + flag configuration ⇒ identical hashes.

Cross-platform stability validated across Linux, Windows, macOS (x64/ARM). Hash parity test infrastructure (`HashParityTestFixture`) provides helpers for comparing hashes across acceleration paths.

## Canonical Serialization Ordering

Stable ordering: artifact states sorted by artifact id → per state: type names + ordered public property values (little-endian primitives, UTF-8 strings) → RNG seed + two peeked UInt32 values.

## Timeline Zipper

Optional flag (e.g., `EnableTimelineZipper`) provides immutable `GameTimeline` with `Push/Undo/Redo` operations referencing existing states (no deep copies). Branching after Undo discards redo segment.

## Replay Determinism Test Contract

1. Build two games with same seed.
2. Apply identical event sequence.
3. Final hashes (64 & 128) and RNG seed must match.

See `CrossPlatformHashStabilityTests`, `RandomizedReplayDeterminismTests`, and `AccelerationPathHashParityTests` for comprehensive test coverage.

## When to Use Hashes

* Parity & regression tests
* Detecting duplicate states (future interning)
* Simulation caching / transposition tables (planned)
* Cross-platform replay validation
* Bug reproduction verification

## Future Enhancements

128-bit only mode, hash interning map, external reproduction envelope, timeline diff utilities.
