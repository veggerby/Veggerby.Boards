# Core Concepts

Concise model reference. For architectural layering see `architecture.md`.

## Artifacts

Immutable identities created in the builder: `Board`, `Tile`, `Piece`, `Dice`, `Player` (and custom). Equality = concrete type + Id.

## Game & GameState

`Game`: structural aggregation (no mutable fields).
`GameState`: immutable snapshot chain of artifact states (`PieceState`, `DiceState<T>`, etc.) with `Previous` link and comparison utilities.

## Events

Declarative intents: `MovePieceGameEvent`, `RollDiceGameEvent<T>`, plus feature/experimental events (turn sequencing). They never mutate state themselves.

## Rules & Conditions

`IGameEventRule` couples event filtering + condition evaluation + mutators. Conditions return `ConditionResponse` (`Valid`, `Invalid`, `Ignore`, `NotApplicable`). Composite forms support logical composition.

## Mutators

Pure transformations implementing `IStateMutator<TEvent>` returning either a *new* `GameState` or the original if no changes (idempotent outcome). No side effects; no in-place mutation.

## Phases

Hierarchical (composite) conditional scopes. Phase resolution selects FIRST valid leaf (deterministic ordering). Pre-processors can expand an event into derived events before rule evaluation.

## DecisionPlan (Compiled Rule Pipeline)

Rules and phases compile into a linear evaluation plan (see `decision-plan-and-acceleration.md`). Optimization flags may group, filter, and mask entries without changing semantics.

## Movement Patterns

Direction, MultiDirection, Fixed, (experimental compiled IR) produce candidate paths. Sliding fast-path & compiled patterns preserve semantics (movement charter). See `movement-and-patterns.md`.

## Determinism & Hashing

Optional hashing + RNG fingerprinting provide replay validation; identical inputs produce identical state hashes. See `determinism-rng-timeline.md`.

## Extension Surface

Add: new event, mutator, condition, phase wiring, or pattern type. Avoid relying on internal acceleration data structures; treat `EngineCapabilities.PathResolver` + topology as the boundary.

## Error & Rejection Handling

`HandleEventResult` returns structured reasons (see `diagnostics.md` table). Construction & invariant breaches throw specialized exceptions.
