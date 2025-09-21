# Veggerby.Boards

Composable .NET board game engine: define boards (graphs), artifacts (pieces, dice, players), immutable state, and rule-driven phases for turn progression. Current example modules: Backgammon & Chess.

## Highlights

- Immutable structural model (Board, Tiles, Relations, Pieces, Dice, Players)
- Pattern-based movement (directional, repeatable, fixed sequences)
- Hierarchical phase system (finite-state style) with conditions and event pre-processing
- Deterministic, persistent state transitions (history retained)
- Extensible: add events, mutators, conditions, phases without modifying core

## Quick Start

```csharp
// Backgammon
var builder = new BackgammonGameBuilder();
var progress = builder.Compile(); // GameEngine + initial GameState

// Roll dice (demo numbers)
var d1 = progress.Game.GetArtifact<Dice>("dice-1");
var d2 = progress.Game.GetArtifact<Dice>("dice-2");
progress = progress.HandleEvent(new RollDiceGameEvent<int>(new DiceState<int>(d1, 3), new DiceState<int>(d2, 1)));

// Move a piece using resolved path
var piece = progress.Game.GetPiece("white-1");
var from = progress.Game.GetTile("point-1");
var to = progress.Game.GetTile("point-5");
var resolver = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
piece.Patterns.Single().Accept(resolver);
progress = progress.HandleEvent(new MovePieceGameEvent(piece, resolver.ResultPath));
```

## Core Concepts

| Concept | Summary |
|---------|---------|
| Artifact | Immutable identity object (Piece, Dice, Board, Player, Tile) |
| Game | Structural aggregate (no mutable state) |
| GameState | Snapshot of artifact states (piece positions, dice values) |
| Event | Intent (MovePiece, RollDice, etc.) |
| Mutator | Pure state transition logic |
| Condition | Gate phase activation or event validity |
| Rule | Binds event conditions + mutators |
| Phase | Conditional scope with optional pre-processors |
| Builder | Declarative compilation of structure + initial state + phases |

See `docs/core-concepts.md` for details.

## Architecture

Layered solution:

```txt
Game Modules (Backgammon, Chess)
                ↓
            Core (artifacts • state • phases • rules)
                ↓
            API (demo HTTP exposure)
```

More in `docs/architecture.md`.

## Extending

1. Create a new `GameBuilder` subclass.
2. Define tiles + relations, directions, players, pieces (patterns), dice.
3. Specify initial placements / dice states.
4. Add phases and rules: `.ForEvent<MovePieceGameEvent>().Then().Do<MovePieceStateMutator>()`.
5. Compile and handle events.

Step-by-step: `docs/extensibility.md`.

## API Demo

`Veggerby.Boards.Api` exposes a sample endpoint:

```txt
GET /api/games/{guid}
    endsWith("1") => Backgammon (with simulated dice + move)
    otherwise     => Chess initial state
```

Models map engine objects to simple DTOs (tiles, pieces, dice values, active player, etc.).

Details: `docs/api-layer.md`.

## Development

```bash
dotnet restore
dotnet build
dotnet test
```

## Contributing

Guidelines in `CONTRIBUTING.md` (focus on immutability, deterministic state, small mutators, full test coverage for new rule branches).

## License

MIT

## Documentation Index

See `docs/index.md` for full documentation set.
