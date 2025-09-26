# Feature Flags

Each experimental / optimization subsystem is gated. Flags default OFF unless proven parity + performance.

| Flag | Purpose | Stability | Related Docs |
|------|---------|----------|--------------|
| EnableCompiledPatterns | Use compiled movement IR | Stable (Graduated) | movement-and-patterns.md |
| EnableBitboards | Occupancy & attack bitboards | Experimental | movement-and-patterns.md, performance.md |
| EnableSlidingFastPath | Sliding ray fast-path | Experimental | movement-and-patterns.md, performance.md |
| EnableDecisionPlanGrouping | Predicate grouping in rule plan | Experimental | decision-plan-and-acceleration.md |
| EnableDecisionPlanEventFiltering | Coarse EventKind filtering | Experimental | decision-plan-and-acceleration.md |
| EnableDecisionPlanMasks | Exclusivity masking | Experimental | decision-plan-and-acceleration.md |
| EnableTurnSequencing | TurnState & sequencing events | Experimental | turn-sequencing.md |
| EnableStateHashing | 64/128-bit state hashing | Experimental | determinism-rng-timeline.md |
| EnableTimelineZipper | GameTimeline undo/redo | Experimental | determinism-rng-timeline.md |
| EnableTraceCapture | Evaluation trace capture | Preview | diagnostics.md |
| EnableSimulation | Simulation / playout APIs | Experimental | performance.md (future), diagnostics.md |

Stability levels: Experimental (API / semantics may change), Preview (likely to stabilize), Stable (parity-proven & default on), Graduated (legacy path removed).
