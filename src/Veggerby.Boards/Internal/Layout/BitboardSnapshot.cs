using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Layout;

/// <summary>
/// Immutable bitboard occupancy snapshot. For boards up to 64 tiles we use a single 64-bit mask.
/// Provides: global occupancy mask and per-player occupancy masks. Future extension will add piece-type masks
/// and Bitboard128 for larger boards.
/// </summary>
internal sealed class BitboardSnapshot
{
    public BitboardLayout Layout { get; }
    // For legacy (<=64 tiles) we keep ulong fields. When tile count exceeds 64 we store a Bitboard128 composite.
    public ulong GlobalOccupancy { get; }
    public ulong[] PlayerOccupancy { get; } // length = player count; each entry is 64-bit (will extend when >64)
    public Bitboard128? GlobalOccupancy128 { get; }
    public Bitboard128[] PlayerOccupancy128 { get; } // length = player count when using 128 variant (null otherwise)

    private BitboardSnapshot(BitboardLayout layout, ulong global, ulong[] perPlayer)
    {
        Layout = layout;
        GlobalOccupancy = global;
        PlayerOccupancy = perPlayer;
    }

    private BitboardSnapshot(BitboardLayout layout, Bitboard128 global128, Bitboard128[] perPlayer128)
    {
        Layout = layout;
        GlobalOccupancy128 = global128;
        PlayerOccupancy128 = perPlayer128;
    }

    public static BitboardSnapshot Build(BitboardLayout layout, GameState state, BoardShape shape)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(shape);

        if (shape.TileCount > 64)
        {
            // Build Bitboard128 variant (two 64-bit segments). Segment0 holds tiles [0..63], segment1 holds [64..127].
            var perPlayer128 = new Bitboard128[layout.PlayerCount];
            var global128 = Bitboard128.Empty;

            foreach (var ps in state.GetStates<PieceState>())
            {
                if (ps.CurrentTile is null)
                {
                    continue;
                }

                if (!shape.TryGetTileIndex(ps.CurrentTile, out var tileIndex))
                {
                    continue;
                }

                bool high = tileIndex >= 64;
                int offset = high ? tileIndex - 64 : tileIndex;
                var bit = 1UL << offset;
                global128 = high ? global128.WithHigh(global128.High | bit) : global128.WithLow(global128.Low | bit);

                if (ps.Artifact.Owner is not null && layout.TryGetPlayerIndex(ps.Artifact.Owner, out var pIdxH))
                {
                    var current = perPlayer128[pIdxH];
                    perPlayer128[pIdxH] = high ? current.WithHigh(current.High | bit) : current.WithLow(current.Low | bit);
                }
            }
            return new BitboardSnapshot(layout, global128, perPlayer128);
        }

        ulong global = 0UL;
        var perPlayer = new ulong[layout.PlayerCount];

        foreach (var ps in state.GetStates<PieceState>())
        {
            if (ps.CurrentTile is null)
            {
                continue;
            }

            if (!shape.TryGetTileIndex(ps.CurrentTile, out var tileIndex))
            {
                continue;
            }

            var bit = 1UL << tileIndex;
            global |= bit;

            if (ps.Artifact.Owner is not null && layout.TryGetPlayerIndex(ps.Artifact.Owner, out var pIdx))
            {
                perPlayer[pIdx] |= bit;
            }
        }

        return new BitboardSnapshot(layout, global, perPlayer);
    }

    /// <summary>
    /// Incremental update for a piece move. Assumes move from one tile to another (no captures yet handled here).
    /// </summary>
    public BitboardSnapshot UpdateForMove(Piece piece, short fromTileIndex, short toTileIndex, PieceMapSnapshot pieceMap, BoardShape shape)
    {
        // PieceMap supplies authoritative current tile index; if mismatch treat as no-op.
        if (pieceMap is null)
        {
            return this; // require piece map for validation
        }

        if (!pieceMap.Layout.TryGetPieceIndex(piece, out var _))
        {
            return this;
        }

        // Validate expected from index if provided (>=0)
        if (fromTileIndex >= 0)
        {
            var currentIdx = pieceMap.GetTileIndex(piece);
            if (currentIdx != fromTileIndex)
            {
                return this; // stale
            }
        }

        if (fromTileIndex == toTileIndex)
        {
            return this; // no movement
        }

        if (shape.TileCount > 64 && GlobalOccupancy128.HasValue)
        {
            // 128-bit incremental path
            var global128 = GlobalOccupancy128.Value;
            var perPlayer128 = (Bitboard128[])PlayerOccupancy128.Clone();
            bool fromHigh = fromTileIndex >= 64;
            bool toHigh = toTileIndex >= 64;
            ulong fromBit = fromTileIndex >= 0 ? 1UL << (fromHigh ? fromTileIndex - 64 : fromTileIndex) : 0UL;
            ulong toBit = toTileIndex >= 0 ? 1UL << (toHigh ? toTileIndex - 64 : toTileIndex) : 0UL;

            if (fromBit != 0)
            {
                global128 = fromHigh ? global128.WithHigh(global128.High & ~fromBit) : global128.WithLow(global128.Low & ~fromBit);
            }

            if (toBit != 0)
            {
                global128 = toHigh ? global128.WithHigh(global128.High | toBit) : global128.WithLow(global128.Low | toBit);
            }

            if (piece.Owner is not null && Layout.TryGetPlayerIndex(piece.Owner, out var pIdxX))
            {
                var cur = perPlayer128[pIdxX];
                if (fromBit != 0)
                {
                    cur = fromHigh ? cur.WithHigh(cur.High & ~fromBit) : cur.WithLow(cur.Low & ~fromBit);
                }

                if (toBit != 0)
                {
                    cur = toHigh ? cur.WithHigh(cur.High | toBit) : cur.WithLow(cur.Low | toBit);
                }

                perPlayer128[pIdxX] = cur;
            }
            return new BitboardSnapshot(Layout, global128, perPlayer128);
        }
        else
        {
            // 64-bit path
            var perPlayerClone = (ulong[])PlayerOccupancy.Clone();
            ulong global = GlobalOccupancy;
            ulong fromBit = fromTileIndex >= 0 ? 1UL << fromTileIndex : 0UL;
            ulong toBit = toTileIndex >= 0 ? 1UL << toTileIndex : 0UL;

            if (fromBit != 0)
            {
                global &= ~fromBit;
            }

            if (toBit != 0)
            {
                global |= toBit;
            }

            if (piece.Owner is not null && Layout.TryGetPlayerIndex(piece.Owner, out var pIdx2))
            {
                if (fromBit != 0)
                {
                    perPlayerClone[pIdx2] &= ~fromBit;
                }

                if (toBit != 0)
                {
                    perPlayerClone[pIdx2] |= toBit;
                }
            }

            return new BitboardSnapshot(Layout, global, perPlayerClone);
        }
    }
}
