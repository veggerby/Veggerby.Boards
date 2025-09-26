---
id: 6
slug: observability-and-diagnostics
name: "Observability & Diagnostics"
status: partial
last_updated: 2025-09-26
owner: core
flags:
  - EnableTraceCapture (exp)
  - EnableEventKindFiltering (on)
  - EnableGrouping (on)
summary: >-
  Establishes explicit rejection / result taxonomy, trace capture prototype, grouping + kind filtering to bound evaluation scope;
  advanced span correlation & streaming trace export not implemented yet.
acceptance:
  - Unified EventResult with rejection reasons (Invalid, NotApplicable, Ignored) consumed in tests.
  - Grouping + kind filtering reduces candidate rules without correctness regressions.
  - Trace capture gated behind flag; deterministic replay matches when enabled/disabled.
open_followups:
  - Streaming trace export (NDJSON) for long-running simulations.
  - Span correlation IDs across phases & nested evaluations.
  - Compact binary trace envelope for bulk archival.
  - Observer skip reason metrics + counters.
  - Failure fingerprint (hash) for cluster analysis.
---

# Observability & Diagnostics

## Delivered

- EventResult taxonomy & rejection reasons
- Rule grouping (structural) + event kind filtering
- Trace capture prototype (flag-gated)
- Deterministic replay verification with tracing enabled
- Preliminary skip reason enumeration

## Pending / Deferred

See open_followups in front matter.

## Risks

Trace retention could cause memory growth; lack of streaming export delays large scale analysis.

## Next Steps

Implement streaming NDJSON exporter, then instrumentation counters for skip reasons + failure fingerprints.
