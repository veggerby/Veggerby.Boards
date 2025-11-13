using System;
using System.Numerics;

namespace Veggerby.Boards.Internal.Bitboards;

/// <summary>
/// Lightweight 64-bit bitset wrapper representing occupancy or feature masks for boards with up to 64 tiles.
/// </summary>
/// <remarks>
/// Internal experimental structure behind <c>EnableBitboards</c> feature flag. Public surface area intentionally small to
/// avoid premature commitment. All operations are branch-light and inline friendly; PopCount uses <see cref="BitOperations.PopCount(ulong)"/>.
/// </remarks>
/// <remarks>
/// Initializes a new <see cref="Bitboard64"/> with the provided mask.
/// </remarks>
/// <param name="mask">Raw 64-bit occupancy or feature bits.</param>
public readonly struct Bitboard64(ulong mask) : IEquatable<Bitboard64>
{
    /// <summary>
    /// Underlying 64-bit mask value.
    /// </summary>
    public ulong Mask { get; } = mask;

    /// <summary>
    /// Gets a value indicating whether the bit at <paramref name="index"/> is set.
    /// </summary>
    public bool this[int index] => ((Mask >> index) & 1UL) != 0UL;

    /// <summary>
    /// Returns a new bitboard with the bit at <paramref name="index"/> set.
    /// </summary>
    public Bitboard64 Set(int index) => new Bitboard64(Mask | (1UL << index));

    /// <summary>
    /// Returns a new bitboard with the bit at <paramref name="index"/> cleared.
    /// </summary>
    public Bitboard64 Clear(int index) => new Bitboard64(Mask & ~(1UL << index));

    /// <summary>
    /// Counts the number of set bits using hardware accelerated popcount where available.
    /// </summary>
    public int PopCount() => BitOperations.PopCount(Mask);

    /// <summary>
    /// Gets a value indicating whether no bits are set.
    /// </summary>
    public bool IsEmpty => Mask == 0UL;

    /// <inheritdoc />
    public override string ToString() => $"0x{Mask:X16}";

    /// <inheritdoc />
    public bool Equals(Bitboard64 other) => Mask == other.Mask;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Bitboard64 bb && Equals(bb);

    /// <inheritdoc />
    public override int GetHashCode() => Mask.GetHashCode();

    /// <summary>Bitwise OR combination.</summary>
    public static Bitboard64 operator |(Bitboard64 a, Bitboard64 b) => new Bitboard64(a.Mask | b.Mask);
    /// <summary>Bitwise AND intersection.</summary>
    public static Bitboard64 operator &(Bitboard64 a, Bitboard64 b) => new Bitboard64(a.Mask & b.Mask);
    /// <summary>Bitwise XOR difference.</summary>
    public static Bitboard64 operator ^(Bitboard64 a, Bitboard64 b) => new Bitboard64(a.Mask ^ b.Mask);
    /// <summary>Bitwise NOT inversion.</summary>
    public static Bitboard64 operator ~(Bitboard64 a) => new Bitboard64(~a.Mask);
}