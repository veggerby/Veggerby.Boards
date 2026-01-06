# Legal Move Generation API

## Overview

The Legal Move Generation API provides a standard, core-level interface to enumerate and validate legal game events across all game modules. This enables:

- **AI Development**: Agents can query legal moves without module-specific code
- **UI/UX**: Highlight valid moves, show rejection explanations
- **Game Analysis**: Generate move trees, compute branching factors
- **Testing**: Validate rule completeness via exhaustive legal move generation
- **Developer Experience**: Consistent API across all game modules

## Core Interfaces

### ILegalMoveGenerator

The main interface for legal move generation:

```csharp
public interface ILegalMoveGenerator
{
    IEnumerable<IGameEvent> GetLegalMoves(GameState state);
    MoveValidation Validate(IGameEvent @event, GameState state);
    IEnumerable<IGameEvent> GetLegalMovesFor(Artifact artifact, GameState state);
}
```

### MoveValidation

Result of move validation with structured diagnostics:

```csharp
public sealed record MoveValidation(
    bool IsLegal, 
    IGameEvent Event, 
    RejectionReason Reason, 
    string Explanation);
```

### RejectionReason

Structured rejection reasons for diagnostics:

- `None` - No rejection; move is legal
- `PieceNotOwned` - Artifact doesn't belong to the active player
- `PathObstructed` - Movement path is blocked
- `DestinationOccupied` - Destination occupied by friendly artifact
- `InvalidPattern` - Move doesn't match movement pattern
- `WrongPhase` - Action not allowed in current phase
- `InsufficientResources` - Not enough resources (dice, cards, etc.)
- `RuleViolation` - Game-specific rule violation
- `GameEnded` - Game has already ended

## Usage

### Basic Usage

```csharp
// Get the legal move generator for a game
var generator = progress.GetLegalMoveGenerator();

// Enumerate all legal moves
var legalMoves = generator.GetLegalMoves(progress.State);
foreach (var move in legalMoves)
{
    Console.WriteLine($"Legal: {move}");
}

// Validate a specific move
var moveToTest = new MovePieceGameEvent(piece, path);
var validation = generator.Validate(moveToTest, progress.State);
if (!validation.IsLegal)
{
    Console.WriteLine($"Illegal: {validation.Explanation} ({validation.Reason})");
}

// Get legal moves for a specific piece
var pieceMoves = generator.GetLegalMovesFor(piece, progress.State);
```

### Chess Example

```csharp
var builder = new ChessGameBuilder();
var progress = builder.Compile();
var generator = progress.GetLegalMoveGenerator();

// Starting position has 20 legal moves
var legalMoves = generator.GetLegalMoves(progress.State).ToList();
// 16 pawn moves (8 pawns × 2 moves each) + 4 knight moves (2 knights × 2 moves each)

// Validate a specific move (e2-e4)
var e2Pawn = progress.Game.GetPiece("white-pawn-5"); // e-file pawn
var toTile = progress.Game.GetTile("tile-e4");
var path = /* resolve path using pattern visitor */;
var move = new MovePieceGameEvent(e2Pawn, path);

var validation = generator.Validate(move, progress.State);
if (validation.IsLegal)
{
    progress = progress.HandleEvent(move);
}
```

### AI Example (Random Agent)

```csharp
public class RandomAgent
{
    public GameProgress MakeMove(GameProgress progress)
    {
        var generator = progress.GetLegalMoveGenerator();
        var legalMoves = generator.GetLegalMoves(progress.State).ToList();

        if (!legalMoves.Any())
        {
            Console.WriteLine("No legal moves - game over or stuck");
            return progress;
        }

        var randomMove = legalMoves[Random.Shared.Next(legalMoves.Count)];
        return progress.HandleEvent(randomMove);
    }
}
```

## Implementation

### DecisionPlanMoveGenerator

The base implementation uses the compiled `DecisionPlan` for efficient validation:

```csharp
public class DecisionPlanMoveGenerator : ILegalMoveGenerator
{
    protected GameEngine Engine { get; }
    protected GameState State { get; }

    public virtual MoveValidation Validate(IGameEvent @event, GameState state)
    {
        // Uses DecisionPlan to evaluate conditions
        // Maps ConditionResponse to RejectionReason
        // Provides structured diagnostics
    }

    protected virtual IEnumerable<IGameEvent> GenerateCandidates(GameState state, GamePhase phase)
    {
        // Extensibility point for module-specific candidate generation
        // Override in subclasses to provide actual move candidates
    }
}
```

### Chess Integration

The Chess module provides a specialized generator:

```csharp
public sealed class ChessLegalMoveGenerator : ILegalMoveGenerator
{
    // Integrates ChessMoveGenerator (pseudo-legal moves)
    // Applies ChessLegalityFilter (king safety check)
    // Converts PseudoMove to MovePieceGameEvent
}
```

## Rejection Reason Mappings

The following table shows how `ConditionResponse` reasons map to `RejectionReason`:

| Condition Message Pattern | Rejection Reason |
|---------------------------|------------------|
| "not owned", "not your", "active player" | `PieceNotOwned` |
| "obstructed", "blocked" | `PathObstructed` |
| "occupied", "not empty" | `DestinationOccupied` |
| "no valid path", "invalid pattern", "movement pattern" | `InvalidPattern` |
| "phase" | `WrongPhase` |
| "dice", "insufficient" | `InsufficientResources` |
| (default) | `RuleViolation` |

## Extensibility

### Module-Specific Generators

Game modules can provide specialized generators by:

1. Subclassing `DecisionPlanMoveGenerator`
2. Overriding `GenerateCandidates` to provide module-specific move enumeration
3. Optionally overriding `EventInvolvesArtifact` for artifact-specific filtering

Example:

```csharp
public class MyGameLegalMoveGenerator : DecisionPlanMoveGenerator
{
    protected override IEnumerable<IGameEvent> GenerateCandidates(GameState state, GamePhase phase)
    {
        // Module-specific candidate generation logic
        foreach (var piece in GetActivePieces(state))
        {
            foreach (var destination in GetValidDestinations(piece, state))
            {
                yield return new MovePieceGameEvent(piece, destination);
            }
        }
    }
}
```

### Auto-Detection

The `GetLegalMoveGenerator()` extension method automatically detects game modules and returns the appropriate generator implementation. Currently supports:

- **Chess**: Returns `ChessLegalMoveGenerator` when Chess state extras are detected
- **Default**: Returns `DecisionPlanMoveGenerator` for all other games

## Performance

### Targets

- **Chess mid-game**: < 1ms for ~30-40 legal moves
- **Go 19x19 empty board**: < 5ms for ~361 legal placements
- **Validation**: O(1) for single move validation (uses precompiled DecisionPlan)

### Optimization Notes

- Uses lazy evaluation via `IEnumerable<IGameEvent>`
- Avoids LINQ in hot paths for allocation efficiency
- Leverages precompiled `DecisionPlan` to avoid repeated rule traversal
- Module-specific generators can optimize candidate generation

## Determinism

The API guarantees determinism:

- Same `GameState` → same legal moves (modulo ordering)
- Same event + state → same validation result
- No hidden randomness (dice values are part of state)

Ordering guarantees:

- Base implementation: **no specific ordering** (performance-optimized)
- Module implementations **may** provide deterministic ordering

## Testing

### Example Tests

```csharp
[Fact]
public void GivenChessMove_WhenValidatingLegalMove_ThenReturnsLegal()
{
    var progress = new ChessGameBuilder().Compile();
    var generator = progress.GetLegalMoveGenerator();
    var move = CreateMoveEvent(progress, "white-pawn-5", "tile-e4");

    var validation = generator.Validate(move, progress.State);

    validation.IsLegal.Should().BeTrue();
    validation.Reason.Should().Be(RejectionReason.None);
}

[Fact]
public void GivenEndedGame_WhenValidatingMove_ThenReturnsGameEnded()
{
    var progress = new ChessGameBuilder().Compile();
    var move = CreateMoveEvent(progress, "white-pawn-5", "tile-e4");
    var endedState = progress.State.Next([new GameEndedState()]);
    var generator = progress.GetLegalMoveGenerator();

    var validation = generator.Validate(move, endedState);

    validation.IsLegal.Should().BeFalse();
    validation.Reason.Should().Be(RejectionReason.GameEnded);
}
```

## Limitations

### Current Scope

- ✅ Move validation with structured diagnostics
- ✅ Chess move enumeration via ChessLegalMoveGenerator
- ⚠️ Generic move enumeration requires module-specific candidate generators

### Future Enhancements

- Candidate generator registration system
- Move ordering heuristics (for AI alpha-beta pruning)
- Transposition table integration
- Performance instrumentation

## Related Documentation

- [Decision Plan and Acceleration](/docs/decision-plan-and-acceleration.md)
- [Phase-Based Design](/docs/phase-based-design.md)
- [Game Termination](/docs/game-termination.md)
- [Movement and Patterns](/docs/movement-and-patterns.md)

## Migration Guide

### From ChessEndgameDetector

Before:

```csharp
var detector = new ChessEndgameDetector(game);
var pseudoLegalMoves = detector.GetPseudoLegalMoves(state, piece);
```

After:

```csharp
var generator = progress.GetLegalMoveGenerator();
var legalMoves = generator.GetLegalMovesFor(piece, progress.State);
// Note: These are fully legal (not pseudo-legal), including king safety checks
```

### From Try-Catch Enumeration

Before:

```csharp
foreach (var tile in board.Tiles)
{
    try
    {
        var testProgress = progress.HandleEvent(new PlaceStoneGameEvent(stone, tile));
        // Move was legal
    }
    catch
    {
        // Move was illegal, but why?
    }
}
```

After:

```csharp
var generator = progress.GetLegalMoveGenerator();
foreach (var tile in board.Tiles)
{
    var candidateMove = new PlaceStoneGameEvent(stone, tile);
    var validation = generator.Validate(candidateMove, progress.State);
    if (validation.IsLegal)
    {
        progress = progress.HandleEvent(candidateMove);
    }
    else
    {
        Console.WriteLine($"Illegal: {validation.Explanation} ({validation.Reason})");
    }
}
```

## Summary

The Legal Move Generation API provides a unified, efficient interface for discovering and validating legal moves across all game modules. It integrates seamlessly with the existing `DecisionPlan` infrastructure while providing extensibility for module-specific optimizations.

Key benefits:

- **Consistency**: Same API across all games
- **Diagnostics**: Structured rejection reasons
- **Performance**: Leverages precompiled decision plans
- **Extensibility**: Module-specific generators via subclassing
- **Determinism**: Guaranteed same input → same output
