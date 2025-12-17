# Veggerby.Boards.Checkers

Checkers/Draughts module for Veggerby.Boards providing standard 8×8 dark-square topology, mandatory multi-jump capture mechanics, king promotion, and endgame detection built atop the immutable deterministic core engine.

> Depends on `Veggerby.Boards`. Use when you want a ready checkers ruleset or a foundation for draughts variants.

## Install

```bash
dotnet add package Veggerby.Boards.Checkers
```

## Overview

This module provides a deterministic implementation of standard checkers (American/English draughts) with:

- Standard 8×8 board using dark squares only (32 playable squares)
- Traditional checkers numbering (1-32 for dark squares)
- Diagonal movement patterns for regular pieces and kings
- Jump captures with mandatory capture enforcement
- Multi-jump chain captures
- King promotion on reaching the back row
- Endgame detection (no pieces or no moves available)
- Support for 2 players (Black vs White)

This package does **not** implement AI, UI, or network play.

## Quick Start

```csharp
using Veggerby.Boards.Checkers;

// Create a standard checkers game
var builder = new CheckersGameBuilder();
var progress = builder.Compile();

// Get a piece and move it
var piece = progress.Game.GetPiece(CheckersIds.Pieces.BlackPiece9);
var from = progress.Game.GetTile(CheckersIds.Tiles.Tile9);
var to = progress.Game.GetTile(CheckersIds.Tiles.Tile14);

// Resolve the move path
var pathVisitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
piece.Patterns.Single().Accept(pathVisitor);
progress = progress.HandleEvent(new MovePieceGameEvent(piece, pathVisitor.ResultPath));

// Check for captures (jump moves)
var jumpFrom = progress.Game.GetTile(CheckersIds.Tiles.Tile14);
var jumpTo = progress.Game.GetTile(CheckersIds.Tiles.Tile23); // Jump over tile 18
// ... execute jump capture

// Check if game is over
if (progress.IsGameOver())
{
    var outcome = progress.GetOutcome() as CheckersOutcomeState;
    Console.WriteLine($"Winner: {outcome.Winner?.Id ?? "Draw"}");
}
```

## Key Concepts

### Board Topology

The checkers board uses dark squares only with standard numbering:

```
Row 8:  29  30  31  32     BLACK PROMOTION ROW
Row 7:    25  26  27  28
Row 6:  21  22  23  24
Row 5:    17  18  19  20
Row 4:  13  14  15  16
Row 3:    9   10  11  12
Row 2:  5   6   7   8
Row 1:    1   2   3   4    WHITE PROMOTION ROW
```

- **32 Dark Squares**: Standard checkers playable squares
- **Black Pieces**: Start on tiles 1-12 (rows 1-3)
- **White Pieces**: Start on tiles 21-32 (rows 6-8)
- **Diagonal Movement**: All moves follow diagonal paths (NE, NW, SE, SW)

### Movement Patterns

**Regular Pieces**:
- Move forward diagonally only (one square)
- Black pieces move toward row 8 (SE/SW directions)
- White pieces move toward row 1 (NE/NW directions)
- Capture by jumping over opponent piece to empty square

**Kings**:
- Move diagonally in any direction (NE, NW, SE, SW)
- Created when a piece reaches the opponent's back row
- Can capture in any diagonal direction

### Capture Rules

- **Mandatory Capture**: If a capture is available, it must be taken
- **Multi-Jump**: A piece can chain multiple jumps in a single turn
- **Longest Capture**: When multiple capture sequences exist, the longest must be taken
- **Jump Landing**: Must land on empty square beyond captured piece

### Piece Metadata

Each piece carries metadata identifying its role and color:

```csharp
public sealed record CheckersPieceMetadata(
    CheckersPieceRole Role,    // Regular or King
    CheckersPieceColor Color); // Black or White
```

### Events & Mutators

| Event | Mutator | Description |
|-------|---------|-------------|
| `MovePieceGameEvent` | `MovePieceStateMutator` | Moves piece diagonally |
| - | `CheckersCapturePieceStateMutator` | Removes captured pieces from jumps |
| - | `PromoteToKingMutator` | Promotes piece to king on back row |
| - | `NextPlayerStateMutator` | Switches active player |

### Conditions

| Condition | Description |
|-----------|-------------|
| `KingMovementCondition` | Restricts regular pieces to forward moves only |
| `MandatoryCaptureCondition` | Enforces mandatory capture when available |
| `DestinationIsEmptyGameEventCondition` | Validates landing square is empty |
| `CheckersEndgameCondition` | Detects win/draw conditions |
| `GameNotEndedCondition` | Guards actions after game termination |

### Endgame Detection

The game ends when:
- **Victory**: Opponent has no pieces remaining
- **Victory**: Opponent has no legal moves available
- **Draw**: (Rare) Agreed draw or repetition (not automatically enforced)

## Phases

The Checkers module uses a single play phase with integrated capture and promotion:

```csharp
AddGamePhase("play")
    .WithEndGameDetection(
        game => new CheckersEndgameCondition(game),
        game => new CheckersEndGameMutator(game))
    .If<GameNotEndedCondition>()
    .Then()
        .ForEvent<MovePieceGameEvent>()
            .If<PieceIsActivePlayerGameEventCondition>()
                .And(game => new KingMovementCondition(game))
                .And(game => new MandatoryCaptureCondition(game))
                .And<DestinationIsEmptyGameEventCondition>()
            .Then()
                .Do<MovePieceStateMutator>()
                .Do(game => new CheckersCapturePieceStateMutator(game))
                .Do(game => new PromoteToKingMutator(game))
                .Do(game => new NextPlayerStateMutator(...));
```

## Board ASCII Rendering

```csharp
// CheckersBoardRenderer provides ASCII visualization
var renderer = new CheckersBoardRenderer(progress.Game);
string board = renderer.Render(progress.State);
Console.WriteLine(board);
```

Example output:
```
  +---+---+---+---+---+---+---+---+
8 |   | b |   | b |   | b |   | b |
  +---+---+---+---+---+---+---+---+
7 | b |   | b |   | b |   | b |   |
  +---+---+---+---+---+---+---+---+
...
```

## Testing

Run the module tests:

```bash
cd test/Veggerby.Boards.Tests
dotnet test --filter "FullyQualifiedName~Checkers"
```

**Test Coverage**:
- Board topology (32-square dark-square layout)
- Diagonal movement validation
- Regular piece forward-only movement
- King movement in all directions
- Jump capture mechanics
- Multi-jump chain captures
- Mandatory capture enforcement
- King promotion on back row
- Endgame detection
- ASCII board rendering

## Known Limitations

- **Mandatory Capture**: Full mandatory capture logic not yet implemented (currently allows all moves)
- **No Legal Moves Detection**: Endgame detection only checks for zero pieces, not absence of valid moves
- **Multi-Jump Chains**: Only single-jump captures are handled; multi-jump sequences beyond 2 relations not implemented
- **International Draughts**: 10×10 variant not implemented
- **Flying Kings**: Kings limited to single-square moves (no long-range)
- **Huffing**: Historical "huffing" rule not implemented
- **Draw Detection**: Threefold repetition not automatically enforced
- **Tournament Rules**: Time controls not implemented

## Extending This Module

Common extension scenarios:

### Adding International Draughts (10×10)

```csharp
public class InternationalCheckersGameBuilder : GameBuilder
{
    protected override void Build()
    {
        // 50 dark squares instead of 32
        // Flying kings (multi-square diagonal moves)
        // Different starting positions
    }
}
```

### Implementing Flying Kings

```csharp
public class FlyingKingMovementCondition : IGameEventCondition<MovePieceGameEvent>
{
    public ConditionResponse Evaluate(Game game, GameState state, MovePieceGameEvent @event)
    {
        // Allow kings to move multiple squares diagonally
        // Similar to bishop in chess
    }
}
```

### Adding Time Controls

```csharp
public sealed record CheckersTimeExtras(
    TimeSpan BlackTime,
    TimeSpan WhiteTime);

// Add time tracking mutators
```

### Adding Draw Detection

```csharp
public sealed class RepetitionDrawCondition : IGameStateCondition
{
    public ConditionResponse Evaluate(Game game, GameState state)
    {
        // Track position history
        // Detect threefold repetition
    }
}
```

## References

- **Core Documentation**: See [/docs/core-concepts.md](../../docs/core-concepts.md) for engine fundamentals
- **Phase Patterns**: See [/docs/phase-based-design.md](../../docs/phase-based-design.md) for phase design guidelines
- **Game Termination**: See [/docs/game-termination.md](../../docs/game-termination.md) for outcome patterns
- **Movement Patterns**: See [/docs/movement-and-patterns.md](../../docs/movement-and-patterns.md) for pattern system details

## Versioning

Semantic versioning aligned with repository releases. Breaking rule or API changes bump MAJOR.

## Contributing

Open issues & PRs at <https://github.com/veggerby/Veggerby.Boards>. Follow contributor guidelines.

## License

MIT License. See root `LICENSE`.
