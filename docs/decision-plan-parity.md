# DecisionPlan Parity Strategy (Historical Archive)

Status: Retired (legacy traversal removed, parity harness decommissioned)
Scope (historical): Rule Evaluation Engine Modernization / Observability & Diagnostics

## Summary

This historical document recorded the strategy used to validate the DecisionPlan against the legacy rule traversal. The goals were achieved and the legacy path, along with the associated feature flags (`EnableDecisionPlan`, `EnableDecisionPlanDebugParity`), has been removed. Current validation relies on targeted behavioral tests, state hashing invariants, and optimization-specific tests (grouping, filtering, masking) rather than dual-run comparisons.

## Historical Parity Dimensions

The following dimensions were previously compared across curated and randomized suites:

- Piece positions (tile occupancy parity)
- 64-bit and 128-bit state hashes
- Ignored / illegal event inertness
- Implicit move sequencing depth parity
- (Planned) observer skip reason taxonomy

## Retired Test Layers

Legacy test files (`DecisionPlan*Parity*`) implemented:

1. Curated deterministic scenarios
2. Deterministic randomized pawn move sequences

They have been replaced by DecisionPlan-only behavior tests now that a legacy baseline no longer exists.

## Feature Flags (Removed)

`EnableDecisionPlan` and `EnableDecisionPlanDebugParity` were temporary migration flags. Both are deleted. Optimization flags (`EnableDecisionPlanGrouping`, `EnableDecisionPlanEventFiltering`, `EnableDecisionPlanMasks`) remain for incremental performance validation but operate on the always-on evaluator.

## Exit Criteria (Met)

All historical exit criteria (sustained parity, hash stability, benchmark neutrality, absence of TODOs) were satisfied prior to removal.

## Operational Guidance (Superseded)

Shadow dual-run parity should not be reintroduced unless a future large-scale evaluator rewrite occurs. Prefer focused invariant/property tests and deterministic replay harnesses.

## Archive Note

This file is frozen for reference. Do not extend. See `decision-plan.md` for current evaluator design and optimization flags.
