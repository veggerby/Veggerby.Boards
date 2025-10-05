## Project Overview

Veggerby.Boards is a composable, deterministic .NET board game engine. It models boards as graphs, immutable artifact identities (Pieces, Tiles, Players, Dice, Board), and an immutable `GameState` history advanced only by validated game events processed through rules and phases. The design optimizes for clarity, testability, and deterministic replay.

### What It Provides
* Immutable artifact identities created via `GameBuilder`.
* Declarative events (`MovePiece`, `RollDice`, etc.) that express intent only.
* Pure mutators producing new `GameState` snapshots (persistent chain).
* Rule + phase gating to ensure only valid events transition state.
* Inline efficient bitboard / layout primitives (performance-minded, allocation aware).
* Reusable game modules (Chess, Backgammon; Go planned) demonstrating composition.

### What It Explicitly Does Not Provide
* UI / rendering / animation.
* Generic rule-engine DSL or physics simulation.
* AI players or heuristic search.
* Hidden side‑effects or mutable global state.

### Mental Model (Authoritative)
Artifacts are immutable identities; GameState is immutable history; Events are declarative intentions; Rules couple conditions and mutators; Phases are conditional scopes. Determinism is sacrosanct. Same input → same output always (no ambient randomness—dice encapsulate randomness explicitly in `DiceState`).

## Folder / Layer Structure

Core (`Veggerby.Boards`): foundational domain primitives (Artifact, Game, GameState, GameBuilder, Rules, Phases, Layout, Bitboards).

Modules (shipped: `Veggerby.Boards.Chess`, `Veggerby.Boards.Backgammon`; planned: `Veggerby.Boards.Go`): declarative builders and rule sets composed from core primitives; no cross‑contamination of module specifics into core.

API / Samples / Benchmarks: façade + demonstration + performance validation; never introduce side‑effects into core logic.

Docs (`/docs`): conceptual explanations and extension guidance. Update when introducing new public concepts or extension seams.

## Libraries & Frameworks

* .NET (C#) for engine implementation.
* xUnit + AwesomeAssertions + NSubstitute for testing.
* (Optional) AutoMapper only in API layer (not core) for DTO mapping.

## Coding Standards

* Obey `.editorconfig` strictly.
* File‑scoped namespaces (`namespace Veggerby.Boards;`).
* 4 spaces indentation, no tabs, no trailing whitespace.
* `using System;` first, blank line, then other usings alphabetical/logical.
* Naming: private fields `_camelCase`; public members PascalCase; constants PascalCase.
* Always use braces, even for single statements.
* Expression-bodied members only when strictly clearer.
* Favor explicit loops in perf‑sensitive paths over LINQ.
* Tests always segmented with `// arrange`, `// act`, `// assert` comments.

## Engine Semantics & Invariants

* Artifacts: equality = type + Id; immutable once built.
* GameState: persistent history chain; never mutated in place.
* Events: never apply themselves; they are pure intentions.
* Rules: couple predicates + mutators; must encode all gating logic explicitly.
* Phases: conditional scopes; only active phases evaluate events.
* Mutators: pure, allocation-conscious; must return original state if no change.
* Determinism: identical prior state + event => identical next state.
* Errors: domain exceptions (`BoardException`, `InvalidGameEventException`) on invariant breach.

## Testing Requirements

* Frameworks: xUnit, AwesomeAssertions, NSubstitute (only these—no extra deps).
* Deterministic: randomness encapsulated solely via `DiceState<T>`.
* Branch Coverage: every rule path (Valid / Invalid / Ignore / NotApplicable) has tests.
* Structure: `// arrange`, `// act`, `// assert` required for clarity.
* No reliance on previous test ordering (tests independent & reproducible).
* Avoid hidden global toggles except explicit feature flags (restore original value in `IDisposable` scope helpers).

### Example Test Pattern
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

## Performance Guidelines

* Keep hot paths allocation-free where practical (e.g., bitboards, movement resolution, rule dispatch).
* Avoid LINQ and unnecessary boxing in inner loops.
* Immutable value types (e.g., compact structs) favored where semantically stable.
* No speculative caching unless data is immutable and microbenchmarks justify it.

## Documentation Expectations

* Public APIs require XML docs (summary + key invariants in `<remarks>` if non-trivial).
* Update `/docs` when introducing: new event kinds, rule composition helpers, movement pattern visitors, or extension seams.
* Keep examples minimal and deterministic.

## Dependency Policy

* Core: zero external dependencies beyond BCL.
* Tests: xUnit, AwesomeAssertions, NSubstitute only.
* API layer may use AutoMapper—never leak into core.

## Forbidden Patterns

* Mutating `GameState` or any artifact state directly.
* Hidden global state / implicit singletons.
* Skipping rule/phase gating (no backdoors).
* Random behavior outside explicit dice abstraction.
* Mixing game-specific module logic into core primitives.
* Obscure implicit conversions or magic reflection hacks.
* LINQ inside mutators (explicit loops required for determinism & perf clarity).

## Suitable / Unsuitable Tasks

Suitable:
* New `IGameEvent` + accompanying mutator + tests.
* New condition / composite condition helper.
* Module builder extension (Chess, Backgammon; Go planned) introducing rule branch.
* New movement pattern visitor with tests + microbench (must include perf validation rationale).
* API layer extension (DTO / façade) that remains a thin mapping layer with zero new core coupling.

Unsuitable:
* Introducing AI heuristics.
* UI or rendering responsibilities.
* Adding convenience external dependencies for non-core concerns.

## Definition of Done (Authoritative)

A change is DONE when ALL of the following hold:
1. Build passes (`dotnet build`) with no warnings introduced.
2. All existing and new tests pass (`dotnet test`) without `--no-build`.
3. New logic covered by tests including edge, invalid, and no-op paths.
4. Public APIs added or modified have XML documentation (and docs updated if conceptually new).
5. No mutation of existing state objects; immutability preserved (verified via code review / tests).
6. Determinism preserved (no hidden randomness; dice changes visible in state snapshots).
7. Performance-sensitive paths not regressed (microbench or reasoning if changed significantly).
8. No forbidden patterns introduced (see Forbidden Patterns section).
9. Feature flags or temporary toggles restored to prior state after tests.
10. Naming, formatting, and structure conform to Coding Standards.

## Safety & Invariant Enforcement

* Validate inputs early; throw domain exceptions with clear intent.
* Favor explicit guard branches over silent failure.
* Tests must assert failure modes for invalid operations (not just happy paths).

## Final Guidance

Favor small, verifiable steps. If an abstraction does not clearly reduce duplication, enable a proven reuse scenario, or sharpen semantics—do not add it. All new abstractions must be justified by at least one concrete in-repo use case (present or imminent). Determinism, clarity, and immutability outweigh cleverness.

