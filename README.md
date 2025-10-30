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

Layered solution (current repositories focus on engine + modules; HTTP facade removed for now):

```txt
Game Modules (Backgammon, Chess)
                ↓
            Core (artifacts • state • phases • rules)
```

More in `docs/architecture.md`.

## Extending

1. Create a new `GameBuilder` subclass.
2. Define tiles + relations, directions, players, pieces (patterns), dice.
3. Specify initial placements / dice states.
4. Add phases and rules: `.ForEvent<MovePieceGameEvent>().Then().Do<MovePieceStateMutator>()`.
5. Compile and handle events.

Step-by-step: `docs/extensibility.md`.

<!-- API layer temporarily removed; previous demo HTTP facade will return later as an optional package. -->

## Development

```bash
dotnet restore
dotnet build
dotnet test
```

### Benchmarks

Run all performance benchmarks (dynamic discovery). The script no longer generates the consolidated markdown report—use the harness for that.

```bash
./scripts/run-all-benchmarks.sh
```

Faster short job iteration:

```bash
BENCH_JOB="--job short" ./scripts/run-all-benchmarks.sh
```

Filter to a specific benchmark:

```bash
BENCH_FILTER="*SlidingAttackGeneratorCapacityBenchmark*" ./scripts/run-all-benchmarks.sh

Generate consolidated markdown report (harness):

```bash
dotnet run -c Release --project benchmarks/Veggerby.Boards.Benchmarks.csproj -- --generate-report --report-out ./perf-out
```

Filter a specific benchmark (quote patterns to prevent shell glob expansion):

```bash
dotnet run -c Release --project benchmarks/Veggerby.Boards.Benchmarks.csproj -- --filter '*BitboardIncrementalBenchmark*' --generate-report -o ./benchmarks/docs/bitboard-incremental.md
```

Short flag aliases (harness):

- `-g` for `--generate-report`
- `-o` for `--report-out`

If `--report-out` / `-o` is omitted the harness writes to `docs/benchmark-results.md`.

The generated report is published alongside NuGet packages to document current latency & allocation profiles.

## Contributing

Guidelines in `CONTRIBUTING.md` (focus on immutability, deterministic state, small mutators, full test coverage for new rule branches).

## License

MIT

## Documentation Index

See `docs/index.md` for full documentation set.
