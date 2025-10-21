# Code Quality & Hygiene Opportunities

Date: 2025-10-21
Branch: core/test-assertion-and-structure

## Overview

This document catalogs structural/code smells and improvement opportunities discovered via pattern searches and quick inspection. Focus areas: allocation hygiene, LINQ usage in hot paths, null-forgiving lifecycle markers, API consistency, documentation coverage, and latent TODOs. It prioritizes deterministic performance and clarity per project charter.

## Legend

- Status: Active (needs action), Acceptable (intentionally deferred), Candidate (evaluate impact), Watch (monitor but no immediate change).
- Effort: S (small), M (medium), L (large).

## 1. Lifecycle Null-Forgiving Usage

Found in builder definition classes (`ArtifactDefinition`, `PieceDefinition`, `PlayerDefinition`, `TileDefinition`, `TileRelationDefinition`, `DiceDefinition`, `DirectionDefinition`). All now accompanied by `// LIFECYCLE:` comments and `[MemberNotNull]` attributes.

Opportunity:

- Consider migrating to `required` init properties for immutable specification objects to eliminate `null!` entirely.
Status: Candidate; trade-off with fluent mutation convenience.
Effort: M.

## 2. Single Occurring TODO

`GameBuilder.cs` line ~553: Revisit Extras state/artifact design.
Opportunity:

- Formalize Extras pattern: clarify whether extras are typed states or misc ephemeral artifacts. Possibly introduce `ExtraState<T>` typed container.
Status: Active.
Effort: M.

## 3. LINQ Usage in Potential Hot Paths

Examples:

- `GameBuilder` heavy chaining: `.Select(...).ToArray()`, `.Where(...).ToList()` during build.
- Simulation: `candidates.ToArray()` for shuffling; candidate enumeration repeated.
- Condition response combining reasons: `string.Join(",", failures.Select(x => x.Reason))`.
Analysis:
- Builder paths are one-time per game; acceptable.
- Simulation shuffle materialization is necessary for Fisher–Yates; acceptable but consider length guard (already present).
- `ConditionResponse.New` joining reasons likely low frequency; low risk.
Opportunities:
- Replace repeated `patterns.Select(...).ToArray()` with single pass manual loops for micro-alloc reduction (only if profiling shows build overhead dominates game start).
Status: Watch.
Effort: S-M (depending scope).

## 4. Repeated `.ToList()` Snapshotting

`GameSimulator` batch progress uses `list.ToList()` to create snapshots for metrics display.
Opportunity:

- Replace with defensive copy only when observers attached or count changed threshold; or maintain immutable array segments.
Status: Candidate (only if profiling indicates overhead).
Effort: M.

## 5. Collections Allocation Patterns

Numerous new `List<T>` allocations in DeckBuilding mutators cloning dictionaries of piles.
Opportunities:

- Introduce specialized struct representing deck piles with copy-on-write semantics to avoid whole-dictionary cloning for single pile updates.
- Evaluate frequency of pile mutation; if high, microbench copy cost vs complexity.
Status: Candidate (needs profiling).
Effort: L.


## 7. Path & Movement Resolution Null Handling

Current pattern: nullable path returns + `TryResolvePath` added.
Opportunity:

- Audit call sites still using nullable methods directly; migrate to Try pattern for flow clarity; deprecate nullable returns later.
Status: Active (incremental adoption).
Effort: S.




## 11. Simulation Trace Handling

Now always non-null (empty when disabled).
Opportunity:

- Provide `TraceCount` property to avoid `Trace.Count` (still fine—list or array) but minor.
Status: Acceptable.
Effort: S (optional).

## 12. Normalization Helper Adoption

`Normalize` utilities introduced but not yet used broadly.
Opportunity:

- Replace ad-hoc `?? string.Empty` patterns and enumeration snapshots with `Normalize.Text/List` at ingestion boundaries.
Status: Active.
Effort: S.

## 13. Potential Duplicate Lines in Builder

Repeated identical LINQ lines (e.g., patterns and directions appear twice in search results) may indicate method duplication or copy-paste.
Opportunity:

- Inspect for accidental duplication / refactor shared subsets into local functions.
Status: Active (needs manual review).
Effort: S-M.

## 14. Defensive Returns vs Exceptions

Several methods return null or treat unknown states as terminal silently (e.g., `TurnProfile.Next`, some relation lookups).
Opportunity:

- Standardize policy: resolution APIs return null; state machine invariants throw.
Status: Candidate (define policy in docs).
Effort: S.

## 15. Feature Flags Spread

Feature flag checks sprinkled (e.g., `FeatureFlags.EnableTurnSequencing` inside simulation loop).
Opportunity:

- Inline local bool flags at start to avoid repeated static access and branch prediction costs if hot.
Status: Candidate (micro perf only if flagged as hot path by profiler).
Effort: S.

## 16. Builder Extras & States Collections

Use of `IList<object>` for `_extrasStates` reduces type safety.
Opportunity:

- Introduce generic wrapper `ExtraState<T>` or strongly typed registry keyed by artifact id; document semantics.
Status: Active.
Effort: M.

## 17. Exception Messages Consistency

Various `ArgumentException` messages use generic "Value cannot be null or empty".
Opportunity:

- Centralize message constants or adopt clearer context-specific phrasing for easier diagnostics.
Status: Candidate.
Effort: S.

## 18. Fallback Path Construction in Benchmarks

Manual path reconstruction logic with mid-tile fallback present; ensures non-null but duplicates code.
Opportunity:

- Introduce internal helper `TryConstructTwoStepPath(Game, from, midId, to)` for benchmarks/tests to reduce duplication.
Status: Candidate.
Effort: S.

## 19. Allocation in Composite Policy Enumeration

`CompositePlayoutPolicy.GetCandidateEvents` manually materializes enumerations; has nested enumeration logic.
Opportunity:

- Consider a pooled buffer or early capacity estimation (sum of first elements) if policies scale.
Status: Watch (optimize only with evidence).
Effort: M.

## 20. Missing XML Docs (Spot Sampling)

Not all internal classes have XML docs (acceptable), but any public-facing class lacking docs should be audited.
Opportunity:

- Run public API doc coverage check; enforce via test.
Status: Active.
Effort: M.

## 21. Equality & Hashing Patterns

Artifacts rely on identity; ensure no accidental overriding of `Equals`/`GetHashCode` beyond base contract.
Opportunity:

- Add test scanning exported types for unintended overrides.
Status: Candidate.
Effort: S.



## 24. Domain-Specific Exceptions

Custom exceptions limited; builder error conditions use generic exceptions.
Opportunity:

- Replace `ArgumentException` with domain-specific (e.g., `BuilderException`) for clearer upstream handling.
Status: Candidate.
Effort: M.

## 25. Benchmark Fallback Null Checks

Throwing `InvalidOperationException` inside benchmarks if mid tile missing; might obscure underlying board setup errors.
Opportunity:

- Prefer assertion-based (Debug.Assert) to avoid exception overhead in release microbench runs.
Status: Candidate.
Effort: S.

## Immediate Wins (Active)

1. Adopt `Normalize.Text/List` at ingestion points (12).
2. Public API doc coverage (if test strategy retained) (20).
3. Extras state formalization (2 / 16).
4. Duplicate line inspection & refactor (13).
5. Defensive vs exception policy documentation (14).

## Deferred / Needs Profiling

- DeckBuilding pile cloning refactor (Section 5).
- Composite policy allocation tuning (Section 19).

## Monitoring

Keep an eye on LINQ usage expansion in hot paths (Section 3) and extras registry complexity (Section 2/16).

## Appendix: Search Patterns Used

- TODO/FIXME/HACK
- null! lifecycle occurrences
- LINQ method chains: Select/Where/Any/ToList/OrderBy
- `new List<` allocations (non-test code)

---
One-liner: The codebase is structurally healthy; opportunities center on tightening invariants, reducing builder/alloc micro overhead, and formalizing a few ad-hoc patterns (extras, trace normalization adoption, defensive null returns).
