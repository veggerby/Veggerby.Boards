using System;

namespace Veggerby.Boards.Tests;

/// <summary>
/// Lightweight deterministic pseudo-random number generator for tests replacing System.Random.
/// Uses a simple 32-bit LCG (Numerical Recipes constants). Stable across runtimes and platforms.
/// NOT cryptographic; ONLY for deterministic test data variation.
/// </summary>
internal struct TestDeterministicRng
{
    private uint _state;

    public TestDeterministicRng(int seed)
    {
        _state = seed == 0 ? 0xA341316Cu : (uint)seed;
    }

    private uint NextUInt()
    {
        // LCG: state = state * 1664525 + 1013904223  (mod 2^32)
        _state = unchecked(_state * 1664525u + 1013904223u);
        return _state;
    }

    public int Next(int maxExclusive)
    {
        if (maxExclusive <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExclusive));
        }
        return (int)(NextUInt() % (uint)maxExclusive);
    }

    public int Next(int minInclusive, int maxExclusive)
    {
        if (minInclusive >= maxExclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be > minInclusive");
        }
        return minInclusive + Next(maxExclusive - minInclusive);
    }
}