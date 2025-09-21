# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Package metadata (Description, PackageTags) for Core, Backgammon, and Chess projects. (API layer temporarily removed)
- README packaged as NuGet readme (PackageReadmeFile) and included in artifacts.
- Initial CHANGELOG following Keep a Changelog format.
- Comprehensive XML documentation across public APIs (removed CS1591 suppression and achieved clean build with warnings-as-errors).
- Strategic architecture & DX action plan (`docs/plans/action-plan.md`) outlining DecisionPlan, deterministic RNG, pattern compilation, observability, simulation, and versioning roadmap.
- Internal feature flags scaffold (`FeatureFlags` class) to enable incremental rollout of upcoming engine subsystems.
- Benchmark project scaffold (`Veggerby.Boards.Benchmarks`) with initial HandleEvent baseline harness.
- Property tests project scaffold (`Veggerby.Boards.PropertyTests`) including first invariant placeholder.
- Experimental DecisionPlan scaffold (feature-flagged) compiling phase tree to linear leaf phase list (parity only, no optimizations yet).
- Deterministic RNG abstraction (`IRandomSource`, `XorShiftRandomSource`) and `GameState.Random` snapshot cloning.
- Baseline observability hooks: `IEvaluationObserver` (RuleEvaluated, RuleApplied, EventIgnored) + builder injection (future callbacks pending).
- Added `OnPhaseEnter` callback to observer (emitted in both legacy and DecisionPlan paths).
- EventResult placeholder struct for future observer/tracing integration (not yet used in public API).
- Documentation skeletons: `docs/decision-plan.md`, `docs/rng-and-timeline.md`.
- Feature-flagged state hashing scaffold (FNV-1a 64-bit) computing deterministic hash per `GameState` when `EnableStateHashing` is active.
- `IEvaluationObserver.OnStateHashed` callback invoked after successful rule application (hashing enabled only).
- Experimental timeline zipper (`GameTimeline`) with immutable undo/redo (flag-gated).
- 128-bit state hash (`Hash128`) computed alongside legacy 64-bit hash when hashing enabled (xxHash128 scaffold).
- Hashing overhead benchmark (`HashingOverheadBenchmark`) comparing per-event cost with hashing disabled/enabled.
- Pattern resolution benchmark scaffold (`PatternResolutionBaseline`) comparing legacy visitor vs (future) compiled resolver.
- Initial populated pattern compiler mapping FixedPattern and MultiDirectionPattern to compiled IR (feature-flagged, parity with legacy visitor for fixed sequences).
- Resolver exposure via `GameProgress.ResolvePathCompiledFirst` enabling feature-flagged runtime use of compiled patterns.
- Extended parity test suite (multi-direction, repeatable, null/unreachable) and chess integration parity test.
- Documentation: `docs/compiled-patterns.md` describing IR, resolver semantics, parity guarantees, and roadmap.
- Trace capture overhead benchmark (`TraceCaptureOverheadBenchmark`) comparing HandleEvent with trace enabled vs disabled.
- Trace capture scaffold (feature-flagged) recording PhaseEnter, RuleEvaluated, RuleApplied, EventIgnored, StateHashed entries for last evaluation (now enriched with rule index + condition reason fields).

### Changed

- Centralized all package versions via Directory.Packages.props (removed inline versions).
- README updated with roadmap reference.
- State hashing now uses canonical binary serialization (ordered public properties, typed tags) instead of transient ToString() output for stability.
- Added xxHash128-based 128-bit hash (non-cryptographic) for future Merkle / interning work; legacy 64-bit retained for transition.

### Fixed

- Resolved NU1008 central package version conflicts across solution.
- Restored successful build by temporarily suppressing XML documentation warning CS1591 (to be removed once full docs are authored).

### Internal / Maintenance

- Partial XML documentation added for key engine classes (GameEngine, GameBuilder, game-specific builders, selected state artifacts).
-- Removed API layer project and documentation (will return later as optional HTTP facade package).
- Established groundwork for completing remaining public XML docs.
- Removed temporary CS1591 suppression after completing full XML documentation coverage.
- Added initial scaffolds for performance benchmarking and property-based invariant testing.
- Fixed malformed XML documentation in `XXHash128` (removed CS1570 warnings) and enriched algorithm remarks.
- Added internal trace capture observer decorator (no public API exposure yet).

## [0.1.0] - Initial (Unreleased Tag)

- Initial codebase structure, game engine core, Backgammon and Chess modules, and test suite (354 tests passing).

[Unreleased]: https://github.com/veggerby/Veggerby.Boards/compare/v0.1.0...HEAD
