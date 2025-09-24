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
        if (progress.Engine.Services.TryGet<BitboardServices>(out var svc))
        {
            occupancy = svc.BuildOccupancy(progress.State);
            perPlayer = svc.BuildPlayerOccupancy(progress.State);
            return true;
        }
        occupancy = default; perPlayer = null; return false;
    }
}