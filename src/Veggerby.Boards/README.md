# Veggerby.Boards

Immutable, deterministic board game engine primitives: artifacts (Board, Tile, Piece, Player, Dice), immutable `GameState` history chain, declarative events, rules, phases, and safe transition execution.

> This is the foundational package. Other domain/game modules (Chess, Go, Backgammon, Checkers, Ludo, Monopoly, Risk, Cards, DeckBuilding) sit on top without extending internal mutable state.

## Install

```bash
 dotnet add package Veggerby.Boards
```

## When To Use

Use this package when you need to:

- Model a turn/phase based board game with deterministic replay
- Express moves as intent (`IGameEvent`) separated from validation and mutation
- Maintain a full immutable history for branching / analysis / undo
- Compose rules and phases declaratively

If you only need a ready-made game (e.g., Chess) and won't build custom rules, reference the game module package instead (it transitively references Core).

## Quick Start

```csharp
// Build a custom game (example sketch)
var builder = new GameBuilder("my-game");
// builder.AddBoard(...).AddPlayers(...).AddPieces(...).AddRules(...)
var progress = builder.Compile();

// Issue a move event (pseudo—depends on your defined artifacts)
var piece = progress.Game.GetPiece("white-pawn-2");
var from = progress.Game.GetTile("e2");
var to = progress.Game.GetTile("e4");
var pathVisitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
var updated = progress.HandleEvent(new MovePieceGameEvent(piece, pathVisitor.ResultPath));
```

## Core Concepts

| Concept | Summary |
|---------|---------|
| Artifact | Immutable identity created only by `GameBuilder` |
| GameState | Immutable snapshot linked to prior (persistent history) |
| GameProgress | Facade carrying `Game` + current `GameState` + handling pipeline |
| IGameEvent | Declarative intention (e.g., move piece, roll dice) |
| Rule | Couples condition evaluation with deterministic mutator |
| Phase | Conditional scope limiting which rules apply |
| Mutator | Pure function returning a new `GameState` |

## Determinism & Purity

- No randomness except through explicit dice artifacts and their state.
- Same event + same state → same result. Always.
- No hidden global or time-based influences.

## Validation Pipeline (High Level)

1. Submit `IGameEvent`
2. Active phase filters applicable rules
3. Rule conditions yield: Valid, Invalid, Ignore, NotApplicable
4. First Valid rule's mutator executes producing new `GameState`
5. New `GameProgress` returned (state chained)

## Extending

Typical extension points:

- New `IGameEvent` + corresponding mutator & rule
- New pattern visitor for movement resolution
- New composite condition types

Keep extensions explicit and side-effect free.

## Versioning

Semantic versioning aligned with repository releases. Breaking API changes bump MAJOR.

## Roadmap (Selected)

- Decision plan executor (optimized rule dispatch)
- Deterministic timeline hashing
- Movement pattern compilation (DFA)
- Optional bitboard acceleration (game-specific)

## Testing

Run the core tests:

```bash
cd test/Veggerby.Boards.Tests
dotnet test --filter "FullyQualifiedName~Core"
```

## References

- **Documentation Index**: See [/docs/index.md](../../docs/index.md) for full documentation set
- **Core Concepts**: See [/docs/core-concepts.md](../../docs/core-concepts.md) for detailed concept documentation
- **Extensibility Guide**: See [/docs/extensibility.md](../../docs/extensibility.md) for extending the engine
- **GameBuilder Guide**: See [/docs/gamebuilder-guide.md](../../docs/gamebuilder-guide.md) for creating new game modules

## Contributing

Issues & PRs: <https://github.com/veggerby/Veggerby.Boards>

Follow repository contribution guidelines and style rules.

## License

MIT License. See repository root `LICENSE`.
