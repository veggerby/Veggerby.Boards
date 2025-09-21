# Veggerby.Boards.Chess

Chess module for Veggerby.Boards providing piece set, initial FEN-equivalent placement, movement patterns, and rule wiring atop the immutable deterministic core.

> Depends on `Veggerby.Boards.Core`. Use when you want a ready chess ruleset or a foundation for chess variants (house rules, timing layers, analysis tooling).

## Install

```bash
 dotnet add package Veggerby.Boards.Chess
```

## Scope

Adds:

- `ChessGameBuilder` producing a ready game with standard initial layout
- Movement pattern definitions (directional, fixed, multi-direction) consumed by visitors
- Rule conditions for basic piece movement & occupancy
- Path resolution (pattern → concrete `TilePath`)

Not yet included (roadmap): check/checkmate detection helpers, castling / en passant specialized events, bitboard acceleration, hashing.

## Quick Start

```csharp
var builder = new ChessGameBuilder();
var progress = builder.Compile();

var pawn = progress.Game.GetPiece("white-pawn-2");
var from = progress.Game.GetTile("e2");
var to = progress.Game.GetTile("e4");
var pathVisitor = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
var updated = progress.HandleEvent(new MovePieceGameEvent(pawn, pathVisitor.ResultPath));
```

## Key Concepts (Chess Layer)

- Pattern → Path resolution decouples declarative movement from board topology
- Immutable history allows branching for analysis (future engine feature: timeline hashing)
- Piece identity is stable; only state snapshots change

## Extending / Variants

Add a variant by:

1. Subclassing or composing a new game builder that adjusts piece set or starting tiles
2. Introducing new events (e.g., PromotePieceGameEvent) + rule/mutator
3. Adding conditions for special rights (castling readiness, en passant availability)

Keep changes pure & deterministic.

## Planned Enhancements

- Decision plan executor (faster rule dispatch)
- Compiled movement patterns (DFA) for performance
- Optional bitboards for attack map generation
- Deterministic state hashing for repetition detection

## Versioning

Semantic versioning aligned with repository releases. Breaking movement / rule API changes bump MAJOR.

## Contributing

Open issues & PRs at <https://github.com/veggerby/Veggerby.Boards>. Follow contributor guidelines.

## License

MIT License. See root `LICENSE`.
