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

1. Ensure benchmark harness includes three methods: `LegacyVisitor`, `Compiled`, `FastPath` (see `SlidingPathResolutionBenchmark`).
2. Run with densities: `empty`, `quarter`, `half` (blocker distribution seeded for reproducibility).
3. Record mean & p95 for each method.
4. Capture metrics snapshot after running fast-path benchmark to compute hit ratio: `FastPathHits / Attempts`.

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

Last updated: 2025-09-24
