# Performance & Benchmarks

Performance work follows “parity first, optimization second”. Benchmarks ensure accelerations do not regress correctness or inflate allocations.

## Benchmark Categories

| Benchmark | Focus |
|-----------|-------|
| PatternResolutionBenchmark | Legacy vs compiled pattern traversal |
| SlidingPathResolutionBenchmark | Bitboards & fast-path efficacy |
| SegmentedBitboardSnapshotBenchmark | Snapshot build cost (flag on/off, 64 vs 128 tiles) |
| SegmentedBitboardMicroBenchmark | Primitive ops (Test/Set/Clear/PopCount) vs ulong & Bitboard128 |
| SlidingAttackGeneratorBenchmark | Ray precomputation cost across topologies (ring vs grid) |
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
* Path resolution/access: `TilePath` caches relations, tiles, directions, and total distance (no LINQ in accessors) to eliminate transient allocations during rule/path evaluation.
* Builder hot path: artifact assembly uses explicit loops instead of chained LINQ projections to cut transient arrays/lists (single-pass population into `allArtifacts`).
* Extras retrieval: replaced reflective generic `ExtrasState<T>` with a non-generic wrapper and typed registry to eliminate reflection & generic instantiation overhead in hashing and state diffing.
* Canonical hashing: serializer now includes cycle detection (reference-equality set) + depth cap (32) and artifact/type fast-path tags to prevent stack overflows and expensive reflection walks. These safeguards are deterministic (same graph → same emitted tag sequence) and have negligible overhead (< O(n) visited set lookups) compared to previous catastrophic recursion.
* Fast-path parity: any acceleration (compiled patterns, sliding rays) must not alter blocker/capture semantics; benchmarks coupled with parity tests guard regressions.

## Recent Benchmark Additions

| Benchmark | Focus |
|-----------|-------|
| BitboardLayoutOrderingBenchmark | Compares legacy LINQ OrderBy vs current insertion sort ordering for bitboard tile layout across board sizes (8..64). |
| SlidingAttackGeneratorCapacityBenchmark | Evaluates heuristic ray buffer pre-allocation vs baseline dynamic growth for large ring and 8x8 grid topologies. |

### Pending Measurements

* Sliding attack ray allocation optimization (buffer + visited boolean array) requires benchmark delta capture against prior baseline to validate allocation reduction (expect drop in Gen0 and B/op). Integrate into existing SlidingAttackGeneratorCapacityBenchmark with feature flag or version toggle for A/B.

These scaffolds provide empirical data to justify or reject further micro-optimizations (e.g., stackalloc rays, span-based emission) and verify current strategies remain allocation‑efficient at scale.
