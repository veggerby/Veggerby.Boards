using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Bitboards;

/// <summary>
/// Provides derived bitboard snapshots for a game state (piece occupancy, per-player occupancy) using a precomputed layout.
/// </summary>
internal sealed class BitboardServices
{
    public BoardBitboardLayout Layout { get; }

    public BitboardServices(BoardBitboardLayout layout)
    {
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
    }

    /// <summary>
    /// Builds an occupancy bitboard for all pieces in the provided state.
    /// </summary>
    public Bitboard64 BuildOccupancy(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        ulong mask = 0UL;
        foreach (var ps in state.GetStates<PieceState>())
        {
            if (Layout.TryGetIndex(ps.CurrentTile, out var idx))
            {
                mask |= 1UL << idx;
            }
        }
        return new Bitboard64(mask);
    }

    /// <summary>
    /// Builds per-player occupancy bitboards keyed by player artifact (only players present in piece states included).
    /// </summary>
    public Dictionary<Player, Bitboard64> BuildPlayerOccupancy(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var map = new Dictionary<Player, ulong>();
        foreach (var ps in state.GetStates<PieceState>())
        {
            var owner = ps.Artifact.Owner;
            if (owner is null)
            {
                continue;
            }
            if (!Layout.TryGetIndex(ps.CurrentTile, out var idx))
            {
                continue;
            }
            if (!map.TryGetValue(owner, out var current))
            {
                map[owner] = 1UL << idx;
            }
            else
            {
                map[owner] = current | (1UL << idx);
            }
        }
        return map.ToDictionary(k => k.Key, v => new Bitboard64(v.Value));
    }
}