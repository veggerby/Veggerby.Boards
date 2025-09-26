---
id: 1
slug: rule-evaluation-engine-modernization
name: "Rule Evaluation Engine Modernization"
status: done
last_updated: 2025-09-26
owner: core
flags:
  - EnableDecisionPlanGrouping (exp)
  - EnableDecisionPlanEventFiltering (exp)
  - EnableDecisionPlanMasks (exp)
summary: >-
  Replaced legacy traversal with a single DecisionPlan evaluator supporting grouping, event kind filtering,
  static + manual exclusivity masking, predicate hoisting, typed EventResult, and observer skip taxonomy.
acceptance:
  - Legacy traversal removed; DecisionPlan is sole execution path.
  - Parity tests green across deterministic openings and randomized samples.
  - Observer overhead within target (≤5%).
  - Typed EventResult with stable rejection reasons documented.
  - Debug parity harness removed once confidence established.
open_followups:
  - Finer-grained skip classification (invalid vs ignored) (diagnostics enhancement).
  - Composite skip capture (multi-reason recording) (diagnostics enhancement).
  - Decide graduation of grouping/filtering/masks defaults after consolidated perf audit.
---

# Rule Evaluation Engine Modernization

## Overview

Modernizes rule evaluation by compiling phases and rules into a linear DecisionPlan. Eliminates legacy depth-first traversal.

## Delivered

- DecisionPlan immutable model & executor
- Grouping optimization (duplicate predicate collapse)
- EventKind filtering (Move, Roll, State, Phase)
- Manual & static exclusivity masking
- Predicate hoisting for trivially true conditions
- Observer instrumentation (evaluated/applied/skipped/phase enter/state hashed)
- Skip taxonomy (EventKindFiltered, ExclusivityMasked, GroupGateFailed)
- Typed `EventResult` + `EventRejectionReason`
- Removal of legacy traversal & debug parity harness

## Metrics

- Latency improvements localized (final consolidated benchmark summary pending separate perf dossier)
- Overhead targets for observer batching tracked under Observability workstream

## Risks

Residual complexity creep if additional masking layers added without perf justification.

## Next Steps

See open_followups front matter list.
