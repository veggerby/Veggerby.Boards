# Deterministic RNG (Phase 1 Scaffold)

Status: RNG abstraction integrated; timeline + hashing pending.

Components:

1. `IRandomSource` + `XorShiftRandomSource`
2. `GameState.Random` snapshot (cloned on `Next`)

Usage:

```csharp
var rng = XorShiftRandomSource.Create(123UL);
var initial = GameState.New(initialStates, rng);
var next = initial.Next(changes); // RNG cloned
```

Planned: timeline zipper, Merkle hashing, bug report replay.
# Deterministic RNG & Timeline (Preview)
