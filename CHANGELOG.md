# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Chess Full Move Legality (Workstream 10) - COMPLETE**: Chess is now fully playable from start to finish with complete legal move generation and endgame detection.
  - `ChessMoveGenerator`: Complete pseudo-legal move generation for all piece types (pawns, knights, bishops, rooks, queens, kings) with proper occupancy handling, castling, en passant, and promotion support.
  - `ChessLegalityFilter`: King safety validation filtering that simulates each move and removes any that would leave the player's king in check. Handles pins, discovered checks, and all special moves.
  - `ChessEndgameDetector`: Detects checkmate (in check + no legal moves), stalemate (not in check + no legal moves), and check conditions. Provides `GetEndgameStatus()` for game status queries.
  - `PseudoMove` record type: Represents candidate moves with metadata (piece, from/to tiles, kind, promotion role, capture flag).
  - `PseudoMoveKind` enum: Classifies move types (Normal, Castle, EnPassant, Promotion).
  - `EndgameStatus` enum: Represents game status (InProgress, Check, Checkmate, Stalemate).
  - Enhanced `ChessNomenclature`: Updated `IsCheckmateAfter()` to use `ChessEndgameDetector` for accurate checkmate notation. Full SAN notation now includes # (checkmate), + (check), =Q (promotion), O-O/O-O-O (castling), x (captures), and e.p. (en passant).
  - Comprehensive test suite: 16+ unit tests covering move generation, legality filtering, endgame detection, and all move variants. 4 integration tests demonstrating full game playability including Scholar's Mate.
  - All capture types validated: pawn, knight, bishop, rook, queen, and en passant captures with explicit verification that captured pieces are marked via `IsCaptured()` and removed from the board.
  - Castling tests: Kingside and queenside castling with validation that both king and rook move correctly.
  - Multiple sequential captures: Tests verify capture state persists across multiple captures in a single game.

- Deck-building: Optional supply depletion alternate end trigger via `WithEndTrigger(new DeckBuildingEndTriggerOptions(...))` enabling threshold and/or key pile emptiness to permit `EndGameEvent`.
- Deck-building: `DeckSupplyStats` extras providing O(1) empty supply pile tracking (cached `TotalPiles` / `EmptyPiles`) maintained incrementally by mutators to avoid repeated dictionary scans during end-trigger evaluation.
- Benchmarks: `DeckBuildingConditionsBenchmark` and `DeckBuildingConditionOnlyBenchmark` added to quantify full event vs gating condition cost (post-optimization capture: GainFromSupply valid path ≈2.94µs / 7.37KB; condition-only ≈97.6ns / 176B).
- Tests: Structural sharing test ensuring only target pile content changes on `GainFromSupplyEvent`; supply stats decrement tests (no increment unless crossing to zero).

> Bitboard128 scaffolding introduced (global + per-player occupancy up to 128 tiles). See Added/Changed below.

### Breaking

- Removed legacy rule traversal: the **DecisionPlan** evaluator is now the sole execution path. Feature flags `EnableDecisionPlan` and `EnableDecisionPlanDebugParity` and all dual-run parity scaffolds/tests have been removed. Update any code/tests that referenced these flags.
- Deck-building: `DeckBuildingEndTriggerOptions` now enforces that at least one termination mechanism is configured (either `emptySupplyPilesThreshold > 0` or a non-empty `keySupplyPileIds`). Constructing with neither now throws `ArgumentException` (previously accepted and ignored). Negative threshold still throws `ArgumentOutOfRangeException`.

### Added (earlier)

- New module: Veggerby.Boards.Cards
- Workstream 17 – Deck-building Core: initial scaffolding
  - Phase & segment orchestration finalized: dedicated always-active `db-turn` phase handling only `EndTurnSegmentEvent` via `DbEndTurnSegmentAlwaysCondition` + `DbTurnAdvanceStateMutator` (wrapper around internal sequencing mutator) to advance `Start → Main → End` deterministically without leaking sequencing concerns into action rules.
  - Segmented deck-building phases: `db-setup` (Start) for `CreateDeckEvent`; `db-action` (Main) for draw/reshuffle + trash; `db-buy` (Main) for supply gains; `db-cleanup` (End) for hand/in-play to discard consolidation.
  - `TurnSegmentStartCondition` semantics refined: when no `TurnState` is present (e.g. sequencing feature flag disabled in focused tests) it now evaluates `Valid` instead of `NotApplicable`, allowing deck initialization to proceed in minimal scenarios while remaining strict once a `TurnState` exists.
  - Expanded test coverage asserting deck state materialization occurs during Start segment prior to advancing to Main / End, preventing silent gating regressions.
  - New project `Veggerby.Boards.DeckBuilding` added to the solution.
  - `DeckBuildingGameBuilder` introduced with minimal topology and players (phases/rules to follow).
  - `CardDefinition` artifact added (metadata only: name, types, cost, victory points) with XML docs.
  - `WithCard(cardId)` helper on `DeckBuildingGameBuilder` to register concrete card artifacts deterministically before compile.
  - Buy phase wiring: `CreateDeckEvent` (init piles + optional supply) and `GainFromSupplyEvent` (decrement supply and append to a target pile) with rules and mutators.
  - Draw with reshuffle: `DrawWithReshuffleEvent` + rules/mutator to shuffle discard deterministically into draw when needed and perform the draw into hand.
  - Trash from hand: `TrashFromHandEvent` + rules/mutator to remove specified cards from Hand.
  - Cleanup: `CleanupToDiscardEvent` + rules/mutator to move all cards from Hand and InPlay to Discard for end-of-turn cleanup.

    - DecisionPlan structural hardening for Deck-building:
      - Locked deterministic DecisionPlan baseline (`DecisionPlanBaseline`) capturing ordered phase:event entries plus SHA-256 signature.
      - Deterministic signature & diff test (`DecisionPlanSignatureTests`) guarding against accidental reordering/insertion/removal (updated after Action/Buy phase split).
      - Baseline capture harness removed (replaced by inline documented regeneration steps in signature test remarks).
      - Structural invariants test (`DecisionPlanInvariants`) asserting presence of critical event rules across phases (CreateDeck, DrawWithReshuffle, GainFromSupply, TrashFromHand, CleanupToDiscard, EndTurnSegment).
      - Diagnostic flattened plan dump test (skipped) for targeted debugging retained until phase split lands.
      - Feature flag guard (`FeatureFlagGuard`) ensuring `EnableTurnSequencing` isolation inside deck-building tests to remove flakiness from shared global flag mutation.
      - TurnState assertion helpers enforcing single turn state materialization pre-main segment advancement.
      - Sequential collection definition for deck-building tests disabling parallel execution to eliminate race conditions on feature flags.
      - Phases split: former combined `db-main` separated into `db-action` (draw, trash) and `db-buy` (gain) with updated baseline and diagnostics.
  - Scoring + Termination: Added `RegisterCardDefinitionEvent` + `CardDefinitionState`, `ComputeScoresEvent` + `ScoreState` for deterministic victory point aggregation, and `EndGameEvent` + `GameEndedState` wired in cleanup phase (idempotent, gated by `EndGameEventCondition`). DecisionPlan baseline updated with new events & signature; ordering invariant test ensures `ComputeScoresEvent` precedes `EndGameEvent`.
  - Removed obsolete diagnostic plan dump test after phase split stabilization (reliance now on invariants + signature test only).
    - Supply configurator scaffold: `DeckBuildingSupplyConfigurator` fluent helper to register card definitions + supply counts and emit deterministic startup events (`RegisterCardDefinitionEvent`s followed by a single `CreateDeckEvent`). Tests cover insertion ordering, duplicate definition rejection, undefined supply safeguard, and `GainFromSupplyEvent` integration.
    - Deck-building module documentation page (`docs/deck-building.md`) providing phases table, zone descriptions, shuffle determinism notes, supply configurator usage, end-to-end example, error modes, and extension points.

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
- Deck-building: `GainFromSupplyStateMutator` optimized (selective cloning) to allocate only a new list for the target pile instead of cloning every pile prior to `DeckState` freezing, reducing intermediate allocations while preserving immutability guarantees.
- Deck-building: `EndGameEventCondition` now fast-paths supply depletion threshold checks via `DeckSupplyStats` (falls back to legacy scan if stats missing for backward compatibility).
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
- Deck-building phase wiring: Action/Buy split completed; baseline updated again after integrating scoring + termination with locked signature and ordering invariant (ComputeScores → EndGame).
- Invalid game event diagnostics: `InvalidGameEventException.Message` now includes the failing condition reason (e.g. `Invalid game event GainFromSupplyEvent: Unknown pile`) improving debuggability and enabling precise guard-path test assertions.

### Fixed

- Package restore/version conflicts.
- Completed XML documentation across public APIs; removed prior suppressions.
- Unskipped test scaffolds and addressed parity regressions.

### Maintenance

- Fully removed legacy traversal code.
- Refactored extras state wrapper from generic `ExtrasState<T>` + reflection construction to non-generic `ExtrasState` eliminating `Activator.CreateInstance` and property reflection during retrieval.
- Eliminated LINQ from builder hot paths (`CreateTileRelation`, `CreatePiece`, `CreatePattern`) reducing allocations and repeated enumerations during game compile.
- Added defensive cycle detection + per-ray caps in sliding attack generation and neutralization guard for large single-direction boards (stability improvements for synthetic parity tests).
- Added benchmarks, parity packs, and cleanup checklists for regression safety.
- Reaffirmed repository style charter (file-scoped namespaces, explicit braces, no LINQ in hot paths, immutability, deterministic state).
- Extended style enforcement narrative to include: centralized `ChessIds` usage, metadata predicates instead of heuristics, and guard-based coverage validation.
- DecisionPlan group gate early pruning implemented (skip entire identical-condition groups when gate fails) reducing redundant condition evaluations and observer noise.
- Movement path performance: `TilePath` refactored to cache relations, tiles, directions, and distance (removed LINQ from hot accessors).
- Benchmarks assertion hygiene: converted non-critical setup artifact presence checks from exceptions to `Debug.Assert` to reduce noise without affecting release semantics.

## [0.1.0] – Initial

- Initial codebase structure, game engine core, Backgammon and Chess modules, and test suite (354 tests passing).

[Unreleased]: https://github.com/veggerby/Veggerby.Boards/compare/v0.1.0...HEAD
