# DecisionPlan Parity Strategy

Status: Draft
Scope: Rule Evaluation Engine Modernization / Observability & Diagnostics

## Purpose

Ensure the compiled DecisionPlan evaluation pipeline produces externally identical outcomes to the legacy phase traversal before deprecating the legacy path.

## Parity Dimensions

| Dimension | Definition | Current Coverage |
|-----------|------------|------------------|
| Piece positions | Final tile id for each tracked piece | Curated + Randomized tests |
| State hash (64) | `GameState.Hash` when enabled | Curated (hashing=true) |
| State hash (128) | `GameState.Hash128` when enabled | Curated (hashing=true) |
| Ignored events | Illegal / inapplicable events must not mutate state | Curated scenario `illegal-event-ignored` |
| Move sequencing | Ordering of accepted events leads to same terminal history depth | (Implicit) |
| Rule gating reasons | Observer taxonomy parity (future) | Planned |

## Test Layers

1. Curated deterministic scenarios (`DecisionPlanCuratedParityPackTests`) – representative targeted behaviors.
2. Deterministic randomized sequences (`DecisionPlanRandomizedParityTests`) – low-cost stochastic coverage of interaction permutations.
3. Future: Property / fuzz expansions (piece capture, branching resolution, rule exclusivity masks).

## Feature Flag Gating

All parity verifications run with the following toggles to isolate axes:

- `EnableDecisionPlan` (false → true transition) – primary axis.
- Other DecisionPlan modifiers (`Grouping`, `EventFiltering`, `Masks`) remain off in parity suites until individual feature parity harnesses are added.

## Exit Criteria for Legacy Removal

A. Two consecutive releases with all parity suites green (no transient divergences).
B. Hash parity validated for both 64-bit and 128-bit modes across curated + randomized suites.
C. Additional suites cover: grouping on/off, masks on/off (exclusivity), event kind filtering.
D. Observer reason taxonomy mirrored (no reason code drift).
E. Benchmark (DecisionPlanVsLegacyBenchmark) shows neutral or improved P50, P95 for evaluation.
F. No open TODO markers referencing legacy path.

## Planned Additions

- [ ] Masks parity suite (force collisions inside exclusivity groups).
- [ ] Grouping parity suite (gate predicate run-count assertions).
- [ ] EventKind filtering parity (synthetic mixed-kind rule set).
- [ ] Observer reason parity (instrumentation capture compare).
- [ ] Capture / blocked move scenarios (chess + backgammon).
- [ ] Cross-module parity (Backgammon piece entry / bearing off).
- [ ] Replay harness to re-run serialized event logs through both engines.

## Operational Notes

- Randomized tests fixed seeds → deterministic failures.
- Any divergence should dump minimal diff (avoid large dumps) and recommend enabling `EnableDecisionPlanDebugParity` locally for richer diagnostics.
- Hash mismatches are highest severity; treat as potential mutator non-determinism or ordering leaks.

## Removal Procedure (When Criteria Met)

1. Flip default `EnableDecisionPlan = true` in `FeatureFlags`.
2. Mark `HandleEventLegacy` `[Obsolete("Removed – DecisionPlan baseline")]` one release prior to deletion.
3. Remove legacy traversal code and delete parity-only feature flags (debug parity mode may remain as test-only).
4. Archive final benchmark delta in `docs/release-notes/`.

---
This document complements `diagnostics.md` and `decision-plan.md` (future). Keep changes small, measurable, and flag guarded until exit criteria satisfied.
