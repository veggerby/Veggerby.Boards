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

- **Chess – Active Player & Turn Alternation**
  - Introduced explicit active-player gating condition (`PieceIsActivePlayerGameEventCondition`) now returning `Ignore` for non-active attempts (replacing exception-style invalidation for friendlier optimistic move submission).
  - Added automatic player alternation mutator (`NextPlayerStateMutator`) after every successful move/capture in chess phases and focused scenario builders.
  - Implemented negative tests for pawn double-step illegality (off start rank after prior move, intermediate blocked, destination occupied) with minimalist scenario builders.
  - Added turn alternation tests (consecutive same-player move ignored; delayed en-passant capture invalidated post-decline).
  - Refactored queen unlocking tests to respect alternation and friendly-occupancy ignore semantics.
  - Simplified initialization test: replaced exhaustive brittle relation enumeration with orientation spot-check helper.
  - En-passant scenario builder refined (optional auxiliary pawn) and tests aligned with canonical orientation (white moves north).
  - Basic castling support (no check/attack validation yet): standard king/queen placement (king on e-file), structural castling condition (rights, clear path, destination empty) with Invalid responses for malformed attempts, castling mutator moving king and rook and revoking rights, automatic rights revocation on king/rook movement. Tests added for kingside success and queenside blocked attempt.

  - **Chess – Castling Safety & API Enhancements**
    - Added `CastlingKingSafetyGameEventCondition` preventing castling while king is in check, or when intermediate/destination squares are attacked (evaluates start, transit, destination squares).
    - Introduced explicit `GameExtensions.Castle(color, kingSide)` helper removing prior synthetic path hack from generic `Move` helper.
    - Deterministic safety denial tests (king-side intermediate square attacked; queen-side destination square attacked) with rights preservation on denial.
    - Optimized attack enumeration (early exit, static direction arrays, no per-move hash set allocations) reducing transient allocations during safety checks.
    - Castling failure messages now include the specific attacked square id.

  - **Chess – Metadata Classification & Identifier Normalization**
    - Introduced explicit piece role & color metadata maps (replacing string heuristic parsing of ids like `white-king`, `-pawn`).
    - Added predicate helpers (`IsKing`, `IsPawn`, `IsWhite`, `IsBlack`, etc.) centralizing all role/color checks with test coverage.
    - Replaced all production/test/benchmark/sample code uses of raw chess piece & tile identifier string literals with `ChessIds` constants (single source of truth) except in intentional custom scenario cases.
    - Implemented `MetadataCoverageGuard` ensuring scenario builders include every declared piece in metadata maps (prevents silent drift).
    - Added classification & parity tests plus en-passant and castling regression tests validating metadata driven logic.
    - Updated documentation snippets to reflect constant-based usage pattern.

  - **Go – Initial Module Scaffolding**
    - Added `GoGameBuilder` (configurable 9/13/19 board), orthogonal liberty topology, stone pools for both players.
    - Added events & mutators: `PlaceStoneGameEvent` (emptiness-only placement) and `PassTurnGameEvent` (increments pass counter), `GoStateExtras` (ko placeholder, pass count, board size).
    - Minimal `GoNomenclature` placeholder and workstream plan (`11-go-game-module`) outlining capture, ko, suicide, scoring, and termination roadmap.

### Changed

- Compiled movement patterns **enabled by default**.
- State hashing uses canonical binary serialization; added 128-bit xxHash128.
- Incremental bitboard updates temporarily disabled (falls back to full rebuild per transition).
- Consolidated package versions via `Directory.Packages.props`.
- README and docs updated across acceleration, sequencing, and DX topics.
- Chess castling implementation evolved from provisional structural-only version to full safety-gated variant with explicit API and performance-tuned attack scanning.
- Chess move, capture, en-passant, and castling mutators & conditions now use metadata predicates (no id substring heuristics remain).
- Centralized chess identifier constants reduced duplication and removed brittle hard-coded literals across codebase & tests.

### Fixed

- Package restore/version conflicts.
- Completed XML documentation across public APIs; removed prior suppressions.
- Unskipped test scaffolds and addressed parity regressions.

### Maintenance

- Fully removed legacy traversal code.
- Added benchmarks, parity packs, and cleanup checklists for regression safety.
- Reaffirmed repository style charter (file-scoped namespaces, explicit braces, no LINQ in hot paths, immutability, deterministic state).
  - Extended style enforcement narrative to include: centralized `ChessIds` usage, metadata predicates instead of heuristics, and guard-based coverage validation.

## [0.1.0] – Initial

- Initial codebase structure, game engine core, Backgammon and Chess modules, and test suite (354 tests passing).

[Unreleased]: https://github.com/veggerby/Veggerby.Boards/compare/v0.1.0...HEAD
