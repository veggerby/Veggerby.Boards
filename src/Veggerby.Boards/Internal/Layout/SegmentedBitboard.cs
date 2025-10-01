using System;
using System.Numerics;

namespace Veggerby.Boards.Internal.Layout;

/// <summary>
/// Immutable segmented bitboard supporting arbitrary tile counts by partitioning occupancy bits into 64-bit segments.
/// Provides an inline fast-path for up to four segments (&lt;=256 tiles) and spills to a backing array beyond that.
/// </summary>
/// <remarks>
/// Design goals:
/// 1. Preserve the branchless hot path for &lt;=64 tiles (<see cref="IsSingle"/>) via direct field access.
/// 2. Avoid heap allocation for common board sizes (e.g. Chess 64, Checkers 64, Backgammon &lt;64, most games &lt;256).
/// 3. Support larger experimental boards (e.g. 361 Go, synthetic stress) with a single representation to unify logic.
/// 4. Remain purely immutable – all mutating operations return a new <see cref="SegmentedBitboard"/> instance.
/// </remarks>
internal readonly struct SegmentedBitboard
{
    private const int InlineSegmentCapacity = 4; // 4 * 64 = 256 tiles inline before spilling.

    private readonly ulong _s0;
    private readonly ulong _s1;
    private readonly ulong _s2;
    private readonly ulong _s3;
    private readonly ulong[] _spill; // length = SegmentCount - InlineSegmentCapacity when SegmentCount > InlineSegmentCapacity; may be null when segment count <= inline capacity

    /// <summary>
    /// Gets the number of 64-bit segments represented.
    /// </summary>
    public int SegmentCount { get; }

    /// <summary>
    /// Gets a value indicating whether this bitboard contains exactly one 64-bit segment (&lt;=64 tiles).
    /// </summary>
    public bool IsSingle => SegmentCount == 1;

    /// <summary>
    /// Gets the low 64 bits (segment 0). Valid regardless of <see cref="SegmentCount"/>.
    /// </summary>
    public ulong Low64 => _s0;

    /// <summary>
    /// Returns an empty bitboard (zero segments => treated as single empty segment).
    /// </summary>
    public static SegmentedBitboard Empty { get; } = new(1, 0UL, 0UL, 0UL, 0UL, null);

    private SegmentedBitboard(int segmentCount, ulong s0, ulong s1, ulong s2, ulong s3, ulong[] spill)
    {
        if (segmentCount <= 0)
        {
            segmentCount = 1;
        }

        SegmentCount = segmentCount;
        _s0 = s0;
        _s1 = s1;
        _s2 = s2;
        _s3 = s3;
        _spill = spill;
    }

    /// <summary>
    /// Builds a <see cref="SegmentedBitboard"/> from raw segment data. The input array is not retained; its contents are copied into inline fields and spill if required.
    /// </summary>
    /// <param name="segments">Raw segments (length &gt;= 1).</param>
    /// <returns>New bitboard.</returns>
    public static SegmentedBitboard FromSegments(ReadOnlySpan<ulong> segments)
    {
        if (segments.Length == 0)
        {
            return Empty;
        }

        ulong s0 = segments[0];
        ulong s1 = segments.Length > 1 ? segments[1] : 0UL;
        ulong s2 = segments.Length > 2 ? segments[2] : 0UL;
        ulong s3 = segments.Length > 3 ? segments[3] : 0UL;

        ulong[] spill = null;
        if (segments.Length > InlineSegmentCapacity)
        {
            var spillLength = segments.Length - InlineSegmentCapacity;
            spill = new ulong[spillLength];
            for (int i = 0; i < spillLength; i++)
            {
                spill[i] = segments[InlineSegmentCapacity + i];
            }
        }

        return new SegmentedBitboard(segments.Length, s0, s1, s2, s3, spill);
    }

    /// <summary>
    /// Tests whether the bit at <paramref name="tileIndex"/> is set.
    /// </summary>
    public bool Test(int tileIndex)
    {
        if (tileIndex < 0)
        {
            return false;
        }
        int segment = tileIndex >> 6; // divide by 64
        if (segment >= SegmentCount)
        {
            return false;
        }
        int offset = tileIndex & 63;
        ulong mask = 1UL << offset;
        return (GetSegment(segment) & mask) != 0;
    }

    /// <summary>
    /// Returns a new bitboard with the bit at <paramref name="tileIndex"/> set.
    /// </summary>
    public SegmentedBitboard WithSet(int tileIndex)
    {
        if (tileIndex < 0)
        {
            return this;
        }
        int segment = tileIndex >> 6;
        int offset = tileIndex & 63;
        ulong mask = 1UL << offset;
        return MutateSegment(segment, s => s | mask);
    }

    /// <summary>
    /// Returns a new bitboard with the bit at <paramref name="tileIndex"/> cleared.
    /// </summary>
    public SegmentedBitboard WithCleared(int tileIndex)
    {
        if (tileIndex < 0)
        {
            return this;
        }
        int segment = tileIndex >> 6;
        int offset = tileIndex & 63;
        ulong mask = ~(1UL << offset);
        return MutateSegment(segment, s => s & mask);
    }

    /// <summary>
    /// Gets a value indicating whether any bit is set (occupancy &gt; 0).
    /// </summary>
    public bool Any
    {
        get
        {
            if ((_s0 | _s1 | _s2 | _s3) != 0UL)
            {
                return true;
            }
            if (_spill is null)
            {
                return false;
            }
            for (int i = 0; i < _spill.Length; i++)
            {
                if (_spill[i] != 0UL)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether no bits are set.
    /// </summary>
    public bool None => !Any;

    /// <summary>
    /// Counts the total number of set bits across all segments.
    /// </summary>
    public int PopCount()
    {
        int count = 0;
        count += BitOperations.PopCount(_s0);
        if (SegmentCount > 1)
        {
            count += BitOperations.PopCount(_s1);
        }
        if (SegmentCount > 2)
        {
            count += BitOperations.PopCount(_s2);
        }
        if (SegmentCount > 3)
        {
            count += BitOperations.PopCount(_s3);
        }
        if (_spill is not null)
        {
            for (int i = 0; i < _spill.Length; i++)
            {
                count += BitOperations.PopCount(_spill[i]);
            }
        }
        return count;
    }

    private SegmentedBitboard MutateSegment(int segmentIndex, Func<ulong, ulong> mutator)
    {
        if (segmentIndex < 0 || segmentIndex >= SegmentCount)
        {
            return this; // out of range - treat as no-op
        }

        // Inline fast path: mutate local copies then construct new instance.
        ulong s0 = _s0;
        ulong s1 = _s1;
        ulong s2 = _s2;
        ulong s3 = _s3;
        ulong[] spill = _spill;

        switch (segmentIndex)
        {
            case 0:
                s0 = mutator(s0);
                break;
            case 1:
                s1 = mutator(s1);
                break;
            case 2:
                s2 = mutator(s2);
                break;
            case 3:
                s3 = mutator(s3);
                break;
            default:
                if (spill is null)
                {
                    return this; // defensive; should not happen if segmentIndex in range
                }
                int spillIdx = segmentIndex - InlineSegmentCapacity;
                var clone = new ulong[spill.Length];
                // copy spill (immutability)
                for (int i = 0; i < spill.Length; i++)
                {
                    clone[i] = spill[i];
                }
                clone[spillIdx] = mutator(clone[spillIdx]);
                spill = clone;
                break;
        }

        return new SegmentedBitboard(SegmentCount, s0, s1, s2, s3, spill);
    }

    private ulong GetSegment(int segmentIndex)
    {
        return segmentIndex switch
        {
            0 => _s0,
            1 => _s1,
            2 => _s2,
            3 => _s3,
            _ => _spill is null ? 0UL : _spill[segmentIndex - InlineSegmentCapacity]
        };
    }
}