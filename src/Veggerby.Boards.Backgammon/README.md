# Veggerby.Boards.Backgammon

Backgammon module for Veggerby.Boards. Provides artifacts, rules, dice handling, and setup logic built atop the immutable deterministic core engine.

> Depends on `Veggerby.Boards` (transitively referenced when installing this package). Use it when you want a ready Backgammon ruleset or a template for similarly structured race games.

## Install

```bash
 dotnet add package Veggerby.Boards.Backgammon
```

## Scope

This package adds:

- Backgammon game builder (`BackgammonGameBuilder`)
- Standard initial board layout
- Dice artifacts (pair) and doubling cube scaffolding
- Rule conditions (e.g., active player, dice usage) and move validation

It does NOT implement AI, UI, or network play.

## Quick Start

```csharp
var builder = new BackgammonGameBuilder();
var progress = builder.Compile();

// Roll dice (example event might differ once dice events are formalized)
// progress = progress.HandleEvent(new RollDiceEvent());

// Move a checker (illustrative; ensure you resolve a legal path first)
// var piece = progress.Game.GetPiece("white-checker-1");
// var from = progress.Game.GetTile("point-24");
// var to = progress.Game.GetTile("point-18");
// var pathVisitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
// progress = progress.HandleEvent(new MovePieceGameEvent(piece, pathVisitor.ResultPath));
```

## Key Concepts (Backgammon Specific)

- Multi-dice handling: composite dice state enabling pip count validation
- Directional movement relative to player orientation
- Bearing-off readiness validation
- Doubling cube (placeholder scaffolding for match play)

## Interop With Core

All state transitions remain immutable. Backgammon-specific logic only orchestrates existing engine primitives (events, rules, phases, mutators).

## Extending This Module

Examples:

- Add match scoring layer above game state
- Introduce alternate starting positions
- Implement cube offering / acceptance events

Maintain determinism: no hidden random sources beyond dice artifacts.

## Versioning

Follows repository semantic version. Backwards incompatible rule changes bump MAJOR.

## Contributing

Open issues & PRs at <https://github.com/veggerby/Veggerby.Boards>. Follow contribution guidelines and code style.

## License

MIT License. See root `LICENSE`.
