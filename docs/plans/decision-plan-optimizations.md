# DecisionPlan Optimizations (Masking & Grouping Draft)

Status: Draft Design (to be iteratively implemented behind feature flags)

## Context Recap

Current DecisionPlan (Phase 1 + micro-optimizations):

- Linear list of leaf phases: `DecisionPlanEntry { PhaseNumber, Condition, Rule, Phase, ConditionIsAlwaysValid }`.
- Phase reference cached (removed per-event ResolvePhase lookup).
- Predicate hoisting v1: trivially true `NullGameStateCondition` flagged to skip Evaluate.
- Evaluation currently: sequential scan; each entry either auto-valid (flag) or invokes `Condition.Evaluate(state)`; applies first valid rule.

Cost centers observed / anticipated:

1. Repeated evaluation of mutually exclusive conditions after one becomes valid.
2. Conditions that structurally cannot become valid given earlier failing predicates (logical dependency surfaces not encoded yet).
3. Traversal overhead for large phase sets when only a small subset can be relevant for a given event category.
4. Allocation / indirection in composite conditions (future).

## Optimization Goals

Measured, incremental improvements preserving determinism & transparency:

- Reduce average number of condition evaluations per event.
- Provide stable introspection (observer indexes unchanged, or versioned) across optimization phases.
- Avoid premature abstraction; prefer data-driven bitmasks & small structs.

Target (aggregate across representative scenarios after full rollout):

- ≥50% reduction in condition evaluations vs baseline linear scan.
- Maintain zero behavioral drift (verified via exhaustive parity tests & property tests).

## Core Concepts

### 1. Grouping

Logical partitioning of entries into evaluation groups sharing:

- A common *gate predicate* (cheap, coarse filter) evaluated once.
- Potentially a common *event filter* (e.g., event type categories) to short-circuit entire group.

Structure proposal:

```csharp
public readonly struct DecisionPlanGroup
{
    public readonly int StartIndex;   // first entry index in Entries
    public readonly int Length;       // number of entries in group
    public readonly IGameStateCondition Gate; // nullable (null => always)
    public readonly ushort MaskIndex; // index into mask table (optional future use)
}
```

Compilation rule (initial heuristic):

- Consecutive entries with identical condition reference OR identical condition type (stateless) form a group; the first becomes the gate, others reference no gate.
- Future: explicit grouping hints in `GamePhase` builder extension (opt-in attribute / fluent API).

Evaluation algorithm (group aware):

1. For each group:
   a. If `Gate` is null -> proceed.
   b. Else evaluate gate (hoisting if `DecisionPlanEntry.ConditionIsAlwaysValid` for first entry).
   c. If gate invalid -> skip group.
2. Within group: evaluate entries sequentially (still honoring per-entry hoisting); stop when rule applied.

### 2. Short-Circuit Masks

Represent potential skip sets as bitmasks keyed by gate outcomes or prior entry results.

Rationale: Some conditions are mutually exclusive (e.g., phase A valid -> phase B cannot be valid this event). Instead of evaluating B redundantly, we jump ahead.

Data model additions:

```csharp
public readonly struct DecisionPlanMask
{
    public readonly ulong BitsLow;  // support up to 128 entries initially (2x64)
    public readonly ulong BitsHigh;
    // Set bits indicate entries to SKIP when mask activated.
}
```

Link from entries or groups:

```csharp
public readonly struct DecisionPlanEntry
{
    // existing fields ...
    public readonly ushort SkipMaskIndex; // 0xFFFF => none
}
```

Activation semantics:

- When an entry evaluates to Valid, we OR its mask into an "active skip" bitset, and future iteration skips those indices.
- When an entry evaluates to Invalid, optional different mask (overengineering for v1; skip).

Mask derivation strategies (incremental):

1. Manual annotations (builder hints) for mutually exclusive branches.
2. Static inference pass: detect sets of entries sharing a common ancestor in original phase tree where semantics guarantee exclusivity (requires explicit invariant metadata—deferred until we formalize exclusivity markers on phases).

### 3. Event Type Bucketing (Filtering)

Add optional `EventKind` classification (enum or hash) to `IGameEventRule` / phases enabling pre-filtering of irrelevant entries.

Structure change (optional initial prototype):

```csharp
[Flags]
public enum EventKind : ulong { None=0, Move=1, Roll=2, Drop=4, Custom1=8, Any=~0UL }
```

Extend `DecisionPlanEntry` with `EventKind SupportedKinds`. At evaluation: if `(entry.SupportedKinds & currentKind)==0` -> skip without condition evaluation.

### 4. Observer Stability Plan

- Maintain existing `Entries` ordering index for external observers (trace aligns).
- Provide `OptimizationVersion` integer on plan. Increment when structural semantics (grouping/masks) change.
- Add debug-only verification mode to cross-check that skipping did not alter chosen rule vs naive scan.

### 5. Feature Flags

New flags (all default false):

- `EnableDecisionPlanGrouping`
- `EnableDecisionPlanMasks`
- `EnableDecisionPlanEventFiltering`
- `EnableDecisionPlanDebugParity` (forces shadow linear scan when true; asserts same outcome).

### 6. Incremental Rollout Phases

| Phase | Feature Set | Metrics | Exit Criteria |
|-------|-------------|---------|---------------|
| G1 | Group compilation + gate evaluation | Condition eval count | No behavior drift; >5% reduction on target scenarios |
| G2 | Event kind filtering (manual tagging) | Condition eval count per event type | >10% additional reduction when mixed event streams |
| M1 | Manual skip masks (builder hints) | Avg entries skipped after first valid | Safe mask application; no parity diff |
| M2 | Static exclusivity inference | Same | ≥50% total reduction milestone reached |
| D | Debug parity shadow path | Overhead characterization | Overhead <15% when enabled |

### 7. Public API Impact

Remain internal until stable; no public surface change except new feature flags & optional diagnostic properties:

```csharp
public sealed class DecisionPlan
{
    public int OptimizationVersion { get; }
    public IReadOnlyList<DecisionPlanEntry> Entries { get; }
    internal IReadOnlyList<DecisionPlanGroup> Groups { get; }
    internal IReadOnlyList<DecisionPlanMask> Masks { get; }
}
```

### 8. Testing Strategy

New tests per phase:

- Grouping: verify gate invoked once; invalid gate skips children; valid path parity.
- Event filtering: rule not evaluated when event kind mismatched.
- Masks: craft mutually exclusive conditions; ensure second branch skipped after first valid.
- Debug parity: force-enable dual path on randomized property set; assert identical chosen rule index.
- Negative: ensure false NullGameStateCondition not hoisted; gating + masks interplay not skipping needed entries.

Property-based extension: randomly generate phase trees annotated with exclusivity hints; compare naive vs optimized selected rule index across random event sequences.

### 9. Benchmark Plan

Add benchmarks:

- `DecisionPlanGroupingBaseline` (flags off vs grouping on).
- `DecisionPlanEventFilteringBaseline` (mixed event stream simulation).
- `DecisionPlanMaskingBaseline` (mutually exclusive scenario stress).
- Overhead benchmark for DebugParity shadow scan.

Metrics captured: average condition evaluations per event, rule application latency (ns), allocated bytes.

### 10. Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Incorrect mask causing rule skip | Debug parity flag; unit tests; shadow scan in CI optional job |
| Complexity creep | Phase gating and masks behind separate flags; stop if < measurable gain |
| Observer misalignment | Preserve original entry order; expose `OptimizationVersion` |
| Overhead of shadow verification | Make opt-in; micro-benchmark its cost |

### 11. Implementation Order (Actionable Tasks)

1. Add feature flags + scaffolds (no behavior change).

2. Implement grouping compilation & evaluation branch (G1) + tests + benchmark.
3. Introduce EventKind tagging (backfill Move/Roll kinds) + filtering logic (G2) + tests.
4. Add builder hint API for exclusivity/mask annotation + masking evaluation (M1) + tests + benchmark.
5. Debug parity mode (shadow scan + assertion) + overhead benchmark.
6. Static exclusivity inference prototype (M2) + property tests.
7. Documentation updates & finalize optimization versioning.

### 12. Open Questions

- Should EventKind live on rules or phases? (Leaning: rule; phase might host multiple rules in future.)
- Bitmask size >128? (Defer until real scenarios exceed; can extend to 256 via 4x64 fields.)
- Exclusivity inference: require explicit marker interface on conditions? (Simplest: attribute `[MutuallyExclusiveGroup("token")]`).

### 13. Out-of-Scope (For Now)

- JIT code generation / expression tree fusion.
- Condition result caching across events (needs invalidation semantics).
- Probabilistic profiling-driven reordering.

---
Prepared: 2025-09-21
