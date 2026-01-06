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

Phases provide semantic structure for game flows (setup, play, endgame) and support automatic termination detection via `.WithEndGameDetection()`. See `phase-based-design.md` for canonical patterns and best practices.

## Active player projection

Avoid direct scans for ActivePlayerState. Use the centralized helpers:

- Prefer GameStateExtensions.TryGetActivePlayer(out Player) in conditions and gates where the absence of an active player should result in Ignore/NotApplicable rather than an exception.
- Use GameStateExtensions.GetActivePlayer() in strict flows and mutators that require exactly one active player and should fail fast otherwise.

See turn-sequencing.md for rotation semantics and when the active player changes.

## DecisionPlan (Compiled Rule Pipeline)

Rules and phases compile into a linear evaluation plan (see `decision-plan-and-acceleration.md`). Optimization flags may group, filter, and mask entries without changing semantics.

## Movement Patterns

Direction, MultiDirection, Fixed, (experimental compiled IR) produce candidate paths. Sliding fast-path & compiled patterns preserve semantics (movement charter). See `movement-and-patterns.md`.

## Determinism & Hashing

Optional hashing + RNG fingerprinting provide replay validation; identical inputs produce identical state hashes. See `determinism-rng-timeline.md`.

## Defensive Returns vs Invariant Exceptions

Two categories govern error signaling:

1. Resolution / query APIs (e.g., path resolution, optional lookups) may return `null` or an empty collection when a target cannot be produced. These are defensive fallbacks and must be documented.
2. State machine / lifecycle transitions (turn sequencing, builder id resolution, artifact creation) throw when invariants are violated (unknown id, unknown turn segment). Silent `null` would mask corruption and threaten deterministic replay.

Guidelines:

- Optional capabilities: prefer `Try*` pattern (`bool TryResolveX(out Y result)`). If returning `null`, ensure XML docs clarify semantics.
- Invariant breaches: throw `InvalidOperationException` or domain-specific exception early.
- Never convert invariant failures into defensive `null`.

Examples:

* `TurnProfile.Next` throws on unknown segment (invariant breach).
* Builder ID lookups use dictionary access + explicit exception on miss.
* Path resolution now prefers `TryResolvePath` with `out TilePath?` rather than nullable returns; legacy visitor samples retained only in README examples.

## Game Termination & Outcomes

`GameEndedState` is the universal marker for game termination (checked by `progress.IsGameOver()`). Game modules implement `IGameOutcome` on outcome states to provide standardized result information via `progress.GetOutcome()`.

Termination is typically integrated via phase-level endgame detection (`.WithEndGameDetection()`). See `game-termination.md` for implementation patterns and module integration guidelines.

## Player Views & Hidden Information

`GameStateView` provides filtered, player-specific projections of `GameState` for imperfect-information games. Each player or observer sees the same underlying state with appropriate visibility masking:

- **Visibility levels**: `Public` (all see), `Private` (owner only), `Hidden` (none see until revealed).
- **Default**: All states default to `Public` for backward compatibility (perfect-information games unchanged).
- **Player views**: `progress.GetViewFor(player)` returns a `GameStateView` with only visible states.
- **Observer roles**: `progress.GetObserverView(role)` supports spectator modes (`Full`, `Limited`, `PlayerPerspective`).
- **Custom policies**: Implement `IVisibilityPolicy` for game-specific visibility rules (fog of war, ownership-based filtering).

Projections are deterministic (pure functions), immutable (read-only), and performant (lazy evaluation + caching). See `player-views.md` for detailed usage examples, custom policy implementation, and migration guidance.

## Extension Surface

Add: new event, mutator, condition, phase wiring, or pattern type. Avoid relying on internal acceleration data structures; treat `EngineCapabilities.PathResolver` + topology as the boundary.

## Error & Rejection Handling

`HandleEventResult` returns structured reasons (see `diagnostics.md` table). Construction & invariant breaches throw specialized exceptions.
