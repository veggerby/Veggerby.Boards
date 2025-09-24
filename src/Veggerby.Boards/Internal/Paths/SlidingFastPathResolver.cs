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

        if (isSlider && _rays is not null && _occupancy is not null && _rays.TryGetRays(piece, from, out var rays))
        {
            // We need BoardShape to map tiles to indices. Try to obtain from occupancy (bitboard index) by probing GlobalMask after cast; fallback to inner if not available.
            if (_inner is CompiledPathResolverAdapter compiledAdapter)
            {
                // We cannot access internal board shape here; rely purely on geometric reconstruction via piece patterns using adjacency fallback.
                // Proceed only if destination lies on at least one ray mask (geometric inclusion test).
                if (SlidingFastPathResolver.TryResolveViaRays(piece, from, to, rays, state, out var fastPath))
                {
                    return fastPath;
                }
            }
            else
            {
                if (SlidingFastPathResolver.TryResolveViaRays(piece, from, to, rays, state, out var fastPath))
                {
                    return fastPath;
                }
            }
        }

        return _inner.Resolve(piece, from, to, state);
    }

    private static bool TryResolveViaRays(Piece piece, Tile from, Tile to, ulong[] rays, States.GameState state, out TilePath path)
    {
        path = null;
        // Quick inclusion test: if no ray contains 'to', bail.
        // To perform bit test we need tile indices; without BoardShape index mapping we fallback to adjacency traversal using piece patterns.
        // Here we attempt a directional step reconstruction: follow each repeatable direction outward until we reach 'to' or blocked.

        // Collect repeatable direction ids for piece
        var directions = new List<string>();
        foreach (var pattern in piece.Patterns)
        {
            if (pattern is Artifacts.Patterns.DirectionPattern dp && dp.IsRepeatable)
            {
                directions.Add(dp.Direction.Id);
            }
            else if (pattern is Artifacts.Patterns.MultiDirectionPattern md && md.IsRepeatable)
            {
                foreach (var d in md.Directions) { directions.Add(d.Id); }
            }
        }

        if (directions.Count == 0)
        {
            return false;
        }

        // Access BoardShape through piece.Board (indirectly via from relation indices) not directly exposed; fallback to inner resolver if shape unavailable.
        // Retrieve shape by using reflection to locate internal service is avoided; instead rely on geometric board relations enumeration via board.TileRelations.
        // Fast-path reconstruction requires BoardShape adjacency (not yet injected here) – abort for now.
        return false;
    }
}