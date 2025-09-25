# Deterministic RNG & State Hashing (Phase 1 Scaffold)

Status: RNG abstraction integrated; feature-flagged state hashing (64/128-bit) implemented; timeline zipper implemented (flag-gated) with undo/redo invariant tests active.

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
- (Deferred) External reproduction tooling (seed + event list + final hash) per roadmap item 14; not implemented in core engine.

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

## Deferred Reproduction Envelope

The original embedded bug report capture has been removed. Future external tooling (roadmap item 14) will package:

- Seed
- Event script
- Final hash (64/128-bit)

for GitHub issue reproduction without adding surface area to the core engine.

## Hashing Overhead (Benchmark Reference)

The relative runtime impact of enabling `EnableStateHashing` (computing both 64-bit FNV-1a and 128-bit xxHash
over the canonical serialized state) is measured via a dedicated BenchmarkDotNet suite:

`HashingOverheadBenchmark` (in `benchmarks/HashingOverheadBenchmark.cs`)

Scenario: standard chess pawn double advance (e2 -> e4) executed with hashing disabled (baseline) and enabled.

Interpretation Guidance:

1. The delta reflects per-event hashing cost (canonical reflection + hashing) for a modest state size.
2. Larger or more complex states (e.g., Backgammon mid-game, Chess with many captured pieces) may shift the ratio slightly.
3. Improvements under consideration: property metadata caching, source-generated serializer, span-based struct reflection avoidance.
4. If overhead remains within acceptable bounds (< ~10% for representative events) extensive optimization may be deferred.

Future Optimization Hooks:

- Metadata precomputation per artifact state type.
- Pooled scratch buffers for flattened property traversal (avoiding reflection per event).
- Optional opt-in switch for 128-bit only (dropping 64-bit) if dual hash maintenance shows negligible benefit.

Benchmark Execution:

Run the benchmark project selecting either `HandleEventBaseline` or `HashingOverheadBenchmark` entry points.
Results should be captured and summarized in future documentation once performance stabilization phase begins.

## Trace Capture (Experimental)

When `FeatureFlags.EnableTraceCapture` is enabled at engine construction time, the observer pipeline is
decorated with a lightweight in-memory recorder that emits a linear sequence of entries for the latest
handled event batch. Each entry encodes (order, kind, phase label, rule type, event type, condition result,
state hash values if present). Entry kinds currently emitted:

1. `PhaseEnter`
2. `RuleEvaluated`
3. `RuleApplied`
4. `EventIgnored`
5. `StateHashed`

Scope / Intent:

- Debugging parity issues between legacy and DecisionPlan paths (ordering & rule selection).
- Future JSON trace export (current form is internal object graph accessible via `engine.LastTrace`).
- Foundation for future CLI / UI visualizer; schema deliberately minimal.

Limitations:

- Only retains the most recent evaluation sequence (no rolling buffer).
- Not yet serialized externally (JSON string can be produced via `EvaluationTrace.ToJson()` if needed).
- Adds minor allocation per trace entry (acceptable for diagnostic mode; future pooling possible).

Next Steps:

1. Add optional exporter returning immutable DTO or JSON directly.
2. Integrate with planned replay harness for step-by-step divergence detection.
3. Provide benchmark to quantify overhead (<5% target) with trace enabled vs disabled.
