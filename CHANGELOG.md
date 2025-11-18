# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Fluent API Improvements**:
  - **Lambda-Based Builder API**: New `DefineRules()` method with scoped sub-builders for improved ergonomics
    - `IPhaseRuleBuilder` for phase-level event handler registration
    - `IEventRuleBuilder<T>` for event-level condition configuration
    - `IEventConditionBuilder<T>` for chaining conditions with AND/OR logic
    - `IMutatorBuilder<T>` for scoped mutator application
    - Better visual hierarchy through lambda scoping
    - Easier extraction of rule groups into testable helper methods
    - Smaller IntelliSense context for improved discoverability
  - **ConditionGroup Abstraction**: Reusable condition patterns to reduce duplication
    - `ConditionGroup<TEvent>` with fluent `.Require<T>()` API
    - `.With(ConditionGroup)` method for applying predefined condition sets
    - `ChessConditions` helper class with 6 common chess patterns
  - **Integrated Board Building Methods**: Fluent instance methods in GameBuilder
    - `AddGridTiles(width, height, factory, configure)` - Create 2D grid boards
    - `AddRingTiles(count, factory, configure)` - Create circular track/ring boards
    - `AddMultiplePieces(count, factory, configure)` - Bulk piece creation
    - `NextInRing(position, size)` and `PreviousInRing(position, size)` - Ring navigation helpers
    - Integrated directly into GameBuilder (no separate static helper classes)
    - All methods return GameBuilder for fluent chaining
  - **EndGame Detection Extensions**: Simplified endgame configuration
    - `WithEndGame()` fluent wrapper for endgame detection
    - Cleaner syntax consistent with new API naming conventions
  - **Module Migrations**: Existing game modules refactored to new API
    - **Backgammon**: Migrated to use `AddRingTiles` and `AddMultiplePieces` (44% code reduction)
    - **Chess**: Migrated to use `AddGridTiles` and `AddMultiplePieces` (cleaner, more readable)
    - All module tests passing with zero regressions
  - **Example Game Builder**: Complete working example (`SimpleRaceGameBuilderExample`)
    - Demonstrates AddRingTiles for circular board patterns
    - Shows DefineRules with lambda scoping
    - Includes reusable condition groups
    - Illustrates helper method extraction pattern
  - **Chess Fluent Demo**: `ChessGameBuilderFluentDemo` demonstrating:
    - 7 isolated helper methods for different rule types
    - 40% reduction in repetitive code using condition groups
    - Side-by-side comparison with traditional API
  - **Comprehensive Testing**: 8 new unit tests validating builder mechanics
  - **Backward Compatible**: Existing `Then()` API remains fully functional

- **Documentation Improvements**:
  - **Comprehensive GameBuilder Guide** (`docs/gamebuilder-guide.md`): Step-by-step guide for creating new game modules covering:
    - Complete API surface documentation
    - Tile graph construction patterns (grid, ring, branching, graph)
    - Movement patterns (directional, sliding, fixed)
    - Phases and rules with examples
    - Custom events, mutators, and conditions patterns
    - Testing strategies with AAA pattern examples
    - Common patterns by game type (racing, territory control, card games, abstract strategy)
    - Common pitfalls and anti-patterns
  - **Documentation Navigation** (`docs/README.md`): Central entry point for all documentation
  - **Standardized Module READMEs**: All modules (Chess, Go, Backgammon, DeckBuilding) now follow consistent structure:
    - Overview with feature list
    - Quick start examples
    - Key concepts tables
    - Events & mutators reference
    - Testing guidance
    - Known limitations
    - Extension scenarios
    - Cross-references to core documentation
  - **Archived Completed Workstreams**: Moved completed workstreams (WS-10 Chess, WS-11 Go, WS-17 DeckBuilding) to `docs/plans/archive/`
  - **Samples README** (`samples/README.md`): Overview of all demonstration projects

- **Go Demo** (`samples/GoDemo`): Complete demonstration showing:
  - Stone placement on 9×9 board
  - ASCII board rendering with Unicode stones (● ○) and star points
  - Pass mechanics with two-consecutive-pass termination
  - Area scoring calculation
  - Clear documentation of working features and known limitations
  - Example output with move-by-move progression

### Changed

- **BREAKING: Feature Flags Eliminated**: All 18 feature flags have been removed from production code (#035)
  - **Graduated Features (Always Enabled)**:
    - Compiled patterns (DFA-based movement resolution)
    - Bitboards (for boards ≤128 tiles)
    - State hashing (deterministic fingerprints)
    - Sliding fast path (ray-based sliding resolution)
    - Bitboard incremental updates
    - Turn sequencing system
    - Trace capture diagnostics
    - BoardShape neighbor lookup
  - **Removed Experimental Features**:
    - Decision plan optimizations (grouping, event filtering, exclusivity masks) - deferred to future performance story
    - Compiled pattern adjacency cache - superseded by BoardShape
    - Segmented bitboards - no current use case
    - Per-piece masks - deferred to future optimization
    - Topology pruning - needs benchmarks + parity tests
    - Timeline zipper - no usage
    - Observer batching - deferred to future performance story
  - **Simulation**: Always available via API (using API is explicit opt-in)
  - **Benefits**:
    - Eliminates global mutable state (enables parallel test execution)
    - Reduces codebase by ~500+ lines
    - Single clear code path per feature
    - Removes conditional overhead in hot paths
    - Easier onboarding with obvious production-ready features
  - **Migration**:
    - `FeatureFlags` class retained as no-op compatibility shim for tests
    - Tests can be gradually migrated to remove `FeatureFlagScope` usage
    - All graduated features unconditionally enabled in production code
  - **Documentation Updates**:
    - `docs/feature-flags.md`: Marked deprecated with migration guide
    - `CONTRIBUTING.md`: Feature flag policy deprecated
    - `docs/plans/deferred-features-from-flag-elimination.md`: Comprehensive documentation of deferred features with future work guidance, story templates, and implementation recommendations
    - Test compatibility preserved during migration period

### Added

- **Turn Sequencing Graduation (Workstream 9) - COMPLETE**: Turn sequencing is now fully graduated, default-enabled, and integrated across all modules.
  - **Go Two-Pass Terminal Integration**: Go module now uses `TurnState.PassStreak` for two-pass termination detection instead of module-specific extras state.
    - `PassTurnStateMutator` increments `TurnState.PassStreak` and checks for game end when streak reaches 2.
    - `PlaceStoneStateMutator` resets `TurnState.PassStreak` when a stone is placed, breaking the pass sequence.
    - Removed `ConsecutivePasses` field from `GoStateExtras` (migrated to core `TurnState`).
  - **Active Player Projection Verification**: Comprehensive audit confirms all modules use centralized projection helpers (`GetActivePlayer` / `TryGetActivePlayer`) for consistent turn sequencing behavior.
    - Chess: Uses `NextPlayerStateMutator` (legacy but valid) which relies on centralized projection.
    - Backgammon: Uses `TryGetActivePlayer` in conditions and `GetActivePlayer` in mutators as recommended.
    - Go: Pass handling now integrates with core turn sequencing rotation via `ApplyTurnAndRotatePlayer`.
  - **Comprehensive Documentation**: Enhanced `docs/turn-sequencing.md` with:
    - Complete lifecycle guide (initialization, segment flow, advancement variants)
    - PassStreak management patterns with Go integration example
    - Active player projection best practices (condition vs mutator usage)
    - Module integration examples for Chess, Go, and future Ludo/Monopoly patterns
    - Migration guide from legacy projection and manual rotation patterns
    - Invariants, determinism guarantees, and hash parity notes
  - **Test Coverage**: 796 tests passing including new Go regression tests:
    - `GivenTurnSequencingEnabled_WhenTwoPassesOccur_ThenTurnStatePassStreakDrivesTermination`
    - `GivenPassStreakActive_WhenStonePlaced_ThenPassStreakResetsInTurnState`
  - **All Acceptance Criteria Met**: Go two-pass wiring complete, active player projection verified, documentation comprehensive, all module tests passing with turn sequencing enabled.

- **State Hashing Graduation**: `EnableStateHashing` feature flag graduated from experimental to stable (default: ON).
  - Provides deterministic 64-bit (FNV-1a) and 128-bit (xxHash128) state fingerprints for replay validation and cross-platform determinism enforcement.
  - **Hash Parity Test Infrastructure**: New `HashParityTestFixture` base class with `AssertHashParity` helper methods for comparing state hashes across execution paths.
  - **Cross-Platform Stability Tests**: `CrossPlatformHashStabilityTests` (5 tests) validates hash consistency across platforms and execution environments.
  - **Randomized Replay Tests**: `RandomizedReplayDeterminismTests` (4 tests) validates deterministic replay with RNG seeds and state transitions.
  - **Acceleration Path Parity Tests**: `AccelerationPathHashParityTests` (5 tests) validates hash equality across optimization flags (compiled patterns, bitboards, sliding fast-path, decision plan).
  - **CI Enforcement**: New `determinism-parity` workflow validates hash stability across Linux (x64), Windows (x64), and macOS (ARM). Hash divergence fails the build.
  - **Documentation Updates**: 
    - `feature-flags.md`: Updated to reflect graduated status and cross-platform validation.
    - `determinism-rng-timeline.md`: Enhanced with graduation notes, test coverage details, and usage guidance.
    - `CONTRIBUTING.md`: Added cross-platform determinism policy section with testing requirements and examples.
  - **Test Coverage**: 40 total hash-related tests (27 new + 6 existing StateHashingTests + 7 TurnSequencingHashParityTests).
  - All acceptance criteria met from issue #TBD: hash computation stable, parity infrastructure complete, CI validation active, documentation updated.

- **Cards Module Documentation & Extensions (Workstream 18) - COMPLETE**: Cards module is now fully documented with comprehensive guides, examples, and extended event coverage.
  - **New Events**: 
    - `PeekCardsEvent`: View top N cards without removing them (read-only operation for scrying/preview mechanics).
    - `RevealCardsEvent`: Make specific cards visible to all players (optional visibility tracking).
    - `ReshuffleEvent`: Move cards from source pile (discard) to destination pile (draw) and shuffle for continuous play.
  - **Comprehensive Documentation**:
    - Updated `/docs/cards/index.md`: Core concepts, event catalog with all 8 events, integration guide, best practices.
    - New `/docs/cards/examples.md`: 4 practical examples (War card game, deck-building integration, poker hand management, reshuffle mechanics).
    - New `/docs/cards/api-reference.md`: Complete API reference for all artifacts, states, events, builders, conditions, and extension points.
  - **Sample Integration**: `samples/CardGameDemo` - Blackjack-style demonstration showing shuffle, draw, peek, reveal, discard, and reshuffle operations across multiple rounds.
  - **Test Coverage**: 35 total tests (28 existing + 7 new) covering peek, reveal, reshuffle with determinism validation, edge cases (empty pile peek, reshuffle with no discard), and invalid operations.
  - **All Acceptance Criteria Met**: Peek/Reveal/Reshuffle events implemented, comprehensive documentation delivered, sample integration complete, 100% test pass rate.

### Changed

- **Bitboard Acceleration Graduated**: `EnableBitboards` and `EnableBitboardIncremental` feature flags now default to ON (graduated from experimental status).
  - Multi-module soak tests validate zero desync across 10,000+ random moves per module (Chess, Backgammon, Go).
  - Incremental bitboard update path provides allocation parity with full rebuild while maintaining exact semantic equivalence.
  - Comprehensive test suite: `BitboardIncrementalMultiModuleSoakTests` added with deterministic randomized move sequences.
  - Documentation updated: `feature-flags.md` reflects graduated status, `performance.md` includes new "Acceleration Layer Architecture" section detailing incremental update semantics and Bitboard128 constraints.
  - All 796 tests pass with flags enabled by default, confirming production readiness.

### Completed

- **Performance Data Layout & Hot Paths (Workstream 4) - COMPLETE**: Core acceleration infrastructure delivered with proven 4.66× performance gains and comprehensive validation.
  - **Sliding Fast-Path**: Production-ready acceleration with **4.66× speedup** on empty board scenarios, graduated to stable and enabled by default (`EnableSlidingFastPath = true`).
  - **Core Infrastructure**: BoardShape adjacency layout (O(1) neighbor lookups), PieceMap incremental snapshots, bitboard occupancy system (≤64 tiles), sliding attack generator with ray precomputation.
  - **Bitboard128 Scaffolding**: Support for boards up to 128 tiles (global + per-player occupancy) with internal two-segment structure.
  - **Comprehensive Testing**: Parity V2 test suite covering blockers, captures, multi-block scenarios, immobile pieces, zero-length paths. 27 benchmark classes validating performance across empty/blocked/capture/off-ray scenarios.
  - **Allocation Validation**: Allocation probe benchmarks confirming allocation-free fast-path hits in performance-critical scenarios.
  - **Layered Architecture**: Base layer (compiled patterns fallback), acceleration layer (bitboards + sliding fast-path), optimization layer (experimental per-piece masks), scale layer (Bitboard128 scaffolding).
  - **Feature Flags**: `EnableSlidingFastPath` and `EnableCompiledPatterns` graduated to default ON. Experimental flags (`EnableBitboards`, `EnableBitboardIncremental`, `EnablePerPieceMasks`) remain OFF pending further optimization cycles.
  - **All Acceptance Criteria Met**: Sliding fast-path parity validated, PieceMap & bitboard occupancy integrated, allocation-free fast-path confirmed.
  - Workstream status updated to "done" in documentation. Remaining experimental optimizations identified as future enhancement opportunities.

### Added

- **Go Game Module (Workstream 11) - COMPLETE**: Go is now fully playable with complete capture mechanics, ko rule, game termination, and area scoring.
  - `GroupScanner`: Iterative flood-fill algorithm for finding connected stones and counting liberties. Uses efficient non-recursive implementation suitable for 19x19 boards.
  - `PlaceStoneStateMutator`: Enhanced with complete capture logic. Removes opponent groups with zero liberties, enforces suicide rule (cannot place stone that leaves own group at zero liberties unless capturing), detects and tracks ko situations.
  - `PassTurnStateMutator`: Enhanced with game termination logic. Tracks consecutive passes and marks game as ended after double-pass.
  - `GameEndedState`: Terminal state marker indicating game completion.
  - `GoScoring`: Area scoring algorithm that flood-fills empty regions to assign territory to controlling player. Counts stones on board plus surrounded territory.
  - Ko detection: Simple ko rule implemented - prevents immediate recapture in single-stone capture situations by tracking `KoTileId` in extras. Ko is cleared when playing elsewhere or passing.
  - Comprehensive test suite: **29/29 tests passing (100% success rate)** covering single/multi-stone captures, suicide rule enforcement, ko rule validation (immediate recapture blocking, ko clearing via pass, ko clearing via play elsewhere), snapback distinction (multi-stone captures don't trigger ko), pass counting, game termination, and area scoring.
  - All board sizes functional: 9x9, 13x13, 19x19 with orthogonal liberty topology.
  - Game fully playable: Can play complete games from opening through capture sequences to double-pass termination and scoring.

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
