# Bitboard128 Design Note (Draft)

## Purpose

Support occupancy + sliding attack acceleration on boards with >64 and ≤128 tiles without altering external semantics or public APIs. Extend current 64-bit bitboard fast-path determinism and performance characteristics.

## Constraints

- Maintain existing fast-path contract: geometric reachability + path reconstruction + post-filter occupancy semantics identical to compiled/legacy.
- Avoid generic overhead for ≤64 boards (retain existing `ulong` path).
- Zero public API changes; internal services only.
- Deterministic across platforms (no reliance on intrinsics beyond standard X64 bit ops; fallback loops acceptable for other architectures later).

## Representation

```text
struct Bitboard128 {
    ulong Low;  // bits 0..63
    ulong High; // bits 64..127
}
```

Helper ops (static inline methods):

- Set(index)
- Clear(index)
- IsSet(index)
- Any()
- Or/And/Xor
- PopCount() (sum of System.Numerics.BitOperations.PopCount for each segment)

Indices ≥128 are unsupported (fast-path disabled gracefully if board tile count >128).

## Layout & Services

`BitboardLayout` emits either `Bitboard64` or `Bitboard128` based on tile count. Internal enum `BitboardKind { Bits64, Bits128 }` governs the update path. `BitboardServices` carries the kind plus union-like storage. Downstream fast-path selects a specialized branch without virtual dispatch.

## Update Path

On move: clear source index, set destination index in global & per-player masks. Branchless approach (mask selection via ternary) retained. Separate methods for 64 vs 128 keep JIT inlining efficient and avoid added branching for ≤64 boards.

## Sliding Attack Generation

Reuse existing directional ray precomputation (tile -> array of ray indices). For 128 variant, occupancy test becomes:

```csharp
bool blocked = (rayMaskLow & occ.Low) != 0 || (rayMaskHigh & occ.High) != 0;
```

Direction stepping loop remains unchanged; we still walk neighbor indices using `BoardShape` until the first blocker.

## Fallback / Guard

If board tile count >128: skip registering bitboard + sliding attack services; `EnableBitboards` effectively ignored (metrics: SkipNoServices). A forthcoming test will assert no exception and that fast-path attempts fall back to compiled resolver.

## Risks

| Risk | Mitigation |
|------|------------|
| Extra branching harms ≤64 perf | Completely separate code paths for 64 vs 128; JIT eliminates dead branches. |
| Increased complexity for marginal modules | Defer implementation until a >64 tile module (e.g., large hex or custom grid) appears. |
| PopCount overhead on high-density boards | Only used in future heuristics (mobility); not required for path resolution. |

## Future Extensions

- Generic masking interface `IBitboardOps` (not introduced yet to avoid indirection overhead).
- SIMD-assisted scan for first blocker (benchmark before adding).

## Acceptance Criteria (When Implemented)

- Parity suite green for representative >64 board test module.
- Benchmark: ≤10% regression vs 64-bit version on ≤64 boards; measurable speedup vs no bitboards on 96–128 tile board.
- FastPathMetrics shows non-zero hits on >64 board scenarios.

## Status

Design only; not yet implemented. Tracking item added to backlog.

Last updated: 2025-09-24
