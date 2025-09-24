# Core Concepts

This document defines the foundational abstractions of Veggerby.Boards.

## Artifacts

Artifacts are immutable identity objects composing the static topology and actors of a game:

- `Board` – Graph of `TileRelation` edges over `Tile` nodes.
- `Tile` – Positional node; related via directed `TileRelation` with a `Direction`.
- `Piece` – Player-owned movable artifact defined by one or more movement `IPattern`s (directional or fixed sequences).
- `Dice` – Randomizable artifact represented in state via `DiceState<T>`.
- `Player` – Participant identity.
- `CompositeArtifact` / other custom artifacts can extend capabilities.

Artifacts are created only during `GameBuilder.Build()` and aggregated into `Game`.

## Patterns & Movement

Piece mobility is abstracted through pattern objects:

- `DirectionPattern` – Single direction, optionally repeatable (e.g., rook lines).
- `MultiDirectionPattern` – Multiple directions merged, repeatable.
- `FixedPattern` – Explicit sequence of directions (e.g., knight L-shape encoded as sequence pairs).
- `NullPattern` – No movement (placeholder).

Patterns are visited (e.g., `ResolveTilePathPatternVisitor`) to create a concrete `TilePath` at event time.

### Compiled Movement (Experimental)

When the feature flag `EnableCompiledPatterns` is enabled during build, supported pattern types are precompiled
into a lightweight intermediate representation (see `compiled-patterns.md`). Resolution then prefers a
table-driven lookup (`CompiledPatternResolver`) falling back to the visitor only when no compiled candidate path
matches. Parity tests ensure behavior is identical; if the flag is disabled the legacy visitor always executes.

## Game

`Game` aggregates the immutable structural model: `Board`, `Players`, and all `Artifact` instances. It does not track mutable positions or dice values—those live in `GameState`.

## GameState

`GameState` is an immutable snapshot mapping each active `Artifact` to an `IArtifactState` such as:

- `PieceState` (piece on a tile)
- `DiceState<T>` (dice with a value)
- `NullDiceState` (dice without a value yet)

State transitions produce a new `GameState` retaining a pointer to its predecessor enabling diffing (`CompareTo`).

## GameProgress

Encapsulates:

- `GameEngine` (structural + flow logic)
- Current `GameState`
- Last processed `IGameEvent` (nullable)

Methods like `HandleEvent` return a new `GameProgress` with updated state.

## Events

`IGameEvent` marks player or system intent. Provided events:

- `MovePieceGameEvent` – Move a specific `Piece` along a resolved `TilePath`.
- `RollDiceGameEvent<T>` – Assign values to one or more dice simultaneously.
- `NullGameEvent` – No-op placeholder.

## Event Pre-Processing

`IGameEventPreProcessor` can expand or transform an incoming event into derived events (e.g., single-step decomposition, validation path filtering). Phases list zero or more pre-processors.

## Conditions

Two condition families regulate flow:

- `IGameStateCondition` – Evaluates a `GameState` to enable/disable a `GamePhase`.
- `IGameEventCondition` – Evaluates a proposed event inside a rule (`ConditionResponse.Valid | Invalid | Ignore | NotApplicable`).

Composite variants (`CompositeGameStateCondition`, `CompositeGameEventCondition`) enable logical composition (Any, All, None).

## Rules

An `IGameEventRule` couples:

- Applicability & validation logic (`Check`)
- Before/after mutators (`IStateMutator<T>`) executed if valid

`GameEventRule<T>` implements common plumbing (type filtering, ignore semantics). `SimpleGameEventRule` specializes with a single condition.

## Mutators

`IStateMutator<T>` implementations apply deterministic transitions:

- `MovePieceStateMutator` – Repositions a piece.
- `DiceStateMutator<T>` – Assigns rolled value(s).
- `NextPlayerStateMutator` – Advances active player tracking (via active player artifact state or condition interplay).
- `ClearDiceStateMutator` – Clears consumed dice values.
- Custom examples in Backgammon: `ClearToTileStateMutator`, `DoublingDiceStateMutator`.

Mutators must be pure with respect to prior states (no side effects outside returned `GameState`).

## Phases

`GamePhase` represents a conditional rule scope. A `CompositeGamePhase` nests multiple phases while providing a shared parent condition.

Resolution finds the first leaf phase whose `IGameStateCondition` returns `Valid`.

## Builder Pattern

`GameBuilder` collects declarative definitions:

1. Players, directions, tiles, relations (graph edges), artifacts (pieces, dice, custom)
2. Patterns for each piece (directional / fixed / repeatable)
3. Initial placements and dice states
4. Phases with nested rules and event handling chains

`Compile()` produces: `GameEngine` + initial `GameState` + root `GamePhase` tree wrapped inside `GameProgress`.

## Equality & Integrity

- Artifacts: structural equality by type + Id.
- States: equality by set equivalence of child artifact states.
- Board integrity: all tiles derived from relation edges; invalid or empty relation sets rejected.

## Error Handling

Engine throws specialized exceptions (e.g., `BoardException`, `InvalidGameEventException`) when invariants are broken (invalid event application, construction errors).

## Typed Event Handling Results

While `HandleEvent` returns a new `GameProgress` (or throws for invariants), the preferred deterministic evaluation surface is
`GameProgress.HandleEventResult(IGameEvent)`, which returns an `EventResult` capturing:

- `State` – Successor state snapshot (or original if rejected)
- `Applied` – Whether any rule mutator produced a new state
- `Reason` – Structured `EventRejectionReason` enum
- `Message` – Optional diagnostic detail (exception message, condition reason)

### Rejection Reasons

| Reason | Meaning | Typical Trigger |
| ------ | ------- | --------------- |
| None | Event applied successfully | A rule validated and mutators changed state |
| PhaseClosed | No active phase accepted the event | No leaf phase condition evaluated to valid |
| NotApplicable | No rule produced a state change | Valid rule chain but mutators no-op OR no matching rule types |
| InvalidOwnership | Ownership / source tile mismatch | BoardException containing "Invalid from tile" |
| PathNotFound | Required movement/path cannot be resolved against dice or patterns | BoardException containing "No valid dice state for path" |
| RuleRejected | A rule explicitly rejected via invalid condition | Condition returned `Invalid` leading to `InvalidGameEventException` |
| InvalidEvent | Malformed event semantics | Unmapped `BoardException` (payload / structural issue) |
| EngineInvariant | Internal invariant breach | Unexpected exception (should be investigated) |

The mapping is deterministic: identical input state + event yields the same `EventResult`.

### Guidance

Use the typed result when integrating an API or diagnostics layer. Only escalate to thrown exceptions for construction and invariant failures. Module authors should prefer returning `Invalid` from conditions to surface `RuleRejected` rather than throwing.

The legacy extension method `HandleEventResult(this GameProgress, IGameEvent)` remains temporarily (marked `[Obsolete]`) and will be removed in a future release after consumers migrate to the instance API.

## Extensibility Summary

Add new interactions by introducing:

- Artifact subtype (if identity-level distinction is required)
- Event (`IGameEvent`)
- Mutator(s) (`IStateMutator<T>`) for state evolution
- Conditions (`IGameStateCondition` / `IGameEventCondition`) gating flow
- Rules / Phases wiring them together via builder

See `extensibility.md` for detailed steps.
