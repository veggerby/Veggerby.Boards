# DecisionPlan (Experimental)

Status: Experimental (behind `FeatureFlags.EnableDecisionPlan`).

The DecisionPlan is a precompiled linear list of leaf phases (condition + rule). Phase 1 provides parity only.

Enable (before `Compile()`):

```csharp
FeatureFlags.EnableDecisionPlan = true;
```

Goals (Phase 1):

1. Parity with legacy traversal
2. Stable indexes for upcoming observer/tracing

Deferred (later phases): advanced predicate hoisting layers, automatic exclusivity inference, bitmask compression, caching heuristics.

## Current Optimization Stages (Flag-Gated)

All optimizations are opt-in via discrete feature flags to preserve deterministic parity until individually validated.

| Stage | Flag | Description |
|-------|------|-------------|
| Base Plan | `EnableDecisionPlan` | Linear precompiled leaf phase list (parity only) |
| Grouping | `EnableDecisionPlanGrouping` | Collapses contiguous identical state predicate entries; evaluates predicate once per group |
| Event Filtering | `EnableDecisionPlanEventFiltering` | Pre-classifies rules by coarse `EventKind` (e.g., Move, Roll) to skip non-relevant entries early |
| Exclusivity Masks | `EnableDecisionPlanMasks` | Skips later entries whose exclusivity group root already produced an applied rule (mutual exclusion hint) |
| Debug Parity | `EnableDecisionPlanDebugParity` | Dual-run: executes legacy evaluator in shadow; compares resulting state; throws on divergence (safety harness) |

### Exclusivity Groups & Masks

Builder API:

```csharp
AddGamePhase("capture-window")
  .Exclusive("combat-cycle")
  .If<NullGameStateCondition>()
  .Then()
    .ForEvent<MovePieceGameEvent>()
    .Then()
      .Do<MovePieceStateMutator>();
```

Semantics:

1. Phases tagged with the same non-null group are declared mutually exclusive candidates.
2. The compiler records two arrays in the plan: `ExclusivityGroups` (group id per entry or null) and `ExclusivityGroupRoots` (index of first occurrence for each entry or -1 when ungrouped).
3. At runtime (when `EnableDecisionPlanMasks` is true) the evaluator tracks which root indices have produced an applied rule for the current event. Subsequent entries mapping to those roots are skipped entirely (predicate, pre-processors, and rule). This reduces redundant predicate evaluations in mutually exclusive families where only the first applicable rule should have effect.
4. Parity: With the mask flag disabled, all entries evaluate as before (no functional change). Tests assert masked vs unmasked correctness (`DecisionPlanMaskingTests`).

Design Notes:

- Exclusivity masking is intentionally conservative (applies only after a rule "applies" not merely evaluates as Valid but then ignored). This preserves semantics in scenarios where multiple exclusive predicates may validate yet only later rules produce state changes.
- A future enhancement may incorporate precondition short-circuit (skip evaluation of predicates once any earlier predicate in the same exclusivity group returns Valid) when proven semantically safe for given rule categories.

Edge Cases:

- Interleaving grouped predicates and exclusivity: masking logic uses root indices so groups inside the same exclusivity root are still visited until an application occurs.
- Nested composites are flattened prior to exclusivity analysis; group ids are compared as opaque strings.

Observability:

- Future work will surface the optimization stage (and masking decisions) via observer trace entries when debug parity mode is introduced.

Disabling the flag reverts to legacy behavior with identical outcomes.

### Debug Parity (Dual-Run Safety Harness)

When `EnableDecisionPlanDebugParity` is active, every handled event is also evaluated through the legacy traversal path (extracted as `HandleEventLegacy`). The resulting `GameState` instances are compared for structural equality. A mismatch raises a `BoardException` summarizing divergent artifact ids. A test-only hook `DebugParityTestHooks.ForceMismatch` can simulate a divergence to assert the safety net. This mode is intended for incremental rollout of additional optimizations (filtering, masking, hoisting) and should remain enabled in CI until all DecisionPlan stages have stable parity metrics.

Overhead: Expected marginally higher due to duplicate evaluation; benchmarks to be added before graduating new optimization stages.

### EventKind Expansion (Preparation)

The `EventKind` enum now anticipates broader categorization (`Move`, `Roll`, `State`, `Phase`, `Custom1`, `Custom2`, `Any`). Current classifier still only assigns `Move` or `Roll`; all other events default to `Any`. This staged expansion allows future rules (e.g., active player selection, phase control) to opt into more selective filtering without refactoring existing logic. Until events are explicitly tagged, runtime behavior is unchanged.

## Metrics

Initial target: ≥30% p50 latency reduction on representative move scenarios (Chess opening, Backgammon entry) in Phase 1.
