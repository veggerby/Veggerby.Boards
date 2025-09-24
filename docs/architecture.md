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

- Backgammon defines directional linear movement with dice-driven phases and multi-step conditional transitions (e.g., bar clearing, doubling logic).
- Chess defines a dense 8×8 grid with pattern-based piece movement (directional + fixed multi-step patterns).
      - Experimental compiled movement pattern subsystem (flag `EnableCompiledPatterns`) converts supported patterns
         (`FixedPattern`, `MultiDirectionPattern`) into a lightweight IR (Fixed, Ray, MultiRay) resolved by a fast
         table-driven resolver. Parity is enforced via test suite and can be inspected in `compiled-patterns.md`.

## Design Principles

- Declarative over imperative: Builders describe; engine compiles.
- Separation of static structure (Artifacts) from dynamic behavior (State + Events + Rules).
- Explicit gating via Conditions; no hidden side-effects.
- Extensible through additive types (new Mutators / Conditions / Events) without modifying core engine.

### Emerging Optimization Synergy

The DecisionPlan (phase ordering pre-compilation) and Compiled Movement Patterns are independent feature-flagged
optimizations. Future synergy work may precompute rule movement dependencies allowing direct compiled path
evaluation inside decision plan traversal, eliminating duplicate visitor passes. This remains a roadmap item
pending performance measurements.

## Extension Points

- Add new `IGameEvent` types for novel interactions.
- Introduce `IStateMutator<T>` implementations for atomic state transitions.
- Compose complex logic with `CompositeGameEventCondition` and `CompositeGameStateCondition`.
- Layer sequencing using nested `GamePhase` trees.

See `extensibility.md` for a step-by-step guide.
