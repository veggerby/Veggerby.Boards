# Veggerby.Boards

Composable .NET board game engine: define boards (graphs), artifacts (pieces, dice, players), immutable state, and rule-driven phases for turn progression.

**Current Game Modules**:
- **Chess** – Complete move legality, castling, en passant, promotion, checkmate/stalemate detection
- **Go** – Stone placement, capture mechanics, ko rule, territory scoring
- **Backgammon** – Board topology, dice mechanics, checker movement, bearing-off
- **Checkers** – Dark-square topology, mandatory captures, king promotion
- **Ludo** – Dice-driven race game with entry mechanics and capture rules
- **Monopoly** – Property ownership, rent calculation, jail mechanics, auctions, trading
- **Risk** – Territory conquest with multi-dice combat and reinforcement mechanics
- **Cards** – Deterministic decks, piles, shuffle, draw, discard
- **DeckBuilding** – Supply management, player zones, phase-based gameplay

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
Game Modules (Chess, Go, Backgammon, Checkers, Ludo, Monopoly, Risk, Cards, DeckBuilding)
                ↓
            Core (artifacts • state • phases • rules)
```

More in `docs/architecture.md`.

## Game Modules

| Module | Package | Description |
|--------|---------|-------------|
| [Chess](src/Veggerby.Boards.Chess/README.md) | `Veggerby.Boards.Chess` | Complete chess with move legality and SAN notation |
| [Go](src/Veggerby.Boards.Go/README.md) | `Veggerby.Boards.Go` | Go (Weiqi/Baduk) with capture and scoring |
| [Backgammon](src/Veggerby.Boards.Backgammon/README.md) | `Veggerby.Boards.Backgammon` | Backgammon with dice and bearing-off |
| [Checkers](src/Veggerby.Boards.Checkers/README.md) | `Veggerby.Boards.Checkers` | Standard checkers with mandatory captures |
| [Ludo](src/Veggerby.Boards.Ludo/README.md) | `Veggerby.Boards.Ludo` | Ludo/Parcheesi race game |
| [Monopoly](src/Veggerby.Boards.Monopoly/README.md) | `Veggerby.Boards.Monopoly` | Economic property management game |
| [Risk](src/Veggerby.Boards.Risk/README.md) | `Veggerby.Boards.Risk` | Territory conquest with combat resolution |
| [Cards](src/Veggerby.Boards.Cards/README.md) | `Veggerby.Boards.Cards` | Deterministic card/deck primitives |
| [DeckBuilding](src/Veggerby.Boards.DeckBuilding/README.md) | `Veggerby.Boards.DeckBuilding` | Deck-building game foundation |

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

### Running Tests

The repository includes a `.runsettings` file with test timeout configuration to prevent hanging tests:

```bash
# Run all tests with the configured settings
dotnet test --settings .runsettings

# Run specific test project
dotnet test test/Veggerby.Boards.Tests --settings .runsettings

# Run with filter
dotnet test --settings .runsettings --filter "FullyQualifiedName~Determinism"
```

The `.runsettings` file configures:
- **Test session timeout**: 5 minutes (accommodates Debug builds; Release typically completes in ~3 minutes)
- **Serial execution**: `MaxCpuCount=1` to prevent deadlocks (see below)
- **Platform**: x64
- **Framework**: net9.0

**Important**: Tests must run serially (`MaxCpuCount=1`) to prevent deadlocks. The `FeatureFlagScope` test infrastructure uses a static semaphore that causes deadlocks when test assemblies run in parallel. This is a known limitation that will be addressed in a future refactoring.

For CI/CD environments or local debugging, you can override settings via command line:
```bash
dotnet test -- RunConfiguration.TestSessionTimeout=600000
```

### Benchmarks

Run all performance benchmarks (dynamic discovery). The script no longer generates the consolidated markdown report—use the harness for that.

📊 **Performance Summary**: See [docs/performance/benchmark-summary.md](docs/performance/benchmark-summary.md) for key metrics and historical tracking.

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
