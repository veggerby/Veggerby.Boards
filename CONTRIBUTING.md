
# Contributing to Veggerby.Boards

Thank you for your interest in contributing to **Veggerby.Boards**!

This project is a composable .NET board game engine: immutable artifacts (board, tiles, pieces, dice, players), deterministic state transitions, and rule/phase driven flow. Please read these guidelines before submitting code, issues, or documentation.

## How to Contribute

1. **Fork** the repository and create a feature branch from `main`.
2. **Write clear, focused commits**. Each commit should address a single concern.
3. **Follow the style guide** and `.editorconfig` (spaces, braces, naming, etc.).
4. **Add or update tests** (xUnit + AwesomeAssertions) for all new code and branches (happy, edge, exception).
5. **Document public APIs** with XML docs.
6. **Run** `dotnet build` and `dotnet test` to ensure all checks pass.
7. **Submit a pull request** with a clear description and reference any related issues.

## Project Rules

- **Immutability first**: Artifacts and prior GameStates must never be mutated in place.
- **Deterministic transitions**: Mutators are pure functions from (engine,state,event) → new state.
- **Explicit phases**: Game flow changes only through `GamePhase` conditions; no hidden global flags.
- **Small mutators**: One responsibility each; compose rather than branch heavily.
- **No hidden randomness**: Randomness must be injected (e.g., via dice value generators) and testable.
- **Minimal dependencies**: Only xUnit + AwesomeAssertions for tests; avoid runtime deps.
- **Clear failure semantics**: Throw specific exceptions (`BoardException`, `InvalidGameEventException`) when invariants break.

See `docs/` (architecture & core concepts) for authoritative model.

## Code Style

- File-scoped namespaces only.
- Braces required for all control flow.
- Spaces only (4 per indent), no tabs.
- Remove unused usings.
- Public: PascalCase, Private: _camelCase.

## Pull Requests

Include in description:
1. Problem / motivation
2. Approach (new types? new phase? new mutator?)
3. Tests added (list scenarios)
4. Potential regressions / risk
5. Docs updated (yes/no + which files)

## Issues

If you find a bug or have a feature request, please open an issue with as much detail as possible.

## Code of Conduct

Be respectful and considerate in all interactions.

---

## 🚀 Ways to Contribute

- **Report bugs**: Include reproduction (builder + event sequence).
- **Enhance engine**: New conditions, mutators, event pre-processors.
- **Add a sample game module**: Demonstrate new patterns (hex grid, stacked pieces, etc.).
- **Improve docs**: Clarify phase flow, movement patterns, extension steps.
- **Increase test coverage**: Edge cases (blocked paths, composite phases, ignored events).

---

## 📋 Guidelines

1. **Discuss Larger Changes First** – Open an issue for architectural shifts (new phase model, new artifact category).
2. **Keep PRs Focused** – One feature or fix; avoid large refactors + new features together.
3. **Tests Required** – Each new branch (Valid / Ignore / Invalid) and mutator path.
4. **Style Consistency** – File-scoped namespaces, braces always, 4-space indents, no trailing whitespace.
5. **Explain the Why** – Problem, approach, test coverage, risks.
6. **No Silent Behavior Changes** – Document any altered public semantics (phase resolution, event ordering, etc.).
7. **Performance Mindful** – Avoid unnecessary allocations inside hot paths (event handling, phase resolution).

---

## 🛠 Local Setup

- Clone the repository.
- Build the solution (`Veggerby.Boards.sln`) using .NET 9 (primary target) — older TFMs may exist for compatibility.
- Run the tests (`Veggerby.Boards.Tests`) before pushing.
- Keep test count and coverage stable or increased.

```bash
dotnet restore
dotnet build --configuration Release
dotnet test test/Veggerby.Boards.Tests --configuration Release
```

Optional with coverage (locally):

```bash
dotnet test test/Veggerby.Boards.Tests \
   --collect:"XPlat Code Coverage" \
   -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
```

---

## 🧩 Project Structure (Overview)

| Folder | Purpose |
|:-------|:--------|
| `src/Veggerby.Boards` | Core engine (artifacts, state, phases, rules, builder) |
| `src/Veggerby.Boards.Backgammon` | Backgammon sample module |
| `src/Veggerby.Boards.Chess` | Chess sample module |
| `src/Veggerby.Boards.Api` | ASP.NET Core demo API |
| `test/Veggerby.Boards.Tests` | Test suite (xUnit + AwesomeAssertions) |
| `docs/` | Architecture, concepts, extensibility, API docs |
| `.github/` | CI workflows |

---

## 🛡️ Code of Conduct

Be constructive and respectful. Assume good intent. Technical critique is welcome; personal criticism is not.

---

## ✅ Quick PR Checklist

- [ ] Builds clean (`dotnet build`)
- [ ] Tests added/updated & passing (`dotnet test`)
- [ ] No raw `Assert.` (use AwesomeAssertions)
- [ ] Public APIs documented where exposed
- [ ] New events/mutators covered (valid + invalid paths)
- [ ] Phase conditions remain deterministic
- [ ] Docs updated (README / docs/* if applicable)

Thanks for helping make Veggerby.Boards better.
