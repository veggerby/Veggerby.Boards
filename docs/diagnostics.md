# Diagnostics & Observability (Preview)

> Status: Planned (Phase 1 minimal hooks, Phase 3 full trace & CLI).

## Components

- `IEvaluationObserver` (rule evaluated, state hashed; later: phase enter, mutator applied).
- JSON evaluation trace (compact, fixed schema, captures rule index, reason, 128-bit hash).
- CLI visualizer (Phase 3) reading trace to display failure chain & path overlays (future).

## Goals

- Near-zero overhead when disabled (<5%).
- Actionable rejection reasons for test assertions and tooling.
- Deterministic trace reproduction for bug reports.

## Feature Flags

- Trace capture: `FeatureFlags.EnableTraceCapture`.

## Metrics

- Overhead ≤5% in microbenchmarks with observer enabled.
