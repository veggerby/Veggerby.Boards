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

## Acceleration Layer Architecture

The engine employs a layered acceleration architecture for performance-critical operations:

### Layer Overview

1. **Base Layer**: Compiled patterns (default ON) provide deterministic fallback for all movement resolution
2. **Acceleration Layer**: Bitboards + sliding fast-path for performance-critical scenarios (both default ON)
3. **Optimization Layer**: Per-piece masks and heuristic pruning (experimental, future enhancements)
4. **Scale Layer**: Bitboard128 support for boards up to 128 tiles (scaffolded)

### Bitboard Acceleration (Graduated)

**Status**: Default enabled (`EnableBitboards = ON`, `EnableBitboardIncremental = ON`)

Bitboard acceleration provides O(1) occupancy checks and efficient attack set computation for boards with ≤64 tiles. Key components:

* **BoardShape**: Tile index mapping + directional neighbor arrays for O(1) adjacency lookup
* **PieceMapSnapshot**: Piece ownership tracking with incremental updates on move events
* **BitboardSnapshot**: Global + per-player occupancy bitmasks updated incrementally
* **SlidingAttackGenerator**: Precomputed directional rays + blocker truncation for attack sets

**Performance**: 4.66× speedup demonstrated on empty board scenarios; allocation-free fast-path hits validated.

### Incremental Updates (Graduated)

**Status**: Default enabled (`EnableBitboardIncremental = ON`)

The incremental update path maintains bitboard and piece map snapshots across state transitions without full rebuilds:

* **Move Event Optimization**: On `MovePieceGameEvent`, bitboards are updated incrementally by clearing the source tile and setting the destination tile
* **Capture Handling**: Automatic occupancy adjustment when pieces are captured during moves
* **Fallback Safety**: Falls back to full rebuild for non-move events or when incremental update preconditions fail
* **Zero Desync**: Validated via comprehensive multi-module soak tests (10,000+ moves per module across Chess, Backgammon, Go)

**Semantics**: Incremental updates preserve exact parity with full rebuild. The acceleration context (`BitboardAccelerationContext`) manages snapshot lifecycle and delegates to appropriate update strategy based on event type.

**Validation**: Multi-module randomized soak tests ensure deterministic parity between incremental and rebuild paths across diverse game progressions.

### Bitboard128 Support (Scaffolded)

For boards with 65-128 tiles, the engine uses a two-segment `Bitboard128` structure:

* **Segmented Design**: Two 64-bit segments managed inline
* **Automatic Selection**: Layout registration automatically selects Bitboard128 when tile count exceeds 64
* **Parity Validated**: Comprehensive tests ensure correct behavior across segment boundaries
* **Performance**: Overhead remains acceptable; primarily allocation-free operations

**Constraints**: Bitboard128 is an internal optimization; external semantics remain unchanged. Boards >128 tiles fall back to non-bitboard paths.

## Recent Benchmark Additions

| Benchmark | Focus |
|-----------|-------|
| BitboardLayoutOrderingBenchmark | Compares legacy LINQ OrderBy vs current insertion sort ordering for bitboard tile layout across board sizes (8..64). |
| SlidingAttackGeneratorCapacityBenchmark | Evaluates heuristic ray buffer pre-allocation vs baseline dynamic growth for large ring and 8x8 grid topologies. |
| BitboardIncrementalBenchmark | Measures incremental bitboard update overhead vs full rebuild on representative piece moves. |

## Automated Report Generation

Benchmark execution & reporting:

* Script (`scripts/run-all-benchmarks.sh`): convenience wrapper that builds and runs all benchmarks (optionally filtered). It only produces BenchmarkDotNet artifact outputs.
* Harness (`benchmarks/Program.cs`): invoked directly with `--generate-report` (and optional `--report-out`) to build the consolidated markdown performance report.

Script capabilities (execution only):

* Dynamic discovery (no maintenance when adding new benchmark classes).
* Filtering and job configuration via environment variables: `BENCH_FILTER`, `BENCH_JOB`.
* (No reuse/report generation parameters; BENCH_REUSE and BENCH_OUT removed.)

Harness features:

* Emits distilled summary + glossary + raw tables using the internal `MarkdownBuilder`.
* Custom destination via `--report-out <pathOrDirectory>` (directory auto-appends `benchmark-results.md`).
* Integrates cleanly with BenchmarkDotNet’s standard `--filter` semantics.

The canonical published file remains [`benchmark-results.md`](benchmark-results.md) under `docs` unless overridden.

### Usage

Run full suite (Release configuration):

```bash
./scripts/run-all-benchmarks.sh
```

Short job variant (faster iteration):

```bash
BENCH_JOB="--job short" ./scripts/run-all-benchmarks.sh
```

Filter to a single benchmark:

```bash
BENCH_FILTER="*SlidingAttackGeneratorCapacityBenchmark*" ./scripts/run-all-benchmarks.sh

Generate consolidated markdown (harness invocation):

```bash
dotnet run -c Release --project benchmarks/Veggerby.Boards.Benchmarks.csproj -- --generate-report --report-out ./perf-out
```

Harness-based generation (direct .NET run):

```bash
dotnet run -c Release --project benchmarks/Veggerby.Boards.Benchmarks.csproj -- --filter '*BitboardIncrementalBenchmark*' --generate-report --report-out ./perf-out
```

Filter to a single benchmark and write to a custom file (using short aliases):

```bash
dotnet run -c Release --project benchmarks/Veggerby.Boards.Benchmarks.csproj -- --filter '*BitboardIncrementalBenchmark*' -g -o ./benchmarks/docs/bitboard-incremental.md
```

Or default docs path:

```bash
dotnet run -c Release --project benchmarks/Veggerby.Boards.Benchmarks.csproj -- --generate-report

Aliases:

- `-g` equals `--generate-report`
- `-o` equals `--report-out`
```

The generated report (harness only) is intended for publication with NuGet packages as a transparency artifact (performance characteristics & allocation profiles). Include the latest run when preparing release notes.

### Pending Measurements

* Sliding attack ray allocation optimization (buffer + visited boolean array) requires benchmark delta capture against prior baseline to validate allocation reduction (expect drop in Gen0 and B/op). Integrate into existing SlidingAttackGeneratorCapacityBenchmark with feature flag or version toggle for A/B.

These scaffolds provide empirical data to justify or reject further micro-optimizations (e.g., stackalloc rays, span-based emission) and verify current strategies remain allocation‑efficient at scale.
