using System;
using System.Collections.Generic;

using Veggerby.Boards.Internal;
using Veggerby.Boards.Internal.Bitboards;
using Veggerby.Boards.States;

namespace Veggerby.Boards;

/// <summary>
/// Bitboard related extension helpers (feature-flagged).
/// </summary>
public static partial class GameExtensions
{
    /// <summary>
    /// Attempts to build occupancy bitboards for the provided progress state when bitboards are enabled.
    /// </summary>
    /// <param name="progress">Progress snapshot.</param>
    /// <param name="occupancy">All-piece occupancy mask.</param>
    /// <param name="perPlayer">Optional per-player occupancy mapping.</param>
    /// <returns>True when bitboards available; otherwise false.</returns>
    internal static bool TryGetBitboards(this GameProgress progress, out Internal.Bitboards.Bitboard64 occupancy, out Dictionary<Artifacts.Player, Internal.Bitboards.Bitboard64> perPlayer)
    {
        if (!FeatureFlags.EnableBitboards)
        {
            occupancy = default; perPlayer = null; return false;
        }
        if (progress.Engine.Capabilities?.Bitboards is not null && progress.Engine.Capabilities.Shape is not null)
        {
            // Use incremental snapshot
            var snap = progress.Engine.Capabilities.Bitboards.Snapshot;
            occupancy = new Internal.Bitboards.Bitboard64(snap.GlobalOccupancy);
            perPlayer = new Dictionary<Artifacts.Player, Internal.Bitboards.Bitboard64>();
            // Map per-player masks using layout mapping
            var layout = progress.Engine.Capabilities.Bitboards.Layout;
            foreach (var player in progress.Game.Players)
            {
                if (layout.TryGetPlayerIndex(player, out var pIdx) && pIdx < snap.PlayerOccupancy.Length)
                {
                    perPlayer[player] = new Internal.Bitboards.Bitboard64(snap.PlayerOccupancy[pIdx]);
                }
            }
            return true;
        }
        // Legacy on-demand services (if still present for transitional period)
        if (progress.Engine.Capabilities is not null && progress.Engine.Capabilities.Shape is not null)
        {
            var legacy = progress.Engine.Capabilities; // legacy BoardBitboardLayout services removed; fallback false
        }
        occupancy = default; perPlayer = null; return false;
    }
}