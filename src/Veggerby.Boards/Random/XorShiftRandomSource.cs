using System;

namespace Veggerby.Boards.Random;

/// <summary>
/// Minimal XorShift* (64-bit) deterministic PRNG implementation for engine reproducibility.
/// </summary>
/// <remarks>
/// Not cryptographically secure. Selected for speed + small state. State cloning supports replay.
/// </remarks>
public sealed class XorShiftRandomSource : IRandomSource
{
    private ulong _state;

    /// <inheritdoc />
    public ulong Seed { get; }

    private XorShiftRandomSource(ulong state, ulong seed)
    {
        _state = state;
        Seed = seed;
    }

    /// <summary>
    /// Creates a new generator from a 64-bit seed (zero seed is remapped to a non-zero constant).
    /// </summary>
    public static XorShiftRandomSource Create(ulong seed)
    {
        if (seed == 0)
        {
            seed = 0x9E3779B97F4A7C15UL; // golden ratio constant
        }
        return new XorShiftRandomSource(seed, seed);
    }

    /// <inheritdoc />
    public uint NextUInt()
    {
        // xorshift64* variant
        ulong x = _state;
        x ^= x >> 12;
        x ^= x << 25;
        x ^= x >> 27;
        _state = x;
        return (uint)((x * 0x2545F4914F6CDD1DUL) >> 32);
    }

    /// <inheritdoc />
    public void NextBytes(Span<byte> buffer)
    {
        for (int i = 0; i < buffer.Length; i += 4)
        {
            var v = NextUInt();
            var span = buffer.Slice(i, Math.Min(4, buffer.Length - i));
            for (int j = 0; j < span.Length; j++)
            {
                span[j] = (byte)(v & 0xFF);
                v >>= 8;
            }
        }
    }

    /// <inheritdoc />
    public IRandomSource Clone() => new XorShiftRandomSource(_state, Seed);
}