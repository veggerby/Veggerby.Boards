using System;

using Veggerby.Boards; // for BoardException
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
    public ulong GlobalOccupancy { get; }
    public ulong[] PlayerOccupancy { get; } // length = player count

    private BitboardSnapshot(BitboardLayout layout, ulong global, ulong[] perPlayer)
    {
        Layout = layout;
        GlobalOccupancy = global;
        PlayerOccupancy = perPlayer;
    }

    public static BitboardSnapshot Build(BitboardLayout layout, GameState state, BoardShape shape)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(shape);

        if (shape.TileCount > 64)
        {
            // Future: switch to Bitboard128 scaffold once implemented.
            throw new BoardException("BitboardSnapshot only supports boards up to 64 tiles (Bitboard128 pending).");
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

        // Clone per-player array (cheap, small) and recompute affected bits.
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