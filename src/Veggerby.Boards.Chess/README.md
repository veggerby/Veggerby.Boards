# Veggerby.Boards.Chess

Chess module for Veggerby.Boards providing complete move legality, special moves (castling, en passant, promotion), checkmate/stalemate detection, and SAN notation built atop the immutable deterministic core engine.

> Depends on `Veggerby.Boards`. Use when you want a ready chess ruleset or a foundation for chess variants (house rules, timing layers, analysis tooling).

## Install

```bash
dotnet add package Veggerby.Boards.Chess
```

## Overview

This module provides a complete, deterministic implementation of chess with:

- Standard 8×8 board with all piece types
- Full move legality (pseudo-legal generation + king safety filtering)
- Special moves: castling (kingside/queenside), en passant, pawn promotion
- Check, checkmate, and stalemate detection
- Standard Algebraic Notation (SAN) with all symbols (#, +, =Q, O-O, x, e.p.)
- Immutable state history enabling analysis and replay

## Quick Start

```csharp
using Veggerby.Boards.Chess;
using Veggerby.Boards.Chess.MoveGeneration;

// Create a standard chess game
var builder = new ChessGameBuilder();
var progress = builder.Compile();

// Execute moves using SAN notation (simplest approach)
progress = progress.MoveSan("e4");   // White pawn to e4
progress = progress.MoveSan("e5");   // Black pawn to e5
progress = progress.MoveSan("Nf3");  // White knight to f3

// Or use low-level event handling
var piece = progress.Game.GetPiece("white-pawn-5");  // e-file pawn
var from = progress.Game.GetTile("e2");
var to = progress.Game.GetTile("e4");
var pathVisitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
progress = progress.HandleEvent(new MovePieceGameEvent(piece, pathVisitor.ResultPath));

// Check game status
if (progress.IsGameOver())
{
    var outcome = progress.GetOutcome() as ChessOutcomeState;
    Console.WriteLine($"Game ended: {outcome.Result}");
}

// Check for check/checkmate using detector
var detector = new ChessEndgameDetector(progress.Game);
var status = detector.GetEndgameStatus(progress.State);
if (status == EndgameStatus.Checkmate)
{
    Console.WriteLine("Checkmate!");
}
```

## Key Concepts

### ChessStateExtras

The module uses `ChessStateExtras` to track:
- **CastlingRights**: Which castling moves are still available (kingside/queenside for each player)
- **EnPassantTargetTileId**: The target square for en passant capture (if available this turn)

These extras are immutably updated after each move based on piece movements and captures.

### Events & Mutators

| Event | Mutator | Description |
|-------|---------|-------------|
| `MovePieceGameEvent` | `ChessMovePieceStateMutator` | Moves a piece, updates extras, rotates players |
| `CastlingGameEvent` | `CastlingStateMutator` | Executes castling (moves both king and rook) |
| `PromotionGameEvent` | `PromotionStateMutator` | Promotes a pawn to queen/rook/bishop/knight |
| `EnPassantCaptureGameEvent` | `EnPassantCapturePieceStateMutator` | Captures pawn en passant |

### Movement Patterns

Chess pieces use these pattern types:
- **Directional Sliding**: Rooks, bishops, queens (repeating in a direction until blocked)
- **Fixed Step**: Kings, knights (fixed offset jumps)
- **Conditional**: Pawns (forward move, diagonal capture, two-square initial, en passant)

Patterns are resolved to `TilePath` instances representing concrete move sequences.

### Move Legality

Move validation follows a two-stage process:

1. **Pseudo-legal Generation**: `ChessPseudoLegalMoveGenerator` produces all moves that follow piece movement patterns
2. **King Safety Filter**: `ChessLegalityFilter` rejects moves that leave the king in check

This ensures only legal moves are allowed while maintaining performance.

### SAN Notation

The `ChessNomenclature` class provides Standard Algebraic Notation:
- **Piece moves**: `Nf3`, `Qd4`
- **Captures**: `Nxf3`, `Qxd4`
- **Pawn moves**: `e4`, `exd5` (with file disambiguation on capture)
- **Castling**: `O-O` (kingside), `O-O-O` (queenside)
- **Promotion**: `e8=Q`
- **En passant**: `exd6 e.p.`
- **Check**: `Nf3+`
- **Checkmate**: `Qh5#`
- **Disambiguation**: `Nbd2`, `R1a3` (when multiple pieces can move to same square)

### Checkmate & Stalemate Detection

`ChessEndgameDetector` detects:
- **Check**: King is under attack
- **Checkmate**: King is in check with no legal moves
- **Stalemate**: No legal moves but king is not in check

Phase-based endgame detection is also available via `.WithEndGameDetection()` (see Phase-Based Design docs).

## Phases

The Chess module uses phase-based endgame detection by default:

```csharp
AddGamePhase("play")
    .WithEndGameDetection()  // Automatically checks for checkmate/stalemate
    .Then()
        .All()
        .ForEvent<ChessMoveEvent>()
            .If<...>()  // Move validation
            .Then()
                .Do<ChessMovePieceStateMutator>()
                .Do<NextPlayerStateMutator>();
```

Legacy `ChessEndgameDetector` is retained for backward compatibility.

## Testing

Run the module tests:

```bash
cd test/Veggerby.Boards.Tests
dotnet test --filter "FullyQualifiedName~Chess"
```

**Test Coverage** (16+ unit tests, 4 integration tests):
- Pseudo-legal move generation (all piece types)
- King safety filtering (check detection, pinned pieces)
- Castling validation (safety checks, piece movement)
- En passant capture mechanics
- Pawn promotion variants
- SAN notation generation and parsing
- Checkmate and stalemate scenarios
- Full game playability (Scholar's Mate, Immortal Game)

## Known Limitations

- **Draw Rules**: 50-move rule and threefold repetition are not enforced
- **Promotion Choice**: API supports only default queen promotion; UI-driven selection requires extension
- **PGN Support**: Import/export not implemented
- **Time Controls**: No built-in time tracking

## Extending This Module

Common extension scenarios:

### Adding Chess960 (Fischer Random)

```csharp
public class Chess960GameBuilder : ChessGameBuilder
{
    protected override void Build()
    {
        // Custom back-rank setup logic
        // ...
    }
}
```

### Implementing Draw Detection

```csharp
public sealed class FiftyMoveRuleCondition : IGameStateCondition
{
    public ConditionResponse Evaluate(Game game, GameState state)
    {
        // Track half-moves since last pawn move or capture
        // ...
    }
}
```

### Adding Time Controls

```csharp
public sealed record ChessTimeControlExtras(
    TimeSpan WhiteTime,
    TimeSpan BlackTime,
    TimeSpan Increment
);

// Add time tracking mutators
```

## References

- **Core Documentation**: See [/docs/core-concepts.md](../../docs/core-concepts.md) for engine fundamentals
- **Phase Patterns**: See [/docs/phase-based-design.md](../../docs/phase-based-design.md) for phase design guidelines
- **Game Termination**: See [/docs/game-termination.md](../../docs/game-termination.md) for outcome patterns
- **Movement Patterns**: See [/docs/movement-and-patterns.md](../../docs/movement-and-patterns.md) for pattern system details

## Versioning

Semantic versioning aligned with repository releases. Breaking movement / rule API changes bump MAJOR.

## Contributing

Open issues & PRs at <https://github.com/veggerby/Veggerby.Boards>. Follow contributor guidelines.

## License

MIT License. See root `LICENSE`.
