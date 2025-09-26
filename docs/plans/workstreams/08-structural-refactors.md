---
id: 8
slug: structural-refactors
name: "Structural Refactors"
status: partial
last_updated: 2025-09-26
owner: core
flags:
  - EnableTimelineZipper (exp)
  - EnableHashing (on)
summary: >-
  Introduces timeline zipper concept + dual hashing (64/128) scaffolding for deterministic integrity; major reification of zipper
  operations and hash collision test matrix outstanding.
acceptance:
  - Hash scaffolding integrated with state snapshots (non-invasive).
  - Timeline zipper prototype behind flag (no behavioral regressions when off).
open_followups:
  - Full zipper adoption for navigation (prev/next/branch queries).
  - Collision stress tests (random + adversarial boards, >1M states).
  - Hash invalidation triggers on mutator application verification.
  - Serialization canonical ordering conformance tests.
---

# Structural Refactors

## Delivered

- Hash scaffolding (64/128-bit) placeholders
- Timeline zipper prototype (flag-gated)
- Non-invasive integration with existing state chain

## Pending / Deferred

See open_followups in front matter.

## Risks

Insufficient collision testing could mask structural hash weaknesses; premature zipper adoption risks complexity.

## Next Steps

Build collision stress harness then migrate navigation APIs onto zipper abstraction.
