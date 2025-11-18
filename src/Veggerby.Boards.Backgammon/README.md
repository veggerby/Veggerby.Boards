# Veggerby.Boards.Backgammon

Backgammon module for Veggerby.Boards providing standard board layout, dice mechanics, checker movement, and bearing-off logic built atop the immutable deterministic core engine.

> Depends on `Veggerby.Boards`. Use when you want a ready Backgammon ruleset or a template for similarly structured race games.

## Install

```bash
dotnet add package Veggerby.Boards.Backgammon
```

## Overview

This module provides a deterministic implementation of Backgammon with:

- Standard 24-point board with bar and bearing-off areas
- Directional movement based on player orientation
- Dice pair artifacts with pip count validation
- Rule conditions for active player and dice usage
- Move validation based on point occupancy
- Doubling cube scaffolding (for match play extension)

This package does **not** implement AI, UI, or network play.

## Quick Start

```csharp
using Veggerby.Boards.Backgammon;

// Create a standard Backgammon game
var builder = new BackgammonGameBuilder();
var progress = builder.Compile();

// Roll dice (example - actual event signature may vary)
// progress = progress.HandleEvent(new RollDiceEvent(dice1, value1));
// progress = progress.HandleEvent(new RollDiceEvent(dice2, value2));

// Move a checker (illustrative; ensure you resolve a legal path first)
var piece = progress.Game.GetPiece("white-checker-1");
var from = progress.Game.GetTile("point-24");
var to = progress.Game.GetTile("point-18");  // Move 6 pips

// Resolve path based on movement pattern and board topology
var pathVisitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
progress = progress.HandleEvent(new MovePieceGameEvent(piece, pathVisitor.ResultPath));
```

## Key Concepts

### Board Topology

The Backgammon board consists of:
- **24 Points**: Numbered 1-24, organized into four quadrants
- **Bar**: Holding area for hit checkers
- **Bearing-off Area**: Target area for checkers leaving the board
- **Directional Movement**: White moves from 24→1, Black moves from 1→24

### Dice Mechanics

- **Dice Pair**: Two six-sided dice artifacts
- **Pip Count**: Movement distance determined by dice values
- **Doubles**: Rolling the same value on both dice allows four moves of that pip count
- **Composite State**: Dice state enables validation of pip usage across multiple moves

### Movement Rules

- **Occupancy**: Can only move to points with ≤1 opponent checker
- **Entering from Bar**: Must enter before making other moves
- **Bearing Off**: All checkers must be in home board before bearing off
- **Exact Roll**: Bearing off requires exact pip count or higher (if no checkers on higher points)

### Doubling Cube (Scaffolding)

The doubling cube is included as a structural artifact for future match play implementation:
- Current value determines stake multiplier
- Offering/accepting cube requires additional events and conditions
- Scaffolding present but not fully wired for gameplay

## Phases

The Backgammon module uses a simple play phase structure:

```csharp
AddGamePhase("play")
    .Then()
        .All()
        .ForEvent<RollDiceGameEvent>()
            .If<ActivePlayerCondition>()
            .Then()
                .Do<DiceStateMutator>()
        .ForEvent<MovePieceGameEvent>()
            .If<ValidMoveCondition>()
            .Then()
                .Do<MovePieceStateMutator>();
```

## Testing

Run the module tests:

```bash
cd test/Veggerby.Boards.Tests
dotnet test --filter "FullyQualifiedName~Backgammon"
```

## Known Limitations

- **Match Scoring**: Match-level scoring and Crawford rule not implemented
- **Doubling Cube**: Cube offering/acceptance events not fully wired
- **Automatic Moves**: No auto-play for forced moves
- **Bearing Off Validation**: Exact/higher pip validation needs comprehensive testing
- **Crawford Rule**: Match play variation not implemented

## Extending This Module

Common extension scenarios:

### Adding Match Scoring Layer

```csharp
public sealed record BackgammonMatchExtras(
    int MatchLength,
    Dictionary<Player, int> MatchScore,
    bool IsCrawfordGame
);

// Add match-level mutators and win conditions
```

### Implementing Cube Offering

```csharp
public sealed record OfferDoubleEvent(Player Offerer) : IGameEvent;
public sealed record AcceptDoubleEvent(Player Acceptor) : IGameEvent;
public sealed record DeclineDoubleEvent(Player Decliner) : IGameEvent;

// Add cube state mutators and ownership tracking
```

### Alternate Starting Positions

```csharp
public class BackgammonNackgammonBuilder : BackgammonGameBuilder
{
    protected override void Build()
    {
        // Custom initial checker placement
        // (Nackgammon variation uses different starting positions)
    }
}
```

## References

- **Core Documentation**: See [/docs/core-concepts.md](../../docs/core-concepts.md) for engine fundamentals
- **Phase Patterns**: See [/docs/phase-based-design.md](../../docs/phase-based-design.md) for phase design guidelines
- **Dice & Randomness**: See [/docs/determinism-rng-timeline.md](../../docs/determinism-rng-timeline.md) for RNG handling
- **Backgammon Rules**: External reference for official rules

## Versioning

Follows repository semantic version. Backwards incompatible rule changes bump MAJOR.

## Contributing

Open issues & PRs at <https://github.com/veggerby/Veggerby.Boards>. Follow contribution guidelines and code style.

## License

MIT License. See root `LICENSE`.
