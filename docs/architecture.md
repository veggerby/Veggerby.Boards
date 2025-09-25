# Architecture Overview

Veggerby.Boards is organized into layered .NET projects:

- Core (`Veggerby.Boards`): Generic board game engine abstractions (Artifacts, State, Events, Rules, Phases, Mutators, Conditions, Builder).
- Game Modules (`Veggerby.Boards.Backgammon`, `Veggerby.Boards.Chess`): Declarative game definitions built via specialized `GameBuilder` subclasses.

```txt
+-------------v-------------+
|        Game Modules       |
|  Backgammon  |   Chess    |
+-------------+-------------+
              |
+-------------v-------------+
|           Core            |
| Artifacts • State • Flow  |
| Builder • Rules • Phases  |
+---------------------------+
```

## Core Concepts at a Glance

| Concept | Responsibility | Key Types |
|---------|----------------|-----------|
| Artifact | Static domain element (Piece, Tile, Dice, Board, Player) | `Artifact`, `Piece`, `Dice`, `Board`, `Player`, `Tile` |
| Game | Immutable container linking Board + Players + Artifacts | `Game` |
| State | Immutable snapshot of artifact states | `GameState`, `PieceState`, `DiceState<T>` |
| Progress | Engine + current state + last event | `GameProgress` (in tests/usages) |
| Capabilities | Immutable sealed acceleration seam (topology + path resolution + occupancy / attacks) | `EngineCapabilities` (Topology, PathResolver, AccelerationContext) |
| Event | Intention to mutate state | `IGameEvent`, `MovePieceGameEvent`, `RollDiceGameEvent<T>` |
| Mutator | Applies deterministic state change | `IStateMutator<T>` and implementations |
| Condition | Gate logic (state / event validity) | `IGameStateCondition`, `IGameEventCondition` |
| Rule | Couples event filtering + mutation | `IGameEventRule` + subclasses |
| Phase | Conditional grouping of rules / pre-processing | `GamePhase`, `CompositeGamePhase` |
| Builder | Declarative compilation of Game + initial State + Phases | `GameBuilder` and game-specific subclasses |

## Lifecycle

1. A `GameBuilder` subclass declaratively defines players, tiles, relations, directions, pieces, dice, initial placements, and phases.
2. `Compile()` produces a `GameEngine` + initial `GameState` wrapped in `GameProgress`.
3. External code raises an `IGameEvent` (e.g., `MovePieceGameEvent`).
4. Active `GamePhase` is resolved (`CompositeGamePhase` recurses to first matching leaf phase by state condition).
5. Phase pre-processors optionally transform the original event into one or more derived events (e.g., path expansion).
6. Each candidate event is checked by the phase's `IGameEventRule` chain (`Check` -> `ConditionResponse`).
7. If valid, rule invokes before/after mutators to produce a new immutable `GameState`.
8. A new `GameProgress` (engine + new state + last event) emerges.

## Immutability & Identity

- `Artifact` instances are identity-based; equality requires matching concrete type + Id.
- `GameState` is persistent: each transition keeps a reference to previous state, enabling diffs via `CompareTo`.
- Mutators never modify existing state in-place; they produce a new `GameState` through `Next()`.

## Phase Resolution Strategy

`CompositeGamePhase.GetActiveGamePhase(state)` traverses child phases returning the first leaf whose `IGameStateCondition` evaluates `Valid`. This enables hierarchical rule scoping (e.g., "initial roll" → "movement" → "resolution").

## Event Handling Workflow (Simplified)

```txt
Submit Event
   ↓
Active Phase? (condition)
   ↓ yes
Pre-processors (0..n) => derived events
   ↓
Rule.Check(event)
   ├─ Ignore → original state
   ├─ NotApplicable → pass through (other rule)
   └─ Valid → before mutator → after mutator → new GameState
```

## Backgammon vs Chess Modules

Both modules demonstrate reuse of the same engine primitives:

The compiled pattern subsystem is an optimization layer only; the legacy visitor remains the semantic source of truth (parity guard tests enforce identical observable behaviour).

## Design Principles

- Declarative over imperative: Builders describe; engine compiles.
- Separation of static structure (Artifacts) from dynamic behavior (State + Events + Rules).
- Explicit gating via Conditions; no hidden side-effects.
- Extensible through additive types (new Mutators / Conditions / Events) without modifying core engine.

### Emerging Optimization Synergy

The DecisionPlan (phase ordering pre-compilation) and compiled movement patterns are independent feature-flagged optimizations today. Future synergy work will layer a unified path resolution pipeline (sliding fast-path → compiled table lookup → visitor fallback) inside the internal path resolver without leaking additional surfaces. Any pre-binding of movement rays to decision plan nodes must still appear externally only as the existing `EngineCapabilities.PathResolver` behaviour.

## Extension Points

- Add new `IGameEvent` types for novel interactions.
- Introduce `IStateMutator<T>` implementations for atomic state transitions.
- Compose complex logic with `CompositeGameEventCondition` and `CompositeGameStateCondition`.
- Layer sequencing using nested `GamePhase` trees.

See `extensibility.md` for a step-by-step guide.

## Sliding Path Semantics Charter

This charter freezes the semantics used by the (compiled + fast-path) sliding movement resolver. It exists to ensure
all acceleration layers (BoardShape, PieceMap, Bitboards, Sliding Fast-Path) remain correctness-first and that tests
unambiguously capture expected behaviour.

### Scope

Applies to pieces whose movement is defined by directional, repeatable ray patterns (rook, bishop, queen–style). Does **not**
alter semantics for non-sliders (knight / king fixed patterns, pawn special rules). Non-sliders must never trigger the
sliding fast-path.

### Path Resolution Responsibility

`ResolvePath` for a sliding move is **occupancy-aware**:

1. It returns the ordered list of tile relations (edges) from source to target **inclusive** when the target is reachable
    under blocker rules below.
2. It returns `null` (unreachable) when any blocker invalidates reaching the target under these rules.
3. It never returns a partial path to a blocked square different from the requested target.

This means higher-level rules do not re-check intermediate occupancy; they only validate end conditions (e.g., capture legality,
check constraints, turn rules). Geometric-only fallback (legacy visitor) remains for patterns not supported by the compiled
subsystem but must match these semantics for overlapping cases.

### Blocker & Capture Semantics

Given a source S and directional ray R toward prospective target T:

| Situation | Outcome | Returned Path | Notes |
|-----------|---------|---------------|-------|
| Empty ray (no blockers before T) | Reachable | S→…→T | Standard move. |
| Friendly blocker at B before T | Unreachable | null | Cannot move to or beyond friendly blocker. |
| Friendly blocker exactly at T | Unreachable | null | Cannot end turn on a friendly-occupied square. |
| Enemy blocker at B before T | Unreachable | null | Cannot jump enemy. Capture only if T == B. |
| Enemy blocker exactly at T | Reachable (capture) | S→…→T | Path includes capture square, no beyond. |
| Multiple blockers (first at B1, second at B2 later) | Determined solely by B1 | See above | First blocker decisively determines reachability. |

Edge cases:

- Zero-length movement (S==T) is never considered a sliding path.
- Single-step rays (length 1) follow the same rules (effectively a fixed pattern) but may still benefit from fast-path skip logic.
- Board edges simply truncate rays; absence of T in truncated ray ⇒ unreachable.

### Multi-Blocker Rule

Only the nearest blocker along a ray is examined; subsequent occupants are irrelevant because the ray terminates at the first
blocker (capture or stop). This is enforced by the fast-path using the precomputed ray mask & incremental occupancy bitboards.

### Authority of Data Structures

Acceleration internals are intentionally hidden. Public semantics derive solely from `GameState` and pattern definitions. Internal layouts (piece arrays, bitboards, precomputed rays) are derived caches; `GameState` remains the single source of truth and would trigger internal recomputation if divergence were ever detected.

- Internal piece layout & occupancy indices (including bitboards when enabled) are NOT authoritative APIs.
- Attack ray generation, compiled pattern tables and sliding fast-path shortcuts are implementation details that MUST preserve the chartered semantics. Tests assert parity; no external API relies on their shapes.

### Invariants

1. `ResolvePath` must be deterministic: identical state + request ⇒ identical path or null.
2. All internal resolution layers (sliding fast-path, compiled resolver, visitor fallback) must produce identical results for supported slider scenarios (parity tests enforce this).
3. Non-sliding pieces must bypass sliding-specific fast-path logic entirely.
4. No allocation on steady-state successful path resolutions (excluding transient test instrumentation).
5. Blocking semantics above are exhaustive; new semantics require updating this charter **before** code changes.

### Testing Strategy

Parity tests MUST cover:

1. Empty ray (rook, bishop, queen examples in orthogonal & diagonal directions).
2. Friendly blocker mid-ray (unreachable).
3. Enemy blocker mid-ray (unreachable target beyond blocker).
4. Enemy blocker as target (capture path returned).
5. Multiple blockers (first prevails).
6. Edge truncated rays (target just outside board ⇒ null).
7. Non-sliders (ensure fast-path not invoked; parity implicit with compiled or legacy resolver).

### Performance Notes (Non-Normative)

Bitboards (≤64 tiles) + precomputed directional rays enable O(1) reachability checks per ray via masked occupancy & trailing
bit queries. This charter intentionally decouples semantics from optimization so future topology-specific fast paths (e.g.,
orthogonal-only boards) can be added without semantic drift.

### Future Extensions

- Boards >64 tiles will introduce dual-word (Bitboard128) masks retaining identical outward semantics.
- Topology classification (Orthogonal / Orthogonal+Diagonals / Arbitrary) may gate specialized micro-kernels; semantics remain constant.
- Additional move constraints (e.g., chess check rules) remain layered above path semantics, not embedded here.

---

Any change to sliding movement behaviour MUST update this section first; test additions then codify new expectations.
