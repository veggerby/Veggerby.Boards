using System;

using Veggerby.Boards.Internal.Acceleration;
using Veggerby.Boards.Internal.Attacks;
using Veggerby.Boards.Internal.Layout;
using Veggerby.Boards.Internal.Occupancy;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Evaluation.Mobility;

/// <summary>
/// Computes lightweight per-player mobility metrics using current acceleration context (bitboards + attack rays).
/// </summary>
/// <remarks>
/// Intent: Provide an internal heuristic primitive (future evaluation / AI modules) without coupling to any scoring model.
/// Mobility is defined here as the count of pseudo-legal destination tiles reachable by each player's sliding pieces plus
/// fixed leaper moves (currently only directional rays leveraged; non-sliders deferred). Captures are counted once per target tile.
/// This implementation is deliberately minimal: it walks each piece's geometric rays, respecting occupancy (friendly block, enemy capture terminal).
/// If bitboards or attack rays are unavailable the evaluator returns zeroed metrics (caller may fallback).
/// Determinism: Pure function of <see cref="GameState"/> + immutable acceleration snapshots.
/// Allocation: Uses pre-allocated scratch arrays sized to board tile count; no per-piece heap allocations.
/// </remarks>
internal sealed class MobilityEvaluator
{
    private readonly BoardShape _shape;
    private readonly BitboardLayout? _layout; // may be null when bitboards disabled
    private readonly IOccupancyIndex _occupancy;
    private readonly IAttackRays _rays;

    private MobilityEvaluator(BoardShape shape, BitboardLayout? layout, IOccupancyIndex occupancy, IAttackRays rays)
    {
        _shape = shape;
        _layout = layout;
        _occupancy = occupancy;
        _rays = rays;
    }

    public static MobilityEvaluator? TryCreate(EngineCapabilities? caps)
    {
        if (caps?.AccelerationContext is BitboardAccelerationContext bbCtx && caps.AccelerationContext.Occupancy is not null)
        {
            // Reflect board shape field from BitboardAccelerationContext (private _shape).
            BoardShape? shape = null;
            var shapeField = typeof(BitboardAccelerationContext).GetField("_shape", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (shapeField is not null)
            {
                shape = shapeField.GetValue(bbCtx) as BoardShape;
            }

            if (shape is null)
            {
                return null;
            }

            BitboardLayout? layout = null;
            var layoutField = typeof(BitboardAccelerationContext).GetField("_bitboardLayout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (layoutField is not null)
            {
                layout = layoutField.GetValue(bbCtx) as BitboardLayout;
            }

            return new MobilityEvaluator(shape, layout, caps.AccelerationContext.Occupancy, caps.AccelerationContext.AttackRays);
        }

        return null;
    }

    /// <summary>
    /// Computes mobility counts per player. Index ordering matches the underlying bitboard layout player ordering.
    /// Returns an empty array if prerequisites (bitboards, rays) are unavailable.
    /// </summary>
    public int[] Compute(GameState state)
    {
        if (_shape is null || _occupancy is null || _rays is null || _layout is null)
        {
            return Array.Empty<int>();
        }

        var counts = new int[_layout.PlayerCount];
        // Scratch to avoid double counting target tiles per piece (optional). Not needed for simple additive model, omit for now.
        foreach (var pieceState in state.GetStates<PieceState>())
        {
            var piece = pieceState.Artifact;
            if (piece.Owner is null)
            {
                continue;
            }

            if (!_layout.TryGetPlayerIndex(piece.Owner, out var ownerIdx))
            {
                continue;
            }

            var origin = pieceState.CurrentTile;
            if (origin is null)
            {
                continue;
            }

            if (!_rays.TryGetRays(piece, origin, out var rays))
            {
                continue; // non-sliders skipped for now
            }

            if (!_shape.TryGetTileIndex(origin, out var originIdx))
            {
                continue;
            }

            var dirCount = _shape.DirectionCount;
            for (int d = 0; d < dirCount && d < rays.Length; d++)
            {
                var mask = rays[d];
                if (mask == 0UL)
                {
                    continue;
                }

                // Walk geometric ray until block or end.
                var currentIdx = originIdx;
                while (true)
                {
                    var nextIdx = _shape.Neighbors[currentIdx * dirCount + d];
                    if (nextIdx < 0)
                    {
                        break;
                    }

                    var nextTile = _shape.Tiles[nextIdx];
                    var occupied = !_occupancy.IsEmpty(nextTile);
                    if (occupied)
                    {
                        if (!_occupancy.IsOwnedBy(nextTile, piece.Owner))
                        {
                            counts[ownerIdx]++; // capture target
                        }

                        break; // blocked beyond
                    }

                    counts[ownerIdx]++; // empty square
                    currentIdx = nextIdx;
                }
            }
        }
        return counts;
    }
}