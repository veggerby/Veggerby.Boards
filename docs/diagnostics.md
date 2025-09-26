# Diagnostics & Observability

Deterministic transparency into rule evaluation & state evolution.

## Observer Interface

`IEvaluationObserver` receives callbacks (rule evaluated, rule applied, state hashed, trace events when enabled). Default is no-op.

## Trace Capture

Flag: `EnableTraceCapture`. Captures ordered entries (PhaseEnter, RuleEvaluated, RuleApplied, EventIgnored, StateHashed) for the last handled event batch. Intended for debugging & future visualization.

## Event Results

`HandleEventResult` surfaces: Applied (bool), Reason (enum), Message (diagnostic). Prefer over exception-driven control for normal invalid moves.

### Rejection Reasons (Summary)

| Reason | Meaning | Typical Trigger |
|--------|---------|-----------------|
| PhaseClosed | No active phase accepted the event | No leaf phase condition valid |
| NotApplicable | No rule produced a state change | Rules matched type but all no-op OR ignored |
| InvalidOwnership | Ownership / source mismatch | Moving opponent piece, wrong starting tile |
| PathNotFound | Movement/path resolution failed | No geometric path or dice/state gating failed |
| RuleRejected | A rule condition returned Invalid | Explicit event condition denial |
| InvalidEvent | Malformed or structurally invalid event | Missing payload, inconsistent artifacts |
| EngineInvariant | Internal invariant breach | Unexpected exception path (investigate) |

Determinism: identical state + identical event yields identical reason classification.

## Metrics

Fast-path hit ratios, decision plan skip reasons, sequencing overhead—all exposed via internal snapshots or observer events (public metrics facade TBD).

## Best Practices

* Use structured reasons in tests (assert on `Reason` not message text).
* Enable trace capture only when diagnosing (overhead budget <5%).
* Keep observer implementations pure / side-effect minimal.
