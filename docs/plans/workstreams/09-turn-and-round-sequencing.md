---
id: 9
slug: turn-and-round-sequencing
name: "Turn & Round Sequencing"
status: partial
last_updated: 2025-09-26
owner: core
flags:
  - EnableTurnSequencing (exp)
summary: >-
  Establishes TurnState segmentation (advance, pass, commit, replay) with overhead benchmarks; richer multi-round orchestration
  and pass-streak heuristics pending.
acceptance:
  - TurnState model integrated without breaking existing rule evaluation.
  - Overhead benchmark captured baseline costs (< target threshold documented elsewhere).
open_followups:
  - Multi-round orchestration API (round summary, phase resets).
  - Pass streak termination heuristic configuration.
  - Replay loop guard rails (max depth, diagnostics event).
  - Sequencing visualization (docs diagram).
---

# Turn & Round Sequencing

## Delivered

- TurnState segmentation model
- Overhead benchmark instrumentation
- Integration path w/out evaluation interruptions

## Pending / Deferred

See open_followups in front matter.

## Risks

Unbounded replay loops could stall progression; complex round resets may introduce nondeterminism if mis-scoped.

## Next Steps

Implement replay guard rails then multi-round orchestration API.
