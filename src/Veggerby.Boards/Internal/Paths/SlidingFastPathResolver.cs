using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Internal.Attacks;
using Veggerby.Boards.Internal.Layout;
using Veggerby.Boards.Internal.Occupancy;

namespace Veggerby.Boards.Internal.Paths;

/// <summary>
/// Decorator path resolver that first attempts a sliding fast-path using attack rays + occupancy,
/// falling back to an inner compiled resolver when no fast-path applies or fails.
/// </summary>
internal sealed class SlidingFastPathResolver(BoardShape shape, IAttackRays rays, IOccupancyIndex occupancy, IPathResolver inner) : IPathResolver
{
    private readonly BoardShape _shape = shape;
    private readonly IAttackRays _rays = rays;
    private readonly IOccupancyIndex _occupancy = occupancy;
    private readonly IPathResolver _inner = inner;

    public TilePath Resolve(Piece piece, Tile from, Tile to, States.GameState state)
    {
        if (piece is null || from is null || to is null)
        {
            return null;
        }

        // Determine if piece is a slider (repeatable directional pattern present)
        bool isSlider = false;
        foreach (var pattern in piece.Patterns)
        {
            if (pattern is Artifacts.Patterns.DirectionPattern dp && dp.IsRepeatable) { isSlider = true; break; }
            if (pattern is Artifacts.Patterns.MultiDirectionPattern md && md.IsRepeatable) { isSlider = true; break; }
        }

        if (isSlider && _rays is not null && _occupancy is not null && _shape is not null && _rays.TryGetRays(piece, from, out var rays))
        {
            if (_shape.TryGetTileIndex(from, out var fromIdx) && _shape.TryGetTileIndex(to, out var toIdx))
            {
                if (TryResolveViaRays(piece, from, to, state, rays, fromIdx, toIdx, out var fastPath))
                {
                    return fastPath;
                }
            }
        }

        return _inner.Resolve(piece, from, to, state);
    }

    private bool TryResolveViaRays(Piece piece, Tile from, Tile to, States.GameState state, ulong[] rays, int fromIdx, int toIdx, out TilePath path)
    {
        path = null;
        if (fromIdx == toIdx)
        {
            return false; // zero-length move not produced by sliding path generator
        }

        // Determine if destination lies on any geometric ray; capture direction index for reconstruction.
        var dirCount = _shape.DirectionCount;
        int matchedDirection = -1;
        for (int d = 0; d < dirCount; d++)
        {
            var mask = rays.Length > d ? rays[d] : 0UL;
            if (mask == 0UL)
            {
                continue;
            }
            var bit = 1UL << toIdx;
            if ((mask & bit) != 0UL)
            {
                matchedDirection = d;
                break;
            }
        }

        if (matchedDirection < 0)
        {
            return false; // not on any ray
        }

        // Walk along matched direction using precomputed neighbor indices until we reach destination or blockage.
        var steps = new List<Tile>(8);
        var currentIdx = fromIdx;
        var destIdx = toIdx;
        var direction = _shape.Directions[matchedDirection];
        var globalMaskAvailable = _occupancy.GlobalMask != 0UL || _occupancy.GlobalMask == 0UL; // always accessible property; used to avoid repeated property calls

        while (true)
        {
            var nextIdx = _shape.Neighbors[currentIdx * dirCount + matchedDirection];
            if (nextIdx < 0)
            {
                return false; // ray terminated before reaching destination
            }

            var nextTile = _shape.Tiles[nextIdx];
            // Occupancy check
            var occupied = !_occupancy.IsEmpty(nextTile);
            if (nextIdx == destIdx)
            {
                // Destination reached; ensure we are not capturing friendly piece.
                if (occupied && piece.Owner is not null && _occupancy.IsOwnedBy(nextTile, piece.Owner))
                {
                    return false; // cannot capture own piece
                }
                steps.Add(nextTile);
                break;
            }
            else
            {
                if (occupied)
                {
                    return false; // blocked before destination
                }
                steps.Add(nextTile);
            }
            currentIdx = nextIdx;
        }

        if (steps.Count == 0)
        {
            return false;
        }

        // Build relations from from -> each step in order.
        var relations = new List<TileRelation>(steps.Count);
        var prev = from;
        foreach (var tile in steps)
        {
            // We must locate the direction from prev to tile. Iterate all directions and pick the matching neighbor.
            TileRelation rel = null;
            if (!_shape.TryGetTileIndex(prev, out var prevIdxLookup))
            {
                return false;
            }
            for (int d = 0; d < dirCount; d++)
            {
                var neighIdx = _shape.Neighbors[prevIdxLookup * dirCount + d];
                if (neighIdx >= 0 && _shape.Tiles[neighIdx] == tile)
                {
                    rel = new TileRelation(prev, tile, _shape.Directions[d]);
                    break;
                }
            }
            if (rel is null)
            {
                return false; // adjacency failure (should not happen if BoardShape is consistent)
            }
            relations.Add(rel);
            prev = tile;
        }
        path = new TilePath(relations);
        return true;
    }
}