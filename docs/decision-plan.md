# DecisionPlan (Preview)

> Status: Planned (Phase 1). This document will be expanded as the implementation lands.

## Overview

The DecisionPlan is a pre-compiled, immutable representation of rule evaluation for a game instance. It replaces per-event dynamic condition scanning with tight, array-based iteration over pre-bound predicate delegates and associated mutators.

## Goals

- Reduce branching & allocations in `HandleEvent`.
- Provide stable rule indexes for tracing and diagnostics.
- Enable short-circuit optimizations in later phases without altering public behavior.

## Structure (Draft)

- Phases array (stable ids)
- Rules array (phase id, predicates[], mutator)
- Phase gate bitsets (v1 simple, v2 optional masks)

## Feature Flag

Controlled by `FeatureFlags.EnableDecisionPlan` until parity and performance targets are met.

## Parity Strategy

A legacy evaluator path will remain in tests (compile symbol) to assert identical results across a corpus of generated event sequences until removal criteria are satisfied.

## Metrics

Initial target: ≥30% p50 latency reduction on representative move scenarios (Chess opening, Backgammon entry) in Phase 1.
