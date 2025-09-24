# Bitboard128 Design Note (Draft)

> Status: Draft (no implementation). Motivation documented ahead of any code to avoid premature complexity.

## Problem Statement

Current bitboard acceleration limits boards to ≤64 tiles (single `ulong` occupancy mask). Future modules (e.g., large strategy grids or custom hex layouts) may exceed this. We need a scalable yet minimal overhead representation for 65–128 tiles preserving existing fast-path semantics.

## Design Goals

- Minimal API surface compatible with existing `BitboardSnapshot` update & query logic.
- Zero overhead for ≤64 tile boards (retain current path unchanged).
- Efficient popcount & bit scans (leverage intrinsic-friendly operations where available).
- Deterministic behavior across architectures.

## Proposed Representation

```csharp
internal readonly struct Bitboard128
{
    private readonly ulong _low; // bits 0..63
    private readonly ulong _high; // bits 64..127

    public Bitboard128(ulong low, ulong high) { _low = low; _high = high; }

    public bool IsSet(int index)
    {
        return index < 64 ? ((_low >> index) & 1UL) != 0 : ((_high >> (index - 64)) & 1UL) != 0;
    }

    public Bitboard128 Set(int index)
    {
        return index < 64
            ? new Bitboard128(_low | (1UL << index), _high)
            : new Bitboard128(_low, _high | (1UL << (index - 64)));
    }

    public Bitboard128 Clear(int index)
    {
        return index < 64
            ? new Bitboard128(_low & ~(1UL << index), _high)
            : new Bitboard128(_low, _high & ~(1UL << (index - 64)));
    }

    public int PopCount()
    {
        return System.Numerics.BitOperations.PopCount(_low) + System.Numerics.BitOperations.PopCount(_high);
    }
}
```

## Integration Strategy

1. Introduce an abstraction `IBitboardOps` with methods: `IsSet`, `Set`, `Clear`, `PopCount`.
2. Provide two implementations: `Bitboard64Ops` (wrapper around `ulong`) and `Bitboard128Ops` (wrapper around `Bitboard128`).
3. Extend `BitboardLayout` build step: if tileCount ≤64 use existing path; else if tileCount ≤128 switch to 128-bit path; else skip bitboard acceleration entirely (future extension could use segmented array of ulongs).
4. Update `BitboardSnapshot` to carry either representation via a discriminated struct or internal tagged union.
5. Sliding attack generator remains unchanged (works with tile indices). Fast-path occupancy checks branch on representation to test blockers.

## Parity & Testing Plan

- Unit tests constructing synthetic boards of sizes: 65, 80, 96, 128 verifying occupancy set/clear/popcount and fast-path path resolution parity with compiled resolver.
- Property tests randomizing piece placements; assert that legacy + compiled + fast-path all agree for 500 generated scenarios per size bucket.
- Benchmark variant comparing 64 vs 128 tile boards to quantify added overhead (goal <15% slowdown vs 64-bit path on comparable density).

## Risks

Risk | Mitigation
---- | ---------
Increased branching in hot loops | Inline representation tag & rely on JIT devirtualization.
Allocation churn if using boxed union | Use readonly struct + explicit fields; no heap allocations.
Complexity creep for >128 tile support | Defer; gather concrete use cases first.

## Open Questions

- Should we support sparse >128 boards via chunked bitsets early? (Deferred – no concrete module demand.)
- Introduce vectorized popcount for large batches? (Premature; wait for profiling.)

## Next Steps

1. Validate necessity (identify a target >64-tile module with sliding-heavy movement patterns).
2. Implement `Bitboard128` + ops + snapshot branching behind new flag `EnableBitboards128` (or extend existing `EnableBitboards`).
3. Add benchmarks & parity tests.
4. Assess maintenance burden before graduating flag.

---

Last updated: 2025-09-24
