---
id: 5
slug: concurrency-and-simulation
name: "Concurrency & Simulation"
status: partial
last_updated: 2025-09-26
owner: core
flags:
  - EnableSimulation (exp)
  - EnableObserverBatching (exp)
summary: >-
  Adds deterministic playout engine (sequential + parallel) with metrics (turns, passes, replays) and cancellation; parallel path
  gated behind feature flag pending contention benchmarks and observer batching stability.
acceptance:
  - Deterministic sequential playout stable across 1k+ seeds.
  - Metrics collection (turn advancements, pass streaks, replay counts) exported.
  - Cancellation API safely aborts long simulations.
open_followups:
  - Parallel contention benchmark & threshold heuristics.
  - Observer batching enablement & parity validation.
  - Pluggable policy interface (evaluation strategy abstraction).
  - Lightweight result delta serializer (avoid full state chain for large sims).
  - Failure classification (invalid events vs structural faults) metrics.
---

# Concurrency & Simulation

## Delivered

- Sequential deterministic playout engine
- Metrics: turns, passes, replays, wall-time
- Cancellation token support (mid-playout safe exit)
- Basic policy abstraction (implicit random dice strategy)
- Seed stabilization tests (replay determinism across >1k seeds)

## Pending / Deferred

See open_followups in front matter.

## Risks

Silent observer batching divergence could skew metrics; uncontrolled parallelism risks allocator pressure.

## Next Steps

Add contention benchmarks, finalize batching parity, then graduate minimal parallel heuristic (core count <= logical processors / 2).
