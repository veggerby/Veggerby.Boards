using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Layout;

/// <summary>
/// Immutable snapshot of per-piece tile indices (or -1 if piece not on a tile / captured in future extensions).
/// </summary>
internal sealed class PieceMapSnapshot
{
    public PieceMapLayout Layout
    {
        get;
    }
    public short[] PieceTileIndices
    {
        get;
    } // length = piece count
    public short[] PlayerPieceCounts
    {
        get;
    } // length = player count

    private PieceMapSnapshot(PieceMapLayout layout, short[] pieceTileIndices, short[] playerPieceCounts)
    {
        Layout = layout;
        PieceTileIndices = pieceTileIndices;
        PlayerPieceCounts = playerPieceCounts;
    }

    public static PieceMapSnapshot Build(PieceMapLayout layout, GameState state, BoardShape shape)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(shape);

        var pieceTiles = new short[layout.PieceCount];
        for (int i = 0; i < pieceTiles.Length; i++)
        {
            pieceTiles[i] = -1;
        }

        var counts = new short[layout.PlayerCount];
        foreach (var ps in state.GetStates<PieceState>())
        {
            if (!layout.TryGetPieceIndex(ps.Artifact, out var pIdx))
            {
                continue;
            }

            if (shape.TryGetTileIndex(ps.CurrentTile, out var tileIndex))
            {
                pieceTiles[pIdx] = (short)tileIndex;
            }

            if (ps.Artifact.Owner is not null && layout.TryGetPlayerIndex(ps.Artifact.Owner, out var ownerIdx))
            {
                counts[ownerIdx]++;
            }
        }

        return new PieceMapSnapshot(layout, pieceTiles, counts);
    }

    public short GetTileIndex(Piece piece)
    {
        if (!Layout.TryGetPieceIndex(piece, out var idx))
        {
            return -1;
        }

        return PieceTileIndices[idx];
    }

    public PieceMapSnapshot UpdateForMove(Piece piece, short newTileIndex)
    {
        if (!Layout.TryGetPieceIndex(piece, out var idx))
        {
            return this;
        }

        var clone = (short[])PieceTileIndices.Clone();
        clone[idx] = newTileIndex;

        return new PieceMapSnapshot(Layout, clone, PlayerPieceCounts); // counts unchanged for simple move
    }

    public PieceMapSnapshot UpdateForMove(Piece piece, short expectedFromTileIndex, short toTileIndex)
    {
        if (!Layout.TryGetPieceIndex(piece, out var idx))
        {
            return this;
        }

        // If expectedFromTileIndex provided (>=0) ensure optimistic concurrency style match
        if (expectedFromTileIndex >= 0 && PieceTileIndices[idx] != expectedFromTileIndex)
        {
            return this; // treat mismatch as no-op (could throw if stricter behavior desired)
        }

        var clone = (short[])PieceTileIndices.Clone();
        clone[idx] = toTileIndex;
        return new PieceMapSnapshot(Layout, clone, PlayerPieceCounts);
    }
}