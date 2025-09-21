# Deterministic RNG & Timeline (Preview)

> Status: Planned (Phase 1–2). Will expand with concrete serialization layout and hashing details.

## Components

- `IRandomSource` (xoroshiro/xoshiro based) with fixed-length serialization.
- `GameTimeline` zipper (Past stack, Present, Future stack) enabling O(1) undo/redo.
- Merkle-style 128-bit state hash over canonical ordered payload.

## Goals

- Reproducible simulations (seed + event stream = deterministic final state hash).
- Cheap undo/redo for tooling & exploration.
- Bug report envelope for replay in CI and OSS issues.

## Hash Payload (Summary)

See canonical list in action plan (`docs/wip/2025-09-21-action-plan.md`) — game identity, decision plan version, phase/player ids, piece states, dice, module extensions.

## Feature Flags

- Hashing: `FeatureFlags.EnableStateHashing` (off until stable, then default on).

## Metrics

- Zero divergence across supported platforms (Linux, Windows; ARM optional) for identical seeds & event sequences.
