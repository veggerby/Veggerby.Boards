# Quickstart

Minimal end‑to‑end example: create a tiny two‑tile “hop” game with one piece per player and a single move rule.

## 1. Builder

```csharp
public sealed class HopGameBuilder : GameBuilder
{
    protected override void Build()
    {
        AddPlayer("p1");
        AddPlayer("p2");

        AddDirection("right");

        var a = AddTile("a");
        var b = AddTile("b");
        a.WithRelationTo("b").InDirection("right");

        AddPiece("p1-piece").WithOwner("p1").HasDirection("right");
        AddPiece("p2-piece").WithOwner("p2").HasDirection("right"); // immobile from b (no reverse relation)

        WithPiece("p1-piece").OnTile("a");
        WithPiece("p2-piece").OnTile("b");

        AddGamePhase("main")
            .If<NullGameStateCondition>()
            .Then()
                .ForEvent<MovePieceGameEvent>()
                .Then()
                    .Do<MovePieceStateMutator>();
    }
}
```

## 2. Compile & Play

```csharp
var progress = new HopGameBuilder().Compile();
var piece = progress.Game.GetPiece("p1-piece");
var from = progress.Game.GetTile("a");
var to = progress.Game.GetTile("b");
var resolver = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to);
piece.Patterns[0].Accept(resolver);
progress = progress.HandleEvent(new MovePieceGameEvent(piece, resolver.ResultPath));
```

## 3. Key Points

* Artifacts (players, tiles, pieces) defined only during build.
* Movement uses pattern(s) declared on pieces; path resolved at event time.
* Rules are explicit: if the phase condition passes, the move rule applies and mutator returns a new GameState.

## 4. Next Steps

* Add dice: `AddDice("d6").HasNoValue();` and roll via `RollDiceGameEvent<int>`.
* Add conditional rule: implement an `IGameEventCondition` rejecting moves onto occupied tiles (example exists in core tests).
* Explore deterministic replay: enable hashing & RNG flags (see Determinism doc).

See also: [Core Concepts](core-concepts.md), [Extensibility](extensibility.md).
