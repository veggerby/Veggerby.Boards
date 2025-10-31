# Code Quality & Hygiene Opportunities

Date: 2025-10-27
Date: 2025-10-27 (pruned 2025-10-31)
Branch: core/test-assertion-and-structure

## Overview

Active and candidate improvement areas (completed items removed). Focus areas: allocation hygiene, LINQ usage risk in hot paths, lifecycle null suppression elimination, API clarity, documentation coverage. Determinism & immutability are non-negotiable.

## Legend

- Status: Active (needs action), Candidate (evaluate/queued), Watch (monitor only), Acceptable (intentionally deferred).
- Effort: S (small), M (medium), L (large).

## 1. Lifecycle Null-Forgiving Usage

Definitions still rely on `null!` with lifecycle comments and `[MemberNotNull]` attributes.
Opportunity: Evaluate migration to `required` init properties to remove `null!` while retaining builder fluency.
Status: Candidate
Effort: M

## 2. LINQ Usage in Potential Hot Paths

Examples: `GameBuilder` artifact enumeration chains; simulation shuffle materialization; reason aggregation via `string.Join`.
Opportunity: Manual loops for repeated pattern materialization if profiling shows build cost dominance.
Status: Watch
Effort: S-M

## 3. Repeated `.ToList()` Snapshotting

`GameSimulator` progress snapshots allocate lists for metrics.
Opportunity: Snapshot only when observers present or use immutable segment strategy.
Status: Candidate
Effort: M

## 4. DeckBuilding Collections Allocation Patterns (Partial Mitigation Done)

Selective cloning applied (mutated piles only). Further gains may come from struct-based copy-on-write for `DeckPiles`.
Opportunity: Prototype struct COW if profiling shows pile cloning remains hotspot.
Status: Candidate
Effort: L (struct abstraction) / S (additional selective tweaks)

## 5. Simulation Trace Handling

Trace always non-null; micro improvement possible.
Opportunity: Add `TraceCount` property to avoid list count indirection (low impact).
Status: Acceptable
Effort: S

## 6. Feature Flags Spread

Flag checks appear in simulation loop; consistency of pattern across other loops not audited.
Opportunity: Centralize fast-path flag evaluation or batch snapshot if profiling reveals branch cost.
Status: Active
Effort: S

## 7. Domain-Specific Exceptions

Builder errors sometimes use generic exceptions.
Opportunity: Introduce targeted domain exceptions where they sharpen diagnostics (avoid proliferation).
Status: Candidate
Effort: M

## 8. Missing XML Docs (Public Surface)

Audit for any public type/method lacking XML docs (internal omitted okay).
Opportunity: Coverage pass + lightweight test/assert on doc presence for new public APIs.
Status: Active
Effort: M

## Monitoring

Keep LINQ out of confirmed hot paths; observe deck pile cloning and sliding attack generation in future perf runs for regression.

One-liner: Remaining opportunities emphasize profiling-guided allocation trimming and clearer lifecycle/API semantics while guarding deterministic behavior.
