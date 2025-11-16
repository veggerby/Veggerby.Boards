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
    /// Attempts to build occupancy bitboards for the provided progress state (bitboards always enabled - graduated feature).
    /// </summary>
    /// <param name="progress">Progress snapshot.</param>
    /// <param name="occupancy">All-piece occupancy mask.</param>
    /// <param name="perPlayer">Optional per-player occupancy mapping.</param>
    /// <returns>True when bitboards available; otherwise false.</returns>
    internal static bool TryGetBitboards(this GameProgress progress, out Bitboard64 occupancy, out Dictionary<Artifacts.Player, Bitboard64>? perPlayer)
    {
        // Bitboard snapshots are now internal to the acceleration context; expose only via occupancy masks.
        // Provide minimal reconstruction using IOccupancyIndex when available.
        if (progress.Engine.Capabilities?.AccelerationContext?.Occupancy is not null)
        {
            var occ = progress.Engine.Capabilities.AccelerationContext.Occupancy;
            // Global mask exposure (bitboards feature defines hash semantics external to index)
            occupancy = new Bitboard64((occ as Internal.Occupancy.BitboardOccupancyIndex)?.GlobalMask ?? 0UL);
            perPlayer = new Dictionary<Artifacts.Player, Bitboard64>();
            foreach (var player in progress.Game.Players)
            {
                var mask = (occ as Internal.Occupancy.BitboardOccupancyIndex)?.PlayerMask(player) ?? 0UL;
                perPlayer[player] = new Bitboard64(mask);
            }
            return true;
        }
        occupancy = default;
        perPlayer = null;
        return false;
    }
}