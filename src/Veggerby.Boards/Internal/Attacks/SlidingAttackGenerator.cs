using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Internal.Attacks;

/// <summary>
/// Generates sliding attacks (ray-style movement) using precomputed direction rays and occupancy snapshot.
/// </summary>
internal sealed class SlidingAttackGenerator
{
    private readonly BoardShape _shape;
    private readonly short[][] _rayTileIndices;

    private SlidingAttackGenerator(BoardShape shape, short[][] rays)
    {
        _shape = shape;
        _rayTileIndices = rays;
    }

    public static SlidingAttackGenerator Build(BoardShape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);
        var rays = new short[shape.TileCount * shape.DirectionCount][];
        for (int t = 0; t < shape.TileCount; t++)
        {
            for (int d = 0; d < shape.DirectionCount; d++)
            {
                var list = new List<short>(8);
                var current = t;
                while (true)
                {
                    var neighborIdx = shape.Neighbors[current * shape.DirectionCount + d];
                    if (neighborIdx < 0)
                    {
                        break;
                    }
                    list.Add(neighborIdx);
                    current = neighborIdx;
                }
                rays[t * shape.DirectionCount + d] = list.ToArray();
            }
        }
        return new SlidingAttackGenerator(shape, rays);
    }

    public List<short> GetSlidingAttacks(Piece piece, short fromTileIndex, PieceMapSnapshot pieceMap, BitboardSnapshot bitboards)
    {
        var results = new List<short>();
        if (fromTileIndex < 0 || fromTileIndex >= _shape.TileCount)
        {
            return results;
        }
        var global = bitboards?.GlobalOccupancy ?? 0UL;
        var ownerIndexByTile = new sbyte[_shape.TileCount];
        for (int i = 0; i < ownerIndexByTile.Length; i++) ownerIndexByTile[i] = -1;
        if (pieceMap is not null)
        {
            for (int pIdx = 0; pIdx < pieceMap.Layout.PieceCount; pIdx++)
            {
                var tileIdx = pieceMap.PieceTileIndices[pIdx];
                if (tileIdx < 0) { continue; }
                var pieceOwner = pieceMap.Layout.Pieces[pIdx].Owner;
                if (pieceOwner is null) { continue; }
                if (pieceMap.Layout.TryGetPlayerIndex(pieceOwner, out var ownerIdx))
                {
                    ownerIndexByTile[tileIdx] = (sbyte)ownerIdx;
                }
            }
        }
        var friendlyOwnerIndex = (sbyte)-1;
        if (piece.Owner is not null && pieceMap?.Layout.TryGetPlayerIndex(piece.Owner, out var po) == true)
        {
            friendlyOwnerIndex = (sbyte)po;
        }
        for (int d = 0; d < _shape.DirectionCount; d++)
        {
            var ray = _rayTileIndices[fromTileIndex * _shape.DirectionCount + d];
            if (ray is null || ray.Length == 0)
            {
                continue;
            }
            for (int i = 0; i < ray.Length; i++)
            {
                var target = ray[i];
                var bit = 1UL << target;
                var occupied = (global & bit) != 0UL;
                if (!occupied)
                {
                    results.Add((short)target);
                    continue;
                }
                var occupantOwner = ownerIndexByTile[target];
                if (occupantOwner >= 0 && occupantOwner != friendlyOwnerIndex)
                {
                    results.Add((short)target);
                }
                break;
            }
        }
        return results;
    }
}