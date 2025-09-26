# Performance & Benchmarks

Performance work follows “parity first, optimization second”. Benchmarks ensure accelerations do not regress correctness or inflate allocations.

## Benchmark Categories

| Benchmark | Focus |
|-----------|-------|
| PatternResolutionBenchmark | Legacy vs compiled pattern traversal |
| SlidingPathResolutionBenchmark | Bitboards & fast-path efficacy |
| CompiledPatternKindsBenchmark | Pattern kind latency & allocation profile |
| TurnSequencingOverheadBenchmark | Sequencing flag overhead |
| EventKindFilteringBenchmark | DecisionPlan event filtering gains |

## Metrics

Track: Mean, p95, Alloc B/op, Gen0 collections (where relevant), hit ratios (fast-path). Internal metrics objects (e.g., FastPathMetrics) snapshot per iteration.

## Acceptance Threshold Examples

* Sliding fast-path: ≥2× compiled baseline (empty board) & ≥1.2× at high blocker density.
* Turn sequencing overhead: <3% p50 latency delta, no extra allocations.

## Methodology

1. Reset metrics before each run.
2. Run benchmark harness under Release.
3. Compare against previous JSON baseline (planned regression gate).

## Guidelines

* No LINQ in hot loops.
* Avoid per-call allocations (reuse or stack allocate where safe).
* Add a semantics charter before introducing new acceleration layers.
