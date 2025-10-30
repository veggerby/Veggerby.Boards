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

## 2. LINQ Usage in Potential Hot Paths

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

## 3. Repeated `.ToList()` Snapshotting

`GameSimulator` batch progress uses `list.ToList()` to create snapshots for metrics display.
Opportunity:

- Replace with defensive copy only when observers attached or count changed threshold; or maintain immutable array segments.
Status: Candidate (only if profiling indicates overhead).
Effort: M.

## 4. Collections Allocation Patterns

Numerous `List<T>` allocations in DeckBuilding mutators cloning dictionaries of piles.

Update:

- Selective cloning applied to TrashFromHand, CleanupToDiscard, DrawWithReshuffle, GainFromSupply mutators (only mutated piles now copied).

Remaining Opportunity:

- Investigate further struct-based `DeckPiles` copy-on-write if profiling shows residual hotspot.
Status: Candidate (partial mitigation delivered; profiling pending for next stage).
Effort: L (for full struct abstraction) / S (for further selective tweaks).

## 5. Simulation Trace Handling

Now always non-null (empty when disabled).
Opportunity:

- Provide `TraceCount` property to avoid `Trace.Count` (still fine—list or array) but minor.
Status: Acceptable.
Effort: S (optional).

## 6. Feature Flags Spread

Feature flag checks sprinkled (e.g., `FeatureFlags.EnableTurnSequencing` inside simulation loop).
Opportunity:

Update:


Status: Active (extend to other hot loops if profiling indicates benefit).
Effort: S.


## 9. Missing XML Docs (Spot Sampling)

Not all internal classes have XML docs (acceptable), but any public-facing class lacking docs should be audited.
Opportunity:

Status: Active.
Effort: M.


## 7. Domain-Specific Exceptions

Custom exceptions limited; builder error conditions use generic exceptions.
Opportunity:

Status: Candidate.
Effort: M.

## Deferred / Needs Profiling


## Monitoring

Watch for reintroduction of LINQ in hot paths and potential complexity creep in deck pile cloning logic.

One-liner: Structural hotspots addressed; remaining opportunities now focus on API clarity, selective profiling-driven optimizations (bitboards, remaining deck structure), and maintaining deterministic performance.
