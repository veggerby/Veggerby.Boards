using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Internal;
using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Micro-benchmarks for core <see cref="SegmentedBitboard"/> operations compared to native 64-bit and Bitboard128 usage.
/// Focus: Test (bit lookup), WithSet, WithCleared, PopCount across 64 and 128 tile densities.
/// </summary>
[MemoryDiagnoser]
public class SegmentedBitboardMicroBenchmark
{
    [Params(32, 64, 96, 128)]
    public int SetBits
    {
        get; set;
    }

    private SegmentedBitboard _seg64;
    private SegmentedBitboard _seg128;
    private ulong _u64;
    private Bitboard128 _bb128;
    private int[] _indices = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new System.Random(12345);
        _indices = Enumerable.Range(0, SetBits).Select(_ => rnd.Next(0, 128)).Distinct().ToArray();
        // Build 64-bit baseline mask for indices <64
        ulong low = 0UL;
        ulong high = 0UL;
        foreach (var i in _indices)
        {
            if (i < 64)
            {
                low |= 1UL << i;
            }
            else
            {
                high |= 1UL << (i - 64);
            }
        }
        _u64 = low;
        _bb128 = new Bitboard128(low, high);
        _seg64 = SegmentedBitboard.FromSegments(stackalloc ulong[] { low });
        _seg128 = high == 0UL ? SegmentedBitboard.FromSegments(stackalloc ulong[] { low }) : SegmentedBitboard.FromSegments(stackalloc ulong[] { low, high });
    }

    [Benchmark(Baseline = true)]
    public int Test_Ulong()
    {
        int hits = 0;
        foreach (var i in _indices)
        {
            if (i < 64 && ((_u64 >> i) & 1UL) != 0)
            {
                hits++;
            }
        }
        return hits;
    }

    [Benchmark]
    public int Test_Segmented()
    {
        int hits = 0;
        foreach (var i in _indices)
        {
            if (_seg128.Test(i))
            {
                hits++;
            }
        }
        return hits;
    }

    [Benchmark]
    public int Test_Bitboard128()
    {
        int hits = 0;
        foreach (var i in _indices)
        {
            if (i < 64)
            {
                if (((_bb128.Low >> i) & 1UL) != 0)
                {
                    hits++;
                }
            }
            else
            {
                if (((_bb128.High >> (i - 64)) & 1UL) != 0)
                {
                    hits++;
                }
            }
        }
        return hits;
    }

    [Benchmark]
    public int Mutate_Set_Clear_Roundtrip()
    {
        var seg = _seg128;
        foreach (var i in _indices)
        {
            seg = seg.WithCleared(i);
            seg = seg.WithSet(i);
        }
        return seg.PopCount();
    }

    [Benchmark]
    public int PopCount_Segmented()
    {
        return _seg128.PopCount();
    }

    [Benchmark]
    public int PopCount_Bitboard128()
    {
        return System.Numerics.BitOperations.PopCount(_bb128.Low) + System.Numerics.BitOperations.PopCount(_bb128.High);
    }
}