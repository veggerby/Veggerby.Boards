# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Package metadata (Description, PackageTags) for Core, Backgammon, Chess and API projects.
- README packaged as NuGet readme (PackageReadmeFile) and included in artifacts.
- Initial CHANGELOG following Keep a Changelog format.
- Comprehensive XML documentation across public APIs (removed CS1591 suppression and achieved clean build with warnings-as-errors).
- Strategic architecture & DX action plan (`docs/plans/action-plan.md`) outlining DecisionPlan, deterministic RNG, pattern compilation, observability, simulation, and versioning roadmap.
- Internal feature flags scaffold (`FeatureFlags` class) to enable incremental rollout of upcoming engine subsystems.
- Benchmark project scaffold (`Veggerby.Boards.Benchmarks`) with initial HandleEvent baseline harness.
- Property tests project scaffold (`Veggerby.Boards.PropertyTests`) including first invariant placeholder.

### Changed

- Centralized all package versions via Directory.Packages.props (removed inline versions).
- README updated with roadmap reference.

### Fixed

- Resolved NU1008 central package version conflicts across solution.
- Restored successful build by temporarily suppressing XML documentation warning CS1591 (to be removed once full docs are authored).

### Internal / Maintenance

- Partial XML documentation added for key engine classes (GameEngine, GameBuilder, game-specific builders, selected state artifacts).
- Established groundwork for completing remaining public XML docs.
- Removed temporary CS1591 suppression after completing full XML documentation coverage.
- Added initial scaffolds for performance benchmarking and property-based invariant testing.

## [0.1.0] - Initial (Unreleased Tag)

- Initial codebase structure, game engine core, Backgammon and Chess modules, and test suite (354 tests passing).

[Unreleased]: https://github.com/veggerby/Veggerby.Boards/compare/v0.1.0...HEAD
