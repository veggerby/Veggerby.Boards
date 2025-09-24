using System;

namespace Veggerby.Boards.Internal.Layout;

/// <summary>
/// Service container for bitboard layout + current snapshot (incrementally updated via GameProgress similar to PieceMap).
/// </summary>
internal sealed class BitboardServices(BitboardLayout layout, BitboardSnapshot snapshot)
{
    public BitboardLayout Layout { get; } = layout ?? throw new ArgumentNullException(nameof(layout));
    public BitboardSnapshot Snapshot { get; } = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
}