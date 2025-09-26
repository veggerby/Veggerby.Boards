# DecisionPlan & Acceleration

Compiled rule evaluation pipeline + performance layers. Semantics remain invariant; only latency / allocation profile changes with flags.

## Pipeline

1. Builder flattens phases/rules into ordered entries (leaf phases only).
2. DecisionPlan stores rule metadata (event type, conditions, mutators, exclusivity group roots, masks, optional pre-processors).
3. Evaluation iterates plan applying optimization stages in order.

## Optimization Stages (Flags)

| Stage | Flag | Effect |
|-------|------|--------|
| Grouping | `EnableDecisionPlanGrouping` | Evaluate shared predicate once for grouped entries |
| EventKind Filtering | `EnableDecisionPlanEventFiltering` | Skip rules whose coarse kind mismatches current event |
| Exclusivity Masks | `EnableDecisionPlanMasks` | Skip remaining entries in a mutually exclusive group once one applied |

All stages individually optional; disabling reverts to plain linear evaluation.

## Movement Acceleration Layers

| Layer | Flag(s) | Description |
|-------|---------|-------------|
| Compiled Patterns | `EnableCompiledPatterns` | Precompiled IR for pattern traversal |
| Bitboards | `EnableBitboards` | Occupancy & attack masks for ≤64 tiles |
| Sliding Fast-Path | `EnableSlidingFastPath` | Ray reachability + path reconstruction shortcut |

Fast-path attempts only for repeatable directional pattern pieces. Parity tests ensure identical outcomes.

## Exclusivity Groups

Mark mutually exclusive phase branches (builder `.Exclusive("group-id")`). With masking flag, later entries in the same group are skipped after first applying rule.

## Rejection Mapping

Structured `EventResult` reasons aid diagnostics & automation (see `diagnostics.md`).

## Adding a New Optimization

1. Write semantics charter + success metrics.
2. Implement behind new flag (default OFF).
3. Add parity tests (behavior) + benchmarks (latency & allocations).
4. Update docs & CHANGELOG before enabling by default.
