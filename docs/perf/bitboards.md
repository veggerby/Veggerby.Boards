# Bitboard & Sliding Fast-Path Performance Note

This document describes the methodology for evaluating the experimental bitboard + sliding attack generator acceleration layer and the associated fast-path integrated into `GameProgress.ResolvePathCompiledFirst`.

## Goals

- Accelerate sliding piece path resolution (rook, bishop, queen archetypes) beyond compiled pattern baseline by pruning invalid directions early and avoiding full pattern IR traversal.
- Provide occupancy-aware attack generation (blocker / capture semantics) in O(direction count + blockers encountered) time.
- Maintain deterministic parity with compiled resolver + occupancy post-filter across all tested scenarios.

## Architecture Summary

Component | Responsibility | Notes
--------- | -------------- | -----
`BoardShape` | Tile index mapping + directional neighbor arrays | Built unconditionally; flag governs exploitation.
`PieceMapSnapshot` | SoA piece ownership + tile index array (incremental updates) | Supplies owner lookup without scanning states.
`BitboardSnapshot` | Global + per-player occupancy bitmasks (≤64 tiles) | Updated incrementally on move events.
`SlidingAttackGenerator` | Precomputed directional ray offsets per tile + runtime blocker truncation | Emits attack set as indices.
Fast-Path Resolver | Combines above to (a) check if target index in sliding attacks, (b) reconstruct linear path via `BoardShape` neighbors | Only for pieces with at least one repeatable directional pattern.

## Invocation Path

1. `ResolvePathCompiledFirst` called with `(piece, from, to)`.
2. If fast-path prerequisites present (`EnableBitboards` true, services present, piece has sliding pattern):
   - Increment `FastPathMetrics.Attempts`.
   - Generate sliding attacks for `from`.
   - If `to` index contained: reconstruct path by iterating directional neighbors; on success increment `FastPathMetrics.FastPathHits` and return.
   - Otherwise continue to compiled resolver.
3. If prerequisites missing or guard fails: increment `FastPathMetrics.FastPathSkippedNoPrereq`.
4. Compiled resolver attempt; if success increment `CompiledHits` else fallback to legacy visitor (`LegacyHits`).
5. Apply occupancy semantics filter (blocker traversal + friendly capture rejection) to compiled/legacy path.

## Metrics

Internal counters (see `FastPathMetrics`):

- Attempts: total path resolution calls via progress aware API.
- FastPathHits: paths resolved entirely by sliding fast-path.
- FastPathSkippedNoPrereq: prerequisites absent (services/flags) or immobile piece guard fired.
- CompiledHits: compiled IR produced a path (post fast-path miss/skip).
- LegacyHits: legacy visitor produced a path (compiled miss or feature disabled).

Use `FastPathMetrics.Snapshot()` between benchmark iterations. Always call `FastPathMetrics.Reset()` before a measurement batch.

## Benchmarking Procedure

Benchmark harness (`SlidingPathResolutionBenchmark`) now exposes five resolution modes:

Method | Description | Feature Flags | Purpose
------ | ----------- | ------------- | -------
`LegacyVisitor` | Original pattern visitor per pattern traversal | (all off) | Historical baseline & regression guard
`Compiled` | Pure compiled resolver (no bitboards) | `EnableCompiledPatterns` | Baseline for IR benefits only
`CompiledNoBitboards` | Alias of pure compiled (separated for clarity in results table) | `EnableCompiledPatterns` | Explicit comparator vs bitboards variants
`CompiledWithBitboardsNoFastPath` | Compiled + bitboards acceleration, sliding fast-path disabled | `EnableCompiledPatterns`, `EnableBitboards` | Isolate cost/benefit of bitboard occupancy snapshot alone
`FastPath` | Full stack: bitboards + sliding fast-path + compiled fallback | `EnableCompiledPatterns`, `EnableBitboards`, `EnableSlidingFastPath` | Measures end-to-end optimized path selection

Procedure:

1. Run benchmark across densities: `empty`, `quarter`, `half` (deterministic blocker seed).
2. Record: Mean, p95, Alloc B/op for each method.
3. Collect `FastPathMetrics` snapshot after `FastPath` run to compute: hit ratio (`FastPathHits/Attempts`), skip distribution.
4. Derive relative speedups vs `Compiled` and vs `LegacyVisitor` (empty board primary headline).

Interpretation Template:

Density | LegacyVisitor (ops/s) | Compiled | Compiled+BB (no fast-path) | FastPath | FastPath Hit Ratio | Speedup vs Compiled | Speedup vs Legacy
------- | --------------------- | -------- | -------------------------- | -------- | ------------------ | ------------------- | -----------------
empty   | ~1,641               | ~1,222   | ~1,878                     | ~5,705   | ~100%              | 4.66×               | 3.48×
quarter | ~1,556               | ~1,214   | ~2,020                     | ~3,014   | ~100%              | 2.49×               | 1.94×
half    | ~1,627               | ~1,233   | ~1,939                     | ~1,943   | ~100%              | 1.59×               | 1.20×

Populate the table, then update CHANGELOG and action plan if headline targets (see Acceptance Thresholds) are met or exceeded.

See also: movement semantics charter (`../movement-semantics.md`) for the authoritative definition of sliding occupancy rules used in parity and benchmark validation.

## Acceptance Thresholds (Draft)

Scenario | Target Win vs Compiled | Rationale
-------- | ----------------------- | ---------
Empty board | ≥2.0× | Maximize benefit when rays unblocked (best case for pruning).
Quarter density | ≥1.5× | Moderate blocker presence; expect some early termination.
Half density | ≥1.2× | Heavy blockers reduce difference; still aim for net gain.

If thresholds unmet after two optimization attempts, reassess algorithmic overhead (e.g., path reconstruction loop, attack generation allocation) before proceeding to more complex strategies.

## Pending Work

- Blocker & capture parity test expansion (already partially landed; extend to randomized placements).
- Integration of metrics output into benchmark logs (custom exporter or encoded in result column).
- Bitboard128 / dual-mask design for >64 tile boards.
- Mobility heuristic prototype leveraging attack sets directly.
- Board topology specialization (orthogonal-only vs orthogonal+diagonal) to skip unused directional groups.

## Design Notes (Planned Bitboard128 Strategy)

For boards >64 tiles:

- Introduce `Bitboard128` struct wrapping two `ulong` segments (low/high) with helper methods (`Set`, `Clear`, `IsSet`, `PopCount`).
- Provide unified interface consumed by `SlidingAttackGenerator` (generic via `IBitboardOps`).
- Fallback to existing 64-bit path when tile count ≤64 (avoid overhead).
- Guard fast-path by verifying board tile count fits selected representation at build time; otherwise skip bitboard acceleration.

## Risks

Risk | Impact | Mitigation
---- | ------ | ---------
Incorrect occupancy semantics in fast-path | Silent illegal moves | Post-filter parity tests & immobile guard
High allocation overhead (attack sets) | Offsets perf gains | Preallocate per-tile directional ray arrays, reuse stack spans
Bitboard128 complexity w/ marginal benefit | Maintenance burden | Defer until a concrete >64 tile module justifies

## FAQ

Q: Why reconstruct the path after confirming attack membership?
A: Attack generation produces reachable target indices but not the ordered relation sequence. Reconstructing via `BoardShape` ensures correct intermediate tiles and leverages existing `TilePath` structure without duplicating builder logic.

Q: Are captures handled directly in fast-path?
A: The fast-path only confirms geometric reachability; final occupancy validation (including capture legality) is applied uniformly in `ApplyOccupancySemantics` for consistency.

---

### Results Interpretation

Raw benchmark means (lower is better) were converted to approximate ops/s via 1 / (MeanSeconds). FastPath substantially outperforms both legacy visitor and compiled resolver on empty and moderately occupied boards, meeting or exceeding all draft acceptance thresholds:

- Empty board: 4.66× faster than compiled (target ≥2.0×) and 3.48× faster than legacy.
- Quarter density: 2.49× faster than compiled (target ≥1.5×) and 1.94× faster than legacy.
- Half density: 1.59× faster than compiled (target ≥1.2×) and still 20% faster than legacy. Here FastPath and Compiled+BB (no fast-path) converge, indicating blocker-heavy scenarios reduce pure sliding pruning benefit; residual win comes from avoiding legacy visitor overhead and leveraging bitboard occupancy.

FastPath hit ratio was effectively 100% for benchmarked queries (all rays suitable for sliding resolution). In mixed piece workloads we expect a lower ratio proportional to non-sliding piece share; speedups should scale accordingly but remain positive given negligible skip cost.

Memory: FastPath reduced allocations drastically on empty (≈75% reduction vs legacy) due to early pruning and lean reconstruction. Under higher densities allocation gap narrows (additional occupancy bookkeeping) but remains favorable or neutral.

Outlier & Warning Notes: BenchmarkDotNet warned that per-iteration times are <1ms; batching multiple query resolutions per invocation could further stabilize distributions, but current variance (≤~2%) is acceptable for relative comparisons.

Decision: All thresholds satisfied. Recommendation: enable `EnableSlidingFastPath` by default in a follow-up change after one more parity soak (CI) and integrate metrics exposure (hit ratio) into optional diagnostics.

Last updated: 2025-09-24 (populated benchmark results & analysis)
