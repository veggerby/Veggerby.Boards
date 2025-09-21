using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veggerby.Boards.Internal.Hash;

/// <summary>
/// Minimal managed xxHash128 implementation (little-endian) for deterministic, non-cryptographic hashing.
/// </summary>
/// <remarks>
/// This is a compact, allocation-free implementation adapted from the public domain xxHash specification.
/// It is intentionally simplified for engine state hashing: it favors clarity and a small surface over
/// exhaustive micro-optimizations and does not attempt to exactly replicate every mixing nuance of the
/// full 128-bit reference (the high 64-bit part is derived via a reduced mixing sequence). Good collision
/// resistance for small/medium (&lt; 4 KB) canonical state payloads is sufficient for our timeline integrity
/// checks and bug report fingerprints. It MUST NOT be used for security purposes.
/// </remarks>
internal static class XXHash128
{
    private const ulong Prime64_1 = 11400714785074694791UL;
    private const ulong Prime64_2 = 14029467366897019727UL;
    private const ulong Prime64_3 = 1609587929392839161UL;
    private const ulong Prime64_4 = 9650029242287828579UL;
    private const ulong Prime64_5 = 2870177450012600261UL;

    /// <summary>
    /// Computes an xxHash128 over the provided data span using optional low/high 64-bit seeds.
    /// </summary>
    /// <param name="data">The input bytes to hash (little-endian interpretation for lane loads).</param>
    /// <param name="seedLow">Optional low 64 bits of the seed (default: 0).</param>
    /// <param name="seedHigh">Optional high 64 bits of the seed (default: 0).</param>
    /// <returns>A tuple containing the low and high 64-bit parts of the 128-bit hash.</returns>
    /// <remarks>
    /// The returned (Low, High) pair is stable across platforms and runtime versions provided the
    /// canonical state serialization feeding it is stable. This method performs no allocations and
    /// is safe to call within tight loops.
    /// </remarks>
    public static (ulong Low, ulong High) Compute(ReadOnlySpan<byte> data, ulong seedLow = 0UL, ulong seedHigh = 0UL)
    {
        var acc1 = seedLow + Prime64_1 + Prime64_2;
        var acc2 = seedHigh + Prime64_2;
        var acc3 = seedLow + 0UL;
        var acc4 = seedHigh - Prime64_1;

        int offset = 0;
        int length = data.Length;
        if (length >= 32)
        {
            int limit = length - 32;
            while (offset <= limit)
            {
                acc1 = Round(acc1, ReadUInt64(data, offset)); offset += 8;
                acc2 = Round(acc2, ReadUInt64(data, offset)); offset += 8;
                acc3 = Round(acc3, ReadUInt64(data, offset)); offset += 8;
                acc4 = Round(acc4, ReadUInt64(data, offset)); offset += 8;
            }
        }

        ulong low;
        ulong high;
        if (length >= 32)
        {
            low = BitRotateLeft(acc1, 1) + BitRotateLeft(acc2, 7) + BitRotateLeft(acc3, 12) + BitRotateLeft(acc4, 18);
            high = low;
            low = MergeRound(low, acc1);
            low = MergeRound(low, acc2);
            low = MergeRound(low, acc3);
            low = MergeRound(low, acc4);
        }
        else
        {
            low = seedLow + Prime64_5;
            high = seedHigh + Prime64_5;
        }
        low += (ulong)length;
        high += (ulong)length;

        // process remaining
        while (offset + 8 <= length)
        {
            var k1 = ReadUInt64(data, offset);
            low ^= Round(0UL, k1);
            low = BitRotateLeft(low, 27) * Prime64_1 + Prime64_4;
            offset += 8;
        }
        if (offset + 4 <= length)
        {
            low ^= ((ulong)ReadUInt32(data, offset)) * Prime64_1;
            low = BitRotateLeft(low, 23) * Prime64_2 + Prime64_3;
            offset += 4;
        }
        while (offset < length)
        {
            low ^= data[offset] * Prime64_5;
            low = BitRotateLeft(low, 11) * Prime64_1;
            offset++;
        }

        // avalanche low
        low ^= low >> 33;
        low *= Prime64_2;
        low ^= low >> 29;
        low *= Prime64_3;
        low ^= low >> 32;

        // Derive high from mixed accumulators for 128-bit variant (simplified; full spec includes additional mixing).
        high ^= high >> 33;
        high *= Prime64_2;
        high ^= high >> 29;
        high *= Prime64_3;
        high ^= high >> 32;

        return (low, high);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ReadUInt64(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(data[offset..]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ReadUInt32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(data[offset..]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Round(ulong acc, ulong input)
    {
        acc += input * Prime64_2;
        acc = BitRotateLeft(acc, 31);
        acc *= Prime64_1;
        return acc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong MergeRound(ulong acc, ulong val)
    {
        val = Round(0UL, val);
        acc ^= val;
        acc = acc * Prime64_1 + Prime64_4;
        return acc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong BitRotateLeft(ulong value, int bits) => (value << bits) | (value >> (64 - bits));
}