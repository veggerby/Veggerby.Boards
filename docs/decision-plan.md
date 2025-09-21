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

Deferred (later phases): short-circuit masks, predicate hoisting, caching.

Disabling the flag reverts to legacy behavior with identical outcomes.

## Metrics

Initial target: ≥30% p50 latency reduction on representative move scenarios (Chess opening, Backgammon entry) in Phase 1.
