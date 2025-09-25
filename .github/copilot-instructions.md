# Copilot Instructions – Veggerby.Boards

These rules are binding for any AI-generated code in this repo.
**Follow exactly.**

---

## 1. Purpose & Framing

You are contributing to **Veggerby.Boards**, a composable .NET board game engine.
It models **boards (graphs), artifacts (pieces, dice, players), immutable state, and rule-driven phases** for deterministic turn progression.

### What this library is

* A **structural engine** for board games:

  * **Artifacts** (Piece, Tile, Dice, Player, Board) are immutable identities.
  * **GameState** snapshots current positions, dice values, and history links.
  * **Events** express intent (`MovePiece`, `RollDice`).
  * **Mutators** apply deterministic transitions.
  * **Rules & Phases** gate event applicability and structure turn flow.
* Game modules (Backgammon, Chess) demonstrate reuse of the core engine.
* API layer is a façade for demo exposure (DTOs + ASP.NET).

### What this library is not

* Not a UI toolkit, rendering library, or AI opponent.
* Not a physics simulator or generic rule engine.
* Not for hidden side effects: all state transitions must be explicit and immutable.

### Mental model (authoritative)

* **Artifacts are immutable identities.** They exist only via `GameBuilder`.
* **GameState is immutable history.** Each transition yields a new snapshot linked to the prior.
* **Events are declarative intentions.** They never apply themselves.
* **Rules couple conditions and mutators.** Valid events → mutators → new state.
* **Phases are conditional scopes.** Only active phases handle events.
* **Determinism is sacrosanct.** Same event + same state → same result, always.

---

## 2. Core Principles

1. Determinism > cleverness (no randomness outside explicit `DiceState`).
2. Clarity > abstraction bloat (favor explicit builders and mutators).
3. Immutability: never mutate state; always produce new `GameState`.
4. Explicit gating: conditions control flow, no hidden shortcuts.
5. Testability: every rule branch (Valid, Invalid, Ignore, NotApplicable) must be covered.

---

## 3. Style & Formatting

* Follow `.editorconfig` **exactly**.
* File-scoped namespaces only (`namespace Veggerby.Boards;`).
* Braces required for all control flow.
* Spaces (4 per indent). No tabs, no trailing whitespace.
* `using System;` first, blank line, then others.
* Private fields: `_camelCase`. Public: PascalCase. Constants: PascalCase.
* Expression-bodied members only if trivially clearer.
* Tests: use `// arrange`, `// act`, `// assert`.

---

## 4. Architecture Boundaries

* **Core (`Veggerby.Boards`)**

  * Definitions: `Artifact`, `Game`, `GameState`, `IGameEvent`, `IStateMutator<T>`, `IGameEventRule`, `GamePhase`, `GameBuilder`.
  * No direct coupling to game-specific logic.
* **Modules (Backgammon, Chess)**
  Declarative builders only. Reuse core primitives.

  * Thin façade: build games, handle events, return DTOs.
  * No business logic beyond mapping.

---

## 5. Semantics & Invariants

* **Artifacts:** equality by type + Id.
* **GameState:** persistent chain (`CompareTo` available).
* **Events:** never mutate state directly.
* **Mutators:** must be pure; always return new state or original if no change.
* **Conditions:** must be explicit; compose via provided composite helpers.
* **Phases:** resolve via first valid leaf condition (deterministic).
* **Errors:** throw domain exceptions (`BoardException`, `InvalidGameEventException`) when invariants break.

---

## 6. Testing

* Framework: xUnit + AwesomeAssertions.
* Running: when running unit tests via `dotnet test` **never** use `--no-build` parameter.
* Each rule branch covered: happy, edge, exception.
* Deterministic: no hidden randomness (dice must use explicit `DiceState<T>`).
* Example template:

```csharp
[Fact]
public void GivenValidMove_WhenHandled_ThenPieceMoves()
{
    // arrange
    var builder = new ChessGameBuilder();
    var progress = builder.Compile();
    var pawn = progress.Game.GetPiece("white-pawn-2");
    var from = progress.Game.GetTile("e2");
    var to = progress.Game.GetTile("e4");
    var path = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to).ResultPath;

    // act
    var updated = progress.HandleEvent(new MovePieceGameEvent(pawn, path));

    // assert
    updated.State.GetPieceState(pawn).Tile.Should().Be(to);
}
```

---

## 7. Performance

* Immutable by default; avoid unnecessary allocations in event handling.
* No LINQ in hot mutators (prefer loops for clarity + performance).
* No caching unless immutable and safe.

---

## 8. Documentation

* XML docs for all public APIs.
* Document invariants in `<remarks>`.
* Update `docs/` if adding new concepts or extension points.

---

## 9. Dependency Policy

* Keep dependencies minimal.
* Tests: xUnit + NSubstitute + AwesomeAssertions only.
* API layer may use AutoMapper but not core.

---

## 10. Forbidden Patterns

🚫 Mutating `GameState` or `ArtifactState` directly.
🚫 Hidden global state.
🚫 Skipping condition checks in rules.
🚫 Random behavior outside dice.
🚫 Mixing module-specific logic into Core.

---

## 11. PR Checklist

Before merging:

* [ ] `dotnet build` passes
* [ ] All tests green
* [ ] New logic covered by tests
* [ ] No analyzer/style warnings
* [ ] XML docs added
* [ ] Invariants preserved
* [ ] Docs updated (if public behavior changed)

---

## 12. Suitable Tasks

✅ Add new `IGameEvent` + mutator with tests
✅ Add new condition type
✅ Extend Backgammon/Chess builder with new rule branch
✅ Add visitor for new movement pattern

❌ Add AI logic into core
❌ Add UI rendering into engine
❌ Add external deps for convenience

---

## 13. Safety

* Validate invariants early; throw clear exceptions.
* No user input parsing in core (API must validate externally).
* Keep engine pure: no logging, no side effects.

---

**Final rule:**
Favor *small, verifiable changes*.
If a new abstraction doesn’t clarify semantics or reduce duplication, **don’t add it**.
