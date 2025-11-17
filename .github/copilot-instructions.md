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
* Whole-file compliance: when editing any existing file, ensure the entire file (not just touched lines) complies with these standards and formatting/whitespace conventions before finishing the change.

## Formatting Preferences
* Always insert a blank line between methods, properties, and major logical sections.
* Avoid consecutive code lines without separation of concern.
* Maintain at least one blank line before return statements, after variable declarations, and before region or comment blocks.
* Indentation is 4 spaces—never tabs; continuation lines align to semantic block starts (no column art / manual alignment padding).
* Using directives: `using System;` first, then a single blank line, then remaining usings grouped logically (no blank lines inside a logical group).
* Avoid multiple consecutive blank lines; a single blank line is the unit of vertical separation. Remove trailing blank lines at end of files.
* Opening braces for types, methods, properties, and other blocks are placed on a new line (Allman style). Closing braces are followed by a blank line when another member follows. Example:
    ```csharp
    public void Foo()
    {
            // logic
    }

    public int Bar { get; }
    ```
* Inside methods, group: guards, setup, core loop, post-processing, each separated by one blank line. Do not interleave unrelated statements without spacing.
* Local variable declaration block separated from subsequent logic by one blank line (see `PatternCompiler.Compile` for layout of declarations then loops).
* Prefer early-return guard clauses each followed by a blank line to visually isolate invariant enforcement.
* Do not vertically align assignment operators or parameters—keep a simple single space style for clarity and diff minimalism.
* Example (abridged from `PatternCompiler.cs` – showing spacing patterns):
  ```csharp
  public static CompiledPatternTable Compile(Game game)
  {
      var table = new CompiledPatternTable();
      var directionSequenceCache = new Dictionary<string, Direction[]>();

      foreach (var piece in game.Artifacts.OfType<Piece>())
      {
          var patternCount = piece.Patterns is ICollection<IPattern> coll ? coll.Count : piece.Patterns.Count();
          var compiled = new List<CompiledPattern>(patternCount);

          foreach (var pattern in piece.Patterns)
          {
              // switch body separated by blank lines between cases when logically distinct
              switch (pattern)
              {
                  case FixedPattern fixedPattern:
                      // ...
                      break;

                  case DirectionPattern singleDir:
                      // ...
                      break;
              }
          }

          table.Add(new CompiledPiecePatterns(piece.Id, compiled));
      }

      return table;
  }
  ```
* Larger builders (see `GameBuilder.cs`) follow: field declarations → constructor(s) (each separated by one blank line) → public mutator methods (one blank line between each) → compile/build method at end with clearly spaced sections (artifact assembly, validation, return) each separated by single blank lines.
* When adding new code, mirror these vertical whitespace conventions precisely to maintain readability and consistent diff footprint.

## Engine Semantics & Invariants

* Artifacts: equality = type + Id; immutable once built.
* GameState: persistent history chain; never mutated in place.
* Events: never apply themselves; they are pure intentions.
* Rules: couple predicates + mutators; must encode all gating logic explicitly.
* Phases: conditional scopes; only active phases evaluate events.
* Mutators: pure, allocation-conscious; must return original state if no change.
* Determinism: identical prior state + event => identical next state.
* Errors: domain exceptions (`BoardException`, `InvalidGameEventException`) on invariant breach.

## Phase-First Architecture (Authoritative)

Game modules MUST use phase-based lifecycle patterns for:
* **Turn sequencing** - Setup, play, scoring stages as distinct phases
* **Game termination detection** - Use `.WithEndGameDetection()` to integrate endgame conditions
* **Conditional rule scopes** - Model endgame variants, special conditions as phase transitions

See `/docs/phase-based-design.md` for canonical patterns and best practices.

### Termination Tracking (Mandatory for All Game Modules)

Every game module must implement standardized termination and outcome tracking:

1. **Add `GameEndedState` marker** when game concludes (universal termination signal)
2. **Implement `IGameOutcome`** on outcome states for unified API access via `progress.GetOutcome()`
3. **Use `GameNotEndedCondition`** in active phases to prevent moves after termination
4. **Use `.WithEndGameDetection()`** for phase-level automatic endgame detection

See `/docs/game-termination.md` for implementation patterns and module integration guidelines.

### Deprecated Patterns

- **External detector classes** (e.g., `ChessEndgameDetector`) - Use integrated phase detection instead
- **Module-specific termination checks** - Use unified `progress.IsGameOver()` API
- **Ad-hoc state markers** - Use core `GameEndedState` marker

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
* When implementing any change, ensure related documentation artifacts are updated: CHANGELOG.md (for user-visible changes), README.md (if usage or high-level concepts shift), /docs conceptual pages (for new or modified concepts), plans/roadmap files (if scope or sequencing changes), and inline XML docs (for all new public APIs or altered semantics). Remove completed items from transient (e.g. code-quality) review/opportunities docs rather than marking them as done to prevent drift.

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

## Explicitly Forbidden Actions (Agent Conduct)

To protect repository integrity and ensure reproducibility, the assistant must not perform the following actions under any circumstances unless explicitly and narrowly authorized in the prompt:

- Git and Pull Request operations
    - Never run git commands (add, commit, push, pull, merge, rebase, tag, branch, checkout, cherry-pick, etc.).
    - Never create, modify, rebase, comment on, label, or merge Pull Requests or Issues via CLI/API.
    - If users request guidance, provide instructions only; do not execute repository mutations.

- Test/build shortcuts
    - Never run `dotnet test` with `--no-build` (cannot guarantee the code is built). Let `dotnet test` build or run an explicit build first.
    - Never state that a build or tests succeeded unless they were executed in this session and the output was observed.

- Tooling and file edits
    - Do not bypass repository tool guidance: use the provided file-edit tools; do not attempt file edits via shell when a dedicated editor tool exists.
    - Prefer the smallest, targeted edits; avoid broad reformatting or unrelated changes.

- Scope and invariants
    - Do not introduce UI/AI/heuristics or unrelated dependencies (see “Forbidden Patterns”).
    - Do not weaken determinism or core invariants; never add hidden randomness or mutable global state.

These rules complement the “Forbidden Patterns” section and repository coding standards. When in doubt, ask for clarification before proceeding.

