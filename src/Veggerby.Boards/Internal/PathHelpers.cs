using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Internal;

/// <summary>
/// Internal helpers for constructing common tile path patterns used in benchmarks and tests.
/// </summary>
internal static class PathHelpers
{
    /// <summary>
    /// Attempts to build a two-step path using an intermediate tile id. Returns null when relations are not connected.
    /// </summary>
    /// <param name="game">The game instance.</param>
    /// <param name="from">Source tile.</param>
    /// <param name="to">Destination tile.</param>
    /// <param name="midTileId">Intermediate tile identifier.</param>
    /// <returns>Constructed <see cref="TilePath"/> or null if not possible.</returns>
    public static TilePath? TwoStepPathOrNull(Game game, Tile from, Tile to, string midTileId)
    {
        var mid = game.GetTile(midTileId);
        if (mid is null)
        {
            return null;
        }

        var r1 = game.Board.GetTileRelation(from, mid);
        if (r1 is null)
        {
            return null;
        }

        var r2 = game.Board.GetTileRelation(mid, to);
        if (r2 is null)
        {
            return null;
        }

        var first = TilePath.Create(null, r1);
        return TilePath.Create(first, r2);
    }
}
