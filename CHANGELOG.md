# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Package metadata (Description, PackageTags) for Core, Backgammon, Chess and API projects.
- README packaged as NuGet readme (PackageReadmeFile) and included in artifacts.
- Initial CHANGELOG following Keep a Changelog format.

### Changed

- Migrated test assertions from FluentAssertions to AwesomeAssertions.
- Centralized all package versions via Directory.Packages.props (removed inline versions).

### Fixed

- Resolved NU1008 central package version conflicts across solution.
- Restored successful build by temporarily suppressing XML documentation warning CS1591 (to be removed once full docs are authored).

### Internal / Maintenance

- Partial XML documentation added for key engine classes (GameEngine, GameBuilder, game-specific builders, selected state artifacts).
- Established groundwork for completing remaining public XML docs.

## [0.1.0] - Initial (Unreleased Tag)

- Initial codebase structure, game engine core, Backgammon and Chess modules, and test suite (354 tests passing).

[Unreleased]: https://github.com/veggerby/Veggerby.Boards/compare/v0.1.0...HEAD
