# Veggerby.Boards.Ludo

Ludo/Parcheesi module for Veggerby.Boards providing complete race-to-finish board game mechanics with dice-driven movement, entry mechanics, capture rules, and safe squares built atop the immutable deterministic core engine.

> Depends on `Veggerby.Boards`. Use when you want a ready Ludo/Parcheesi ruleset or a foundation for dice-based race games.

## Install

```bash
dotnet add package Veggerby.Boards.Ludo
```

## Overview

This module provides a deterministic implementation of Ludo (Parcheesi) with:

- Circular 52-square track with 13 squares per player quadrant
- Player-specific home stretches (5 squares each)
- Entry mechanics requiring roll of 6 to enter from base
- Dice-driven movement with exact count finishing rules
- Capture mechanics with safe square immunity on starting positions
- Bonus turn on rolling 6 (configurable)
- Win condition detection (first to get all 4 pieces home)
- Support for 2-4 players (configurable)

This package does **not** implement AI, UI, or network play.

## Quick Start

```csharp
using Veggerby.Boards.Ludo;

// Create a 4-player Ludo game with bonus turns enabled
var builder = new LudoGameBuilder(playerCount: 4, bonusTurnOnSix: true);
var progress = builder.Compile();

// Roll the dice
var dice = progress.Game.GetArtifact<Dice>("dice");
progress = progress.HandleEvent(new RollDiceGameEvent<int>(new DiceState<int>(dice, 6)));

// Enter a piece from base (requires rolling a 6)
var piece = progress.Game.GetPiece("red-1");
progress = progress.HandleEvent(new EnterPieceGameEvent(piece));

// After entering, roll again and move
progress = progress.HandleEvent(new RollDiceGameEvent<int>(new DiceState<int>(dice, 4)));

// Move the piece forward
var from = progress.Game.GetTile("track-0");
var to = progress.Game.GetTile("track-4");
var pathVisitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
progress = progress.HandleEvent(new MovePieceGameEvent(piece, pathVisitor.ResultPath));

// Check if game is over
if (progress.IsGameOver())
{
    var outcome = progress.GetOutcome() as LudoOutcomeState;
    Console.WriteLine($"Winner: {outcome.Winner.Id}");
}
```

## Key Concepts

### Board Topology

The Ludo board consists of:
- **52 Track Squares**: Main circular path shared by all players
- **4 Base Areas**: Starting positions where pieces wait to enter
- **4 Home Stretches**: Player-specific paths to victory (5 squares each)
- **Safe Squares**: Starting positions immune to capture

### Dice Mechanics

- **Single Die**: Standard 6-sided die
- **Entry Requirement**: Must roll a 6 to move a piece from base onto the track
- **Bonus Turn**: Rolling a 6 grants another turn (configurable)
- **Exact Finish**: Must roll exact count to reach final home square

### Movement Rules

- **Forward Only**: Pieces move in one direction around the track
- **Home Entry**: After completing the circuit, pieces enter their home stretch
- **Exact Landing**: Must land exactly on the final home square (no overshoot)
- **Capture**: Landing on an opponent's piece sends it back to base
- **Safe Squares**: Each player's starting square is a safe zone (no captures)

### Events & Mutators

| Event | Mutator | Description |
|-------|---------|-------------|
| `RollDiceGameEvent<int>` | `DiceStateMutator<int>` | Rolls the dice, records value |
| `EnterPieceGameEvent` | `EnterPieceStateMutator` | Moves piece from base to track (requires 6) |
| `MovePieceGameEvent` | `MovePieceStateMutator` | Moves piece forward by dice value |
| - | `LudoCapturePieceStateMutator` | Captures opponent piece if applicable |
| - | `ConditionalBonusTurnStateMutator` | Grants bonus turn on rolling 6 |
| - | `LudoEndGameMutator` | Ends game when all pieces reach home |

### Conditions

| Condition | Description |
|-----------|-------------|
| `EnterPieceCondition` | Validates piece can enter (rolled 6, piece in base) |
| `PieceNotInBaseCondition` | Validates piece is on the track (not in base) |
| `ExactFinishCondition` | Validates exact roll to reach home |
| `AllPiecesHomeCondition` | Checks if all pieces of a player are home |
| `GameNotEndedCondition` | Guards actions after game termination |

### Game End

The game ends when one player moves all 4 pieces to their home squares. The `LudoOutcomeState` records the winner.

## Phases

The Ludo module uses a single gameplay phase with endgame detection:

```csharp
AddGamePhase("ludo gameplay")
    .WithEndGameDetection(
        game => new AllPiecesHomeCondition(_playerCount),
        game => new LudoEndGameMutator())
    .If<SingleActivePlayerGameStateCondition>()
        .And<GameNotEndedCondition>()
    .Then()
        .ForEvent<RollDiceGameEvent<int>>()
            // ... dice handling
        .ForEvent<EnterPieceGameEvent>()
            // ... entry handling
        .ForEvent<MovePieceGameEvent>()
            // ... movement handling
```

## Configuration Options

```csharp
// 2-player game without bonus turns
var builder = new LudoGameBuilder(playerCount: 2, bonusTurnOnSix: false);

// 4-player game with bonus turns (default)
var builder = new LudoGameBuilder();
```

## Testing

Run the module tests:

```bash
cd test/Veggerby.Boards.Tests
dotnet test --filter "FullyQualifiedName~Ludo"
```

**Test Coverage**:
- Entry mechanics (roll 6 requirement, base-to-track transition)
- Movement validation (forward direction, exact finish)
- Capture mechanics (safe square immunity, opponent send-back)
- Bonus turn logic (6 grants extra turn, configurable)
- Win condition detection (all pieces home)
- Turn rotation (player switching after moves)

## Known Limitations

- **No Blockades**: Two pieces on same square don't form a blockade
- **No Double Bonus**: Rolling multiple 6s in a row has no special handling
- **Single Piece Entry**: Cannot enter multiple pieces on same 6
- **No Shortcut Rules**: Standard track only (no shortcut variants)

## Extending This Module

Common extension scenarios:

### Adding Blockade Rules

```csharp
public sealed class BlockadeCondition : IGameStateCondition
{
    public ConditionResponse Evaluate(Game game, GameState state)
    {
        // Check if two pieces of same player on same square
        // Block opponent movement through that square
    }
}
```

### Implementing Multiple Piece Entry

```csharp
public sealed record EnterMultiplePiecesEvent(
    IReadOnlyList<Piece> Pieces) : IGameEvent;

// Allow entering multiple pieces when rolling 6
```

### Adding Time Controls

```csharp
public sealed record LudoTimeExtras(
    Dictionary<Player, TimeSpan> RemainingTime);

// Add time tracking mutators
```

## References

- **Core Documentation**: See [/docs/core-concepts.md](../../docs/core-concepts.md) for engine fundamentals
- **Phase Patterns**: See [/docs/phase-based-design.md](../../docs/phase-based-design.md) for phase design guidelines
- **Game Termination**: See [/docs/game-termination.md](../../docs/game-termination.md) for outcome patterns
- **Module Documentation**: See [/docs/ludo/index.md](../../docs/ludo/index.md) for detailed game rules

## Versioning

Semantic versioning aligned with repository releases. Breaking rule or API changes bump MAJOR.

## Contributing

Open issues & PRs at <https://github.com/veggerby/Veggerby.Boards>. Follow contributor guidelines.

## License

MIT License. See root `LICENSE`.
