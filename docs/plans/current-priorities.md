---
slug: current-priorities
name: "Current High-Impact Priorities"
last_updated: 2025-10-05
owner: core
summary: >-
  Ranked, leverage-focused backlog slice: sequencing graduation, performance layout, diagnostics viewer, governance, and
  deterministic primitives that unlock multiple downstream game modules.
---

# Current High-Impact Priorities

This document distills the most leverage-heavy tasks across `status.md` and active/planned workstreams. Ranking factors:

* Breadth of reuse (multi-module impact)
* Determinism integrity & risk reduction
* Developer velocity and observability impact
* Performance scalability and allocation behavior

## Tier 1 – Foundational Multipliers

1. **Turn Sequencing Graduation (Status 9)**
   Core enabled-by-default (advancement, pass, replay, commit, rotation helper) – remaining: Go two-pass terminal wiring, legacy active player projection removal, hash parity test. Docs sequencing note delivered (helper usage guidance).
   Impact: Enables clean phase-driven loops for Monopoly, Deck-building, Ludo, Risk.

2. **Performance Data Layout & Bitboards (Status 4)**
   Incremental bitboard updates (soak phase) + Bitboard128 scaffolding (global/per-player up to 128 tiles) landed; next: graduation criteria, per-piece masks, heuristic pruning, LINQ sweep.
   Impact: Core for scaling complex move generation and territory evaluation.

3. **Diagnostics Consumability (Status 6)**
   Minimal CLI trace viewer + skip classification surfacing.
   Impact: Accelerates debugging legality/capture/phase issues across all modules.

4. **Analyzer & CI Hardening (Status 7 + Cross-Cutting)**
   Roslyn rules (Random usage, LINQ-in-hot-path), cross-platform determinism CI, style enforcement.
   Impact: Prevents entropy & regression; locks in determinism guarantees.

5. **Feature Flag Governance Table (Cross-Cutting)**
   Create `feature-flags.md` enumerating flags (state, graduation criteria, owner).
   Impact: Stops uncontrolled proliferation & clarifies stabilization path.

## Tier 2 – Pattern-Setting Engine Implementations

1. **Go Capture & Liberty Resolution (Workstream 11)**
   Implement group/liberty resolver, capture, suicide, ko.
   Impact: Establishes reusable graph/region evaluation strategies.

1. **Chess Pseudo-Legal + Legality Completion (Workstream 10)**
   Generation API, promotion, mate/stalemate, final SAN details.
   Impact: Canonical complex movement + legality reference implementation.

1. **Deterministic Shuffle Artifact (Workstreams 14 & 17)**
   Seeded reproducible deck/supply permutation with captured provenance.
   Impact: Standard pattern for any future card/randomized ordering mechanic.

1. **Checkers Capture Chain Enumeration (Workstream 13)**
   Deterministic multi-branch path enumeration + stable ordering.
   Impact: Reusable branching resolution pattern.

1. **Risk Reinforcement & Combat Resolution (Workstream 16)**
    Reinforcement condition + dice comparison engine + conquest transitions.
    Impact: Template for contested multi-dice event resolution.

## Tier 3 – Strategic Extensions / Validation Modules

1. **Deck-building Phase & Zone Engine (Workstream 17)** – Validates phased zone transitions & shuffle lifecycle.
1. **Ludo Core (Workstream 12)** – Simple race abstraction; good tutorial example.
1. **Monopoly Economic Loop (Workstream 14)** – Ownership + cash flow baseline (deferred complexity guarded).

## Quick Wins (≤1 Day Each)

* `feature-flags.md` scaffold.
* Minimal `trace-view` CLI (filter by phase/event kind).
* Deterministic shuffle prototype + reproducibility test.
* Roslyn rule: forbid direct `System.Random` in core.

## Suggested Execution Order

1. Turn Sequencing
2. Diagnostics Viewer
3. Feature Flags + Analyzer/CI (parallelizable)
4. Performance Layout Enhancements
5. Go Capture/Liberty
6. Chess Generation Completion
7. Deterministic Shuffle Artifact
8. Checkers Capture Chain
9. Risk Reinforcement/Combat
10. Deck-building Phases
11. Ludo
12. Monopoly

## KPI Examples

* Determinism: 100% cross-platform replay parity (Linux/Windows/macOS) for a canonical scenario suite.
* Performance: Move/capture generation target latency per 1000 ops (benchmark TBD) post-bitboard incremental path.
* Observability: Trace-to-filtered-view path ≤ 3 steps.

## Follow-Up Candidates

* Consolidated benchmark summary (commit hash keyed).
* Test taxonomy tagging (generation | sequencing | resolution | economics | shuffle).
* Adoption playbook: new module checklist referencing required engine extension points.

---
_End of current priorities._
