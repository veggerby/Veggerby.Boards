# Veggerby.Boards.Go

Go (Weiqi/Baduk) module for Veggerby.Boards providing complete stone placement, capture mechanics, ko rule, and territory scoring built atop the immutable deterministic core engine.

> Depends on `Veggerby.Boards`. Use when you want a ready Go ruleset or a foundation for Go variants.

## Install

```bash
dotnet add package Veggerby.Boards.Go
```

## Overview

This module provides a complete, deterministic implementation of Go (Weiqi/Baduk) with:

- Board topology for standard sizes (9×9, 13×13, 19×19)
- Stone placement with liberty and suicide validation
- Capture mechanics with group detection
- Simple ko rule enforcement
- Pass turns with two-consecutive-pass game termination
- Area scoring (stones + territory)

## Quick Start

```csharp
using Veggerby.Boards.Go;

// Create a 19×19 Go game (default size)
var builder = new GoGameBuilder(size: 19);
var progress = builder.Compile();

// Get players and an available stone
var blackPlayer = progress.Game.GetPlayer("black");
var whitePlayer = progress.Game.GetPlayer("white");
var tile = progress.Game.GetTile("tile-4-4");  // D4 in Go notation

// Find an available stone for black
var blackStone = progress.State.GetStates<PieceState>()
    .Where(ps => ps.Artifact.Owner?.Id == "black" && ps.Tile == null)
    .Select(ps => ps.Artifact)
    .Cast<Piece>()
    .First();

// Place a stone
progress = progress.HandleEvent(new PlaceStoneGameEvent(blackStone, tile));

// Pass turn
progress = progress.HandleEvent(new PassTurnGameEvent());

// Check if game is over (after two consecutive passes)
if (progress.IsGameOver())
{
    var outcome = progress.GetOutcome() as GoOutcomeState;
    Console.WriteLine($"Game ended. Score - Black: {outcome.BlackScore}, White: {outcome.WhiteScore}");
}
```

## Key Concepts

### GoStateExtras

The module uses `GoStateExtras` to track:
- **KoTileId**: The tile position where ko rule is currently active (prevents immediate recapture)
- **BoardSize**: The board dimension (9, 13, or 19)

### Events & Mutators

| Event | Mutator | Description |
|-------|---------|-------------|
| `PlaceStoneGameEvent` | `PlaceStoneStateMutator` | Places a stone, handles captures, validates suicide rule, enforces ko |
| `PassTurnGameEvent` | `PassTurnStateMutator` | Passes turn, increments pass streak, ends game after two consecutive passes |

### Group Detection & Liberties

The `GroupScanner` performs iterative flood-fill to:
- Identify connected stone groups
- Count liberties (empty adjacent intersections)
- Detect which groups have zero liberties (captured)

### Capture Mechanics

When a stone is placed:
1. Check opponent groups for zero liberties → remove captured stones
2. Check own group for zero liberties (suicide) → reject placement unless it captures opponent stones

### Ko Rule

Simple ko prevents immediately recapturing a single stone:
- After a single-stone capture, the capturing position is marked in `KoTileId`
- Next move cannot place a stone at that position
- Ko restriction clears when any other move is made or a turn is passed

### Pass Mechanics

- `PassTurnGameEvent` increments the consecutive pass counter (`TurnState.PassStreak`)
- After two consecutive passes, `PassTurnStateMutator` appends `GameEndedState`
- Any stone placement resets the pass streak to zero

### Scoring

`GoScoring.AreaScore` computes final score using area scoring:
- **Stones**: Each stone on the board counts as 1 point
- **Territory**: Empty intersections surrounded by one color count for that color
- **Result**: Returns `GoOutcomeState` with black and white scores

Territory assignment uses flood-fill: if an empty region touches only one color, it belongs to that color.

## Phases

The Go module uses a single "play" phase:

```csharp
AddGamePhase("play")
    .If<NullGameStateCondition>()  // Always active
    .Then()
        .All()
        .ForEvent<PlaceStoneGameEvent>()
            .Then()
                .Do<PlaceStoneStateMutator>()
        .ForEvent<PassTurnGameEvent>()
            .Then()
                .Do<PassTurnStateMutator>();
```

## Testing

Run the module tests:

```bash
cd test/Veggerby.Boards.Tests
dotnet test --filter "FullyQualifiedName~Go"
```

**Test Coverage** (29/29 passing, 100% success rate):
- Stone placement validation (empty intersection required)
- Single and multi-stone capture mechanics
- Suicide rule enforcement (with and without capture)
- Ko rule: immediate recapture blocking, ko clearing via pass, ko clearing via play elsewhere
- Snapback scenarios (multi-stone captures don't trigger ko)
- Pass counting and double-pass termination
- Area scoring with territory assignment

## Known Limitations

- **Superko**: Only simple ko is enforced; positional superko (full-board repetition) is not detected
- **Territory vs Area Scoring**: Only area scoring is implemented; Chinese rules are assumed
- **Handicap Stones**: No built-in handicap placement helper
- **Dead Stone Adjudication**: No automatic detection of dead groups; players must play out all captures

## Extending This Module

Common extension scenarios:

### Adding Handicap Placement

```csharp
public class GoGameBuilderWithHandicap : GoGameBuilder
{
    public GoGameBuilderWithHandicap(int size, int handicap) : base(size)
    {
        // After Build(), place handicap stones on star points
        // WithPiece("black-stone-1").OnTile("tile-4-4");
        // ...
    }
}
```

### Implementing Territory Scoring Variant

```csharp
public static class GoScoringVariants
{
    public static GoOutcomeState TerritoryScore(GameState state, GoStateExtras extras)
    {
        // Count only territory (empty intersections), not stones
        // Implement Japanese-style scoring
    }
}
```

### Adding Time Controls

```csharp
public sealed record GoTimeControlExtras(
    TimeSpan BlackTime,
    TimeSpan WhiteTime,
    int ByoYomiPeriods
);

// Add time tracking mutators that validate time usage
```

## References

- **Core Documentation**: See [/docs/core-concepts.md](../../docs/core-concepts.md) for engine fundamentals
- **Phase Patterns**: See [/docs/phase-based-design.md](../../docs/phase-based-design.md) for phase design guidelines
- **Game Termination**: See [/docs/game-termination.md](../../docs/game-termination.md) for outcome patterns
- **Go Rules**: [Sensei's Library](https://senseis.xmp.net/) for comprehensive Go rules reference

## Versioning

Semantic versioning aligned with repository releases. Breaking rule or API changes bump MAJOR.

## Contributing

Open issues & PRs at <https://github.com/veggerby/Veggerby.Boards>. Follow contributor guidelines.

## License

MIT License. See root `LICENSE`.
