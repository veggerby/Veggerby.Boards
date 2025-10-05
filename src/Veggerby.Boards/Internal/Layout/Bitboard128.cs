namespace Veggerby.Boards.Internal.Layout;

/// <summary>
/// Simple 128-bit composite bitboard (two 64-bit segments). Immutable value-like helpers supply fluent updates.
/// Internal scaffolding only; not yet exposed externally. Designed to avoid System.UInt128 dependency for now.
/// </summary>
internal readonly struct Bitboard128(ulong low, ulong high)
{
    public ulong Low { get; } = low; public ulong High { get; } = high;

    public static Bitboard128 Empty => new(0UL, 0UL);
    public Bitboard128 WithLow(ulong low) => new(low, High);
    public Bitboard128 WithHigh(ulong high) => new(Low, high);
}