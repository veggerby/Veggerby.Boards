# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Breaking

- Removed legacy rule traversal: the **DecisionPlan** evaluator is now the sole execution path. Feature flags `EnableDecisionPlan` and `EnableDecisionPlanDebugParity` and all dual-run parity scaffolds/tests have been removed. Update any code/tests that referenced these flags.

### Added

- **Turn Sequencing Framework (Workstream 10)**
  Introduced `TurnState`, `TurnArtifact`, and `TurnSegment` for deterministic sequencing:
  - Segment progression (`Start → Main → End`), turn number advancement, and active-player rotation.
  - Events/mutators: `BeginTurnSegmentEvent`, `EndTurnSegmentEvent`, `TurnAdvanceStateMutator`, `TurnPassEvent`, `TurnCommitEvent`, `TurnReplayEvent`.
  - Pass-streak counting and replay turns; metrics (pass/replay/turn length) and benchmark showing <3% overhead.

- **Backgammon**
  - Moved `LastDoubledTurn` to `DoublingDiceState`; blocks same-turn redoubles and supports multi-turn doubling (2→4→8).
  - Gating enforces cube ownership by the active player and alternation across turns.

- **DecisionPlan Modernization**
  - Grouping of identical conditions, event-kind filtering, and exclusivity masking with **static exclusivity inference** (builder/phase/attribute).
  - Observer extensions: batching mode, `OnRuleSkipped` with `RuleSkipReason`, and richer trace capture.
  - Expanded parity and skip-reason tests.

- **Simulation API**
  - `SequentialSimulator` and `ParallelSimulator` with playout policies, stop predicates, cancellation, and detailed metrics (`PlayoutMetrics`, `PlayoutBatchResult`, `PlayoutBatchDetailedResult`).
  - Deterministic, concurrency-safe rollouts. Documentation in `docs/simulation.md`.

- **Movement & Pattern Compilation**
  - Compiled support for `FixedPattern`, `DirectionPattern`, `MultiDirectionPattern`, with adjacency cache, targeted + randomized parity tests, semantics charter, and micro/large-sample benchmarks.
  - Milestone closed; ≥5× aggregate throughput objective deferred pending heuristics.

- **Acceleration & Performance**
  - **Bitboards enabled by default** for boards ≤64 tiles. *Note:* incremental bitboard updates are temporarily disabled; the engine performs a full rebuild per transition until the incremental path is re-enabled.
  - Sliding fast-path (ray attacks + path reconstruction) **enabled by default** after parity/benchmarks (≥4.6× best-case speedup).
  - Per-piece occupancy masks (experimental, flag-gated) and a mobility evaluator prototype (bitboards + rays).
  - New capability seam (`EngineCapabilities` with `Topology`, `PathResolver`, `AccelerationContext`) replaces legacy service-locator wiring.
  - Extensive parity regression and performance benchmarks.

- **Typed Event Results**
  - `EventResult` discriminated type with `EventRejectionReason` for precise outcomes.
  - Promoted `GameProgress.HandleEventResult` to first-class API.

- **Deterministic RNG & State History**
  - Deterministic replay tests, canonical RNG serialization, 64/128-bit state hashing, and finalized timeline zipper.
  - Hashing overhead benchmarks.

- **Developer Experience (DX)**
  - `developer-experience.md` consolidating style charter, benchmark policy, and contribution workflow.
  - Thread-safe `FeatureFlagScope` for deterministic test isolation.
  - Lightweight style-enforcement stub.
  - Expanded property-based invariants for chess/backgammon and a deterministic chess opening helper.

### Changed

- Compiled movement patterns **enabled by default**.
- State hashing uses canonical binary serialization; added 128-bit xxHash128.
- Incremental bitboard updates temporarily disabled (falls back to full rebuild per transition).
- Consolidated package versions via `Directory.Packages.props`.
- README and docs updated across acceleration, sequencing, and DX topics.

### Fixed

- Package restore/version conflicts.
- Completed XML documentation across public APIs; removed prior suppressions.
- Unskipped test scaffolds and addressed parity regressions.

### Maintenance

- Fully removed legacy traversal code.
- Added benchmarks, parity packs, and cleanup checklists for regression safety.
- Reaffirmed repository style charter (file-scoped namespaces, explicit braces, no LINQ in hot paths, immutability, deterministic state).

## [0.1.0] – Initial

- Initial codebase structure, game engine core, Backgammon and Chess modules, and test suite (354 tests passing).

[Unreleased]: https://github.com/veggerby/Veggerby.Boards/compare/v0.1.0...HEAD
