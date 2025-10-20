using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Internal.Attacks;

/// <summary>
/// Generates sliding attacks (ray-style movement) using precomputed direction rays and occupancy snapshot.
/// </summary>
internal sealed class SlidingAttackGenerator : IAttackRays
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
        // Guard against pathological size multiplication (defensive: prevent gigantic allocations or overflow)
        long slots = (long)shape.TileCount * (long)shape.DirectionCount;
        if (slots <= 0 || slots > 1_000_000) // heuristic upper bound (1M direction rays).
        {
            // Fallback to empty rays for safety; still return generator so callers do not crash.
            return new SlidingAttackGenerator(shape, Array.Empty<short[]>());
        }
        // Degenerate topology guard: A very long single-direction ring or chain offers little benefit for precomputed sliding.
        // Empirically this has produced pathological memory growth in synthetic stress boards; we neutralize by skipping.
        if (shape.DirectionCount == 1 && shape.TileCount > 64)
        {
            return new SlidingAttackGenerator(shape, Array.Empty<short[]>());
        }
        var rays = new short[slots][];
        for (int t = 0; t < shape.TileCount; t++)
        {
            for (int d = 0; d < shape.DirectionCount; d++)
            {
                var list = new List<short>(8);
                var current = t;
                // Track visited to avoid infinite loops on cyclic relations (e.g., wrap-around boards)
                int safety = shape.TileCount; // upper bound: cannot traverse more distinct tiles than exist
                var visited = new HashSet<int>();
                while (safety-- > 0)
                {
                    var neighborIdx = shape.Neighbors[current * shape.DirectionCount + d];
                    if (neighborIdx < 0)
                    {
                        break;
                    }
                    if (!visited.Add(neighborIdx))
                    {
                        // cycle encountered; stop extending this ray
                        break;
                    }
                    list.Add(neighborIdx);
                    // Hard per-ray cap: cannot legitimately exceed total tiles. Additionally guard with 4096 absolute limit
                    // to avoid pathological growth in unforeseen cyclic topologies (defensive programming – determinism over completeness).
                    if (list.Count >= shape.TileCount || list.Count >= 4096)
                    {
                        break;
                    }
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
        if (_rayTileIndices.Length == 0)
        {
            return results; // degenerate / neutralized generator
        }
        if (fromTileIndex < 0 || fromTileIndex >= _shape.TileCount)
        {
            return results;
        }
        var global = bitboards?.GlobalOccupancy ?? 0UL;
        var ownerIndexByTile = new sbyte[_shape.TileCount];
        for (int i = 0; i < ownerIndexByTile.Length; i++)
            ownerIndexByTile[i] = -1;
        if (pieceMap is not null)
        {
            for (int pIdx = 0; pIdx < pieceMap.Layout.PieceCount; pIdx++)
            {
                var tileIdx = pieceMap.PieceTileIndices[pIdx];
                if (tileIdx < 0)
                {
                    continue;
                }
                var pieceOwner = pieceMap.Layout.Pieces[pIdx].Owner;
                if (pieceOwner is null)
                {
                    continue;
                }
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

    public bool TryGetRays(Piece piece, Tile from, out ulong[] rays)
    {
        rays = Array.Empty<ulong>();
        if (_rayTileIndices.Length == 0)
        {
            return false; // neutralized generator
        }
        if (piece is null || from is null)
        {
            return false;
        }

        if (!_shape.TryGetTileIndex(from, out var fromIdx))
        {
            return false;
        }

        // Build rays as bitmasks per direction (stop at blockers not considered here—pure geometric rays)
        var dirCount = _shape.DirectionCount;
        var masks = new ulong[dirCount];
        for (int d = 0; d < dirCount; d++)
        {
            var ray = _rayTileIndices[fromIdx * dirCount + d];
            if (ray is null || ray.Length == 0)
            {
                continue;
            }

            ulong mask = 0UL;
            for (int i = 0; i < ray.Length; i++)
            {
                var target = ray[i];
                if (target >= 0)
                {
                    mask |= 1UL << target;
                }
            }

            masks[d] = mask;
        }

        rays = masks;
        return true;
    }
}