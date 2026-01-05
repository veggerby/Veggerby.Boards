
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

### Code Style: Non-Negotiables (Fast Reference)

1. File-scoped namespaces only (no nested namespace blocks).
2. Explicit braces for every control flow statement (no single-line implicit bodies).
3. Immutability: never mutate prior `GameState` or artifact state objects; always produce a new snapshot (pure mutators).
4. Determinism: identical prior state + identical event sequence + identical feature flag configuration must yield identical resulting state & hashes.
5. No LINQ in hot paths (rule evaluation loops, pattern/sliding resolution, hashing, mutator inner loops). Allowed in tests and setup code.
6. Allocation discipline: zero allocations on the success path for core movement & rule evaluation (exceptions documented inline with `// STYLE-DEVIATION:`).
7. No hidden global state; feature behavior selected via explicit capabilities or builder composition.
8. Public APIs must have XML docs (invariants documented in `<remarks>`).
9. Tests must follow AAA pattern and cover each new rule branch (Valid / Invalid / Ignore / NotApplicable).
10. Format cleanliness: `dotnet format` should produce no changes before PR.

### DecisionPlan Graduation Notice

The legacy rule traversal has been removed. All event handling uses the compiled DecisionPlan path. Do not add new conditional branches for a "legacy" evaluator—any future large-scale rewrite must introduce its own migration harness and temporary tests. Optimization flags (`EnableDecisionPlanGrouping`, `EnableDecisionPlanEventFiltering`, `EnableDecisionPlanMasks`) are optional layers on the single evaluator and may themselves be removed once permanently enabled.

For the complete authoritative charter (including hot path definition, property test acceptance criteria) see `docs/developer-experience.md`. Any intentional deviation MUST include `// STYLE-DEVIATION:` plus a CHANGELOG entry under Temporary Exceptions.

### Feature Flag Policy (DEPRECATED)

**⚠️ Feature flags have been eliminated from production code.** This section is retained for historical context only.

All graduated features are now unconditionally enabled. Experimental features have been removed or deferred to future work. The `FeatureFlags` class exists only as a test compatibility shim during migration and will be removed in a future release.

**For new contributions:**
- Do NOT add new feature flags
- Express optional behavior via explicit capabilities / dependency injection
- Use interfaces and strategy objects for pluggable implementations
- Tests using `FeatureFlagScope` should be gradually migrated to remove flag dependencies

### Property / Invariant Tests

All property-style or invariant tests must:

1. Be deterministic (fixed seed / deterministic sequence)
2. Use AAA structuring with clear arrange/act/assert separation
3. ~~Scope feature flags via `FeatureFlagScope` only~~ (DEPRECATED - flags eliminated)
4. Avoid LINQ in per-iteration hot loops (allowed in aggregation)
5. Assert both absence of unintended mutation and presence of intended effect
6. Reuse canonical helpers instead of duplicating engine logic
7. Document any tolerated randomness attempts (loop count + rationale)

See `developer-experience.md` Section 12 for the authoritative, versioned list.

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
- Build the solution (`Veggerby.Boards.sln`) using .NET 10 (primary target) — older TFMs may exist for compatibility.
- Run the tests (`Veggerby.Boards.Tests`) before pushing.
- Keep test count and coverage stable or increased.

```bash
dotnet restore
dotnet build --configuration Release
dotnet test test/Veggerby.Boards.Tests --configuration Release --settings .runsettings
```

The `.runsettings` file provides test timeout protection (5-minute session timeout) and enforces serial execution (`MaxCpuCount=1`) to prevent FeatureFlagScope deadlocks. Always use it when running tests locally or in CI.

**Note**: Tests must run serially due to a static semaphore in FeatureFlagScope. Parallel test assembly execution causes deadlocks. This is enforced by the .runsettings configuration.

Optional with coverage (locally):

```bash
dotnet test test/Veggerby.Boards.Tests \
   --settings .runsettings \
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

## Determinism Checklist (Authoritative Quick Win)

Before submitting changes that touch engine state transitions, confirm:

1. No banned nondeterministic APIs introduced (Random, Guid.NewGuid, DateTime.Now/UtcNow, Task.Run, Parallel.For*, Thread.Sleep, Task.Delay in tests).
2. Feature flags used only within explicit capability boundaries or temporarily wrapped in disposable scopes within tests.
3. Reapplying identical event sequences on identical prior state yields identical resulting state (existing tests cover common paths; add one for new mutators).
4. No reflection-based dynamic invocation added in hot paths (state mutation, pattern resolution, rule evaluation).
5. No hidden global mutable state added (static caches must be immutable after construction and justified in docs if introduced).
6. All new tests deterministic (fixed seeds use TestDeterministicRng not System.Random).
7. Performance-sensitive loops avoid LINQ / allocations beyond documented exceptions.

### Cross-Platform Determinism Policy

State hashing (`EnableStateHashing`, graduated to ON by default) provides deterministic 64/128-bit fingerprints for:

- **Replay validation**: Identical initial state + event sequence → identical hashes across platforms
- **Parity testing**: Verify optimization paths (compiled patterns, bitboards, etc.) produce identical states
- **Bug reproduction**: Capture and replay deterministic state sequences

**CI Enforcement**: The `determinism-parity` workflow validates hash stability across Linux (x64), Windows (x64), and macOS (ARM). Hash divergence fails the build.

**Testing Requirements**:

- Use `HashParityTestFixture` base class for hash comparison tests
- Call `AssertHashParity(reference, candidate, context)` to verify hash equality
- Add parity tests for any new acceleration paths or optimization flags
- Ensure all serialization uses platform-stable types (no `nint`/`nuint`, explicit endianness)
- Document hash algorithm changes in `determinism-rng-timeline.md` with version notes

**Examples**: See `CrossPlatformHashStabilityTests`, `RandomizedReplayDeterminismTests`, and `AccelerationPathHashParityTests` for test patterns.

Thanks for helping make Veggerby.Boards better.
