# Feature Flags

All experimental and optimization subsystems are gated behind feature flags. Defaults reflect the current engine state in `src/Veggerby.Boards/Internal/FeatureFlags.cs`. Identical inputs plus identical active flag set must yield identical outputs (determinism invariant).

Stability levels: Experimental (API/semantics may change), Preview (likely to stabilize), Stable (parity-proven & default on), Graduated (legacy path removed or kept only as fallback for troubleshooting).

| Flag | Area | Default | Status | Purpose | Owner | Graduation criteria | Related docs |
|------|------|---------|--------|---------|-------|----------------------|--------------|
| EnableCompiledPatterns | Movement & Patterns | ON | Graduated | Use compiled movement IR (DFA) instead of visitor; visitor retained for unsupported cases | @veggerby | Completed: full parity suite + adjacency parity, benchmarks within thresholds, docs/tests updated | movement-and-patterns.md |
| EnableBitboards | Bitboards & Accel | OFF | Experimental | Occupancy/attack bitboards for ≤64 tiles | @veggerby | Parity across modules; perf win over compiled-only paths; no desync; acceptable memory | movement-and-patterns.md, performance.md |
| EnableSlidingFastPath | Bitboards & Accel | ON | Stable | Sliding ray generation fast-path (requires bitboards when used) | @veggerby | Completed: Parity V2 + benchmarks; toggle remains for troubleshooting | movement-and-patterns.md, performance.md |
| EnableDecisionPlanGrouping | Decision Plan | OFF | Experimental | Evaluate shared predicate once per group to reduce redundant checks | @veggerby | Parity retained; measurable reduction in predicate invocations; microbench win | decision-plan-and-acceleration.md |
| EnableDecisionPlanEventFiltering | Decision Plan | OFF | Experimental | Pre-filter plan entries by EventKind before predicate evaluation | @veggerby | No false negatives; reduced predicate cost; analyzer/tests cover edge cases | decision-plan-and-acceleration.md |
| EnableDecisionPlanMasks | Decision Plan | OFF | Experimental | Skip subsequent entries in exclusivity group once one applies | @veggerby | Parity maintained across groups; measurable evaluation reduction | decision-plan-and-acceleration.md |
| EnableTurnSequencing | Turn Sequencing | ON | Stable | TurnProfile segments and sequencing events; explicit advancement | @veggerby | Completed: rotation/parity validated; docs/tests updated | turn-sequencing.md |
| EnableStateHashing | Determinism/Hashing | OFF | Experimental | Compute 64/128-bit deterministic state hashes each transition | @veggerby | Stable cross-platform values; no perf cliff; used in parity/replay tooling | determinism-rng-timeline.md |
| EnableTimelineZipper | Timeline | OFF | Experimental | Immutable zipper for undo/redo navigation of history | @veggerby | Determinism preserved; memory/time overhead acceptable; UX/docs updated | determinism-rng-timeline.md |
| EnableTraceCapture | Diagnostics | OFF | Preview | Capture evaluation traces via observer hooks | @veggerby | Low overhead; stable format; safe defaults; opt-in diagnostics | diagnostics.md |
| EnableSimulation | Simulation | OFF | Experimental | Deterministic playout/simulation APIs | @veggerby | Determinism preserved; reproducible benchmarks; safe gating in API | performance.md, diagnostics.md |
| EnableCompiledPatternsAdjacencyCache | Movement & Patterns | OFF | Experimental | Pre-built (tile,direction) adjacency cache for resolver | @veggerby | Parity with topology/boardshape; measurable resolver win | movement-and-patterns.md |
| EnableBoardShape | Topology/Layout | OFF | Experimental | O(1) neighbor lookup using BoardShape service where available | @veggerby | Parity with adjacency cache; perf gain on supported boards | movement-and-patterns.md |
| EnableBitboardIncremental | Bitboards & Accel | OFF | Experimental | Incremental bitboard + piece map updates on moves | @veggerby | Zero desync; microbench win vs rebuild; stress tests | performance.md |
| EnablePerPieceMasks | Bitboards & Accel | OFF | Experimental | Maintain per-piece occupancy masks for pruning/heuristics | @veggerby | Overhead negligible; tangible benefit in consumers | performance.md |
| EnableSegmentedBitboards | Bitboards | OFF | Experimental | Unified segmented bitboard (inline + spill) abstraction | @veggerby | Parity tests across sizes; benchmark justification; memory profile | performance.md |
| EnableTopologyPruning | Topology/Layout | OFF | Experimental | Skip directions not present for board topology to reduce branching | @veggerby | Parity for mixed topologies; measurable reduction in work | movement-and-patterns.md |
| EnableObserverBatching | Observability/Eval | OFF | Experimental | Batch high-frequency evaluation observer callbacks until terminal | @veggerby | Ordering preserved; ≤5% overhead small plans; win on large plans | diagnostics.md |

Notes and hygiene

- Configure flags deterministically at process start or via explicit test scopes; do not toggle mid-evaluation.
- In tests and benchmarks, wrap changes in a disposable scope helper and restore afterward to avoid leakage between tests. See CONTRIBUTING.md (Feature Flag Policy).
- Defaults above are authoritative and sourced from code; update this table whenever `FeatureFlags` defaults change or a flag graduates/deprecates.
