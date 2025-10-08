# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

> Bitboard128 scaffolding introduced (global + per-player occupancy up to 128 tiles). See Added/Changed below.

### Breaking

- Removed legacy rule traversal: the **DecisionPlan** evaluator is now the sole execution path. Feature flags `EnableDecisionPlan` and `EnableDecisionPlanDebugParity` and all dual-run parity scaffolds/tests have been removed. Update any code/tests that referenced these flags.

### Added

- New module: Veggerby.Boards.Cards
- Workstream 17 – Deck-building Core: initial scaffolding
  - New project `Veggerby.Boards.DeckBuilding` added to the solution.
  - `DeckBuildingGameBuilder` introduced with minimal topology and players (phases/rules to follow).
  - `CardDefinition` artifact added (metadata only: name, types, cost, victory points) with XML docs.
  - `WithCard(cardId)` helper on `DeckBuildingGameBuilder` to register concrete card artifacts deterministically before compile.
  - Buy phase wiring: `CreateDeckEvent` (init piles + optional supply) and `GainFromSupplyEvent` (decrement supply and append to a target pile) with rules and mutators.
  - Draw with reshuffle: `DrawWithReshuffleEvent` + rules/mutator to shuffle discard deterministically into draw when needed and perform the draw into hand.
  - Trash from hand: `TrashFromHandEvent` + rules/mutator to remove specified cards from Hand.
  - Cleanup: `CleanupToDiscardEvent` + rules/mutator to move all cards from Hand and InPlay to Discard for end-of-turn cleanup.

  - Deterministic cards & decks capability: artifacts (`Card`, `Deck`), immutable `DeckState` with named ordered piles, and events for create/shuffle/draw/move/discard.
  - Deterministic shuffle powered by `GameState.Random` (seed via `GameBuilder.WithSeed`) for full replay reproducibility.
  - `CardsGameBuilder` with minimal topology and two players to satisfy core invariants, plus helper `CreateInitialDeckEvent()`.
  - Tests covering create+draw flow, deterministic shuffle parity, and invalid draw rejection.

- Bitboard128 scaffolding: `BitboardSnapshot` now supports boards up to 128 tiles using internal two-segment `Bitboard128` structure (global + per-player masks).
- Experimental segmented bitboards (`EnableSegmentedBitboards` flag): unified scalable representation supporting 64 and 128 tile boards with parity tests (global + per-player). Currently feature-gated; default off until extended stress + performance benchmarks complete.
- Synthetic large board test (`Bitboard128SnapshotTests`) validating 128-bit snapshot popcount parity with piece state count.
- Randomized and extended parity stress tests for incremental bitboard updates.

- Unit tests increasing coverage:
  - `BitboardSnapshotTests` for 64-bit and 128-bit build/update paths, including optimistic concurrency no-op and segment-aware assertions (AAA + AwesomeAssertions).
  - `BackgammonBoardRendererTests` verifying ASCII rendering (top/bottom rows, bar/home summary) and exact formatting.

- Documentation guidance:
  - Turn sequencing docs updated with `TryGetActivePlayer(out Player)` vs `GetActivePlayer()` usage guidance (conditions/gates vs strict flows).
  - Core concepts now include an “Active player projection” section discouraging direct `ActivePlayerState` scans.

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

- **Bitboard Incremental Path (Soak Flag)**
  - Added feature flag `EnableBitboardIncremental` (default off) reintroducing incremental bitboard & piece map snapshot updates for move events.
  - Parity test (`BitboardIncrementalParityTests`) validates identical occupancy vs full rebuild for scripted opening sequence.
  - Full rebuild remains default until large randomized suites confirm no desync.

- **Turn Sequencing Graduation (Core Enabled)**
  - `EnableTurnSequencing` now defaults ON; initial `TurnState` emitted when building games.
  - Consolidated active player rotation into `TurnSequencingHelpers.ApplyTurnAndRotate` used by advance & pass mutators.
  - Added deterministic sequencing tests (`TurnSequencingDeterminismTests`) covering scripted advancement, pass streak increment & replay reset.
  - Refactored mutators (`TurnAdvanceStateMutator`, `TurnPassStateMutator`, `TurnReplayStateMutator`, `TurnCommitStateMutator`) to streamlined remarks and helper usage.
  - Remaining (tracked in status/workstreams): Go two-pass termination wiring, legacy active player projection replacement, hash parity test once hashing feature graduates.

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

- Acceleration context selection now enables bitboards for boards up to 128 tiles (previously ≤64). Fast path for ≤64 unchanged.
- Sliding attack generator now defensively skips precomputation on degenerate single-direction boards >64 tiles to avoid pathological allocation growth (no functional regression – rays offer no additional branching on such topologies).
- `BitboardSnapshot` incremental update path extended to handle 128-bit occupancy when active.

- Compiled movement patterns **enabled by default**.
- State hashing uses canonical binary serialization; added 128-bit xxHash128.
- Incremental bitboard updates temporarily disabled (falls back to full rebuild per transition).
- Incremental bitboard updates reintroduced behind soak flag; default behavior still full rebuild until graduation.
- Consolidated package versions via `Directory.Packages.props`.
- README and docs updated across acceleration, sequencing, and DX topics.
- Chess castling implementation evolved from provisional structural-only version to full safety-gated variant with explicit API and performance-tuned attack scanning.
- Chess move, capture, en-passant, and castling mutators & conditions now use metadata predicates (no id substring heuristics remain).
- Centralized chess identifier constants reduced duplication and removed brittle hard-coded literals across codebase & tests.
- Turn sequencing implementation elevated from experimental shadow mode to default-on core; duplicate rotation logic removed.
- Refactor sweep to prefer non-throwing `TryGetActivePlayer(out Player)` in safe contexts (conditions/gates) across core and modules (Backgammon, Chess); strict `GetActivePlayer()` retained in invariant-critical paths.

### Fixed

- Package restore/version conflicts.
- Completed XML documentation across public APIs; removed prior suppressions.
- Unskipped test scaffolds and addressed parity regressions.

### Maintenance

- Fully removed legacy traversal code.
- Added defensive cycle detection + per-ray caps in sliding attack generation and neutralization guard for large single-direction boards (stability improvements for synthetic parity tests).
- Added benchmarks, parity packs, and cleanup checklists for regression safety.
- Reaffirmed repository style charter (file-scoped namespaces, explicit braces, no LINQ in hot paths, immutability, deterministic state).
  - Extended style enforcement narrative to include: centralized `ChessIds` usage, metadata predicates instead of heuristics, and guard-based coverage validation.

## [0.1.0] – Initial

- Initial codebase structure, game engine core, Backgammon and Chess modules, and test suite (354 tests passing).

[Unreleased]: https://github.com/veggerby/Veggerby.Boards/compare/v0.1.0...HEAD
