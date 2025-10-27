# Code Quality & Hygiene Opportunities

Date: 2025-10-27
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

## (Removed) Extras TODO

Extras pattern formalized: non-generic `ExtrasState` wrapper replaces prior generic reflection approach; lifecycle documented in CHANGELOG. No active TODO here.

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

## (Removed) Path & Movement Resolution Null Handling

Migration to `TryResolvePath` completed; remaining direct visitor usage eliminated or documented for legacy reference samples.

## 11. Simulation Trace Handling

Now always non-null (empty when disabled).
Opportunity:

- Provide `TraceCount` property to avoid `Trace.Count` (still fine—list or array) but minor.
Status: Acceptable.
Effort: S (optional).

## (Removed) Normalization Helper Adoption

Guard modernization superseded silent normalization; ingestion points now enforce explicit `ThrowIfNullOrWhiteSpace` guards instead of Normalize utilities.

## (Removed) Potential Duplicate Lines in Builder

Inspection completed; no accidental duplication beyond intentional artifact assembly loops. Item closed.

## (Removed) Defensive Returns vs Exceptions

Policy documented in `core-concepts.md`; resolution APIs use `Try*`/null for optional queries, invariants throw domain exceptions. Item closed.

## 15. Feature Flags Spread

Feature flag checks sprinkled (e.g., `FeatureFlags.EnableTurnSequencing` inside simulation loop).
Opportunity:

- Inline local bool flags at start to avoid repeated static access and branch prediction costs if hot.
Status: Candidate (micro perf only if flagged as hot path by profiler).
Effort: S.

## (Removed) Builder Extras & States Collections

Implemented typed registry (`Dictionary<Type, object>`) + non-generic `ExtrasState`; item closed.

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

## (Removed) Benchmark Fallback Null Checks

Converted to Debug.Assert; closed.

## Immediate Wins (Active)

1. Duplicate line inspection & refactor (13) – verify after LINQ removal no accidental duplication remains.
2. Defensive vs exception policy documentation (14).
3. Path resolver Try* adoption (7) – finalize migration.

## Deferred / Needs Profiling

- DeckBuilding pile cloning refactor (Section 5).
- Composite policy allocation tuning (Section 19).

## Monitoring

Watch for reintroduction of LINQ in hot paths and potential complexity creep in deck pile cloning logic.

## Appendix: Search Patterns Used

- TODO/FIXME/HACK
- null! lifecycle occurrences
- LINQ method chains: Select/Where/Any/ToList/OrderBy
- `new List<` allocations (non-test code)

---
One-liner: Structural hotspots addressed; remaining opportunities now focus on API clarity, selective profiling-driven optimizations, and maintaining deterministic performance (doc coverage harness tracked separately as a future enforcement test rather than an opportunity item).
