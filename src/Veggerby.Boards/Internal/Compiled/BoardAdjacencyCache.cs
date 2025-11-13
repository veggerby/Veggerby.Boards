using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Internal.Compiled;

/// <summary>
/// Immutable adjacency lookup built from <see cref="Board.TileRelations"/> mapping (Tile, Direction) to <see cref="TileRelation"/>.
/// Eliminates repeated linear scans when resolving compiled movement patterns.
/// </summary>
internal sealed class BoardAdjacencyCache
{
    private readonly Dictionary<Tile, Dictionary<Direction, TileRelation>> _cache;

    private BoardAdjacencyCache(Dictionary<Tile, Dictionary<Direction, TileRelation>> cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Builds a new adjacency cache for the provided board.
    /// </summary>
    /// <param name="board">Board instance.</param>
    /// <returns>Adjacency cache.</returns>
    public static BoardAdjacencyCache Build(Board board)
    {
        ArgumentNullException.ThrowIfNull(board);
        var outer = new Dictionary<Tile, Dictionary<Direction, TileRelation>>();
        foreach (var rel in board.TileRelations)
        {
            if (!outer.TryGetValue(rel.From, out var inner))
            {
                inner = [];
                outer[rel.From] = inner;
            }

            // Board definition should avoid duplicates; last wins if present.
            inner[rel.Direction] = rel;
        }
        return new BoardAdjacencyCache(outer);
    }

    /// <summary>
    /// Attempts to get the relation from a tile in a given direction.
    /// </summary>
    /// <param name="from">Source tile.</param>
    /// <param name="direction">Direction.</param>
    /// <param name="relation">Result relation (may be null if false).</param>
    /// <returns><c>true</c> when relation exists; otherwise <c>false</c>.</returns>
    public bool TryGet(Tile from, Direction direction, out TileRelation? relation)
    {
        relation = null;
        if (from is null || direction is null)
        {
            return false;
        }
        if (_cache.TryGetValue(from, out var inner))
        {
            if (inner.TryGetValue(direction, out var rel))
            {
                relation = rel;
                return true;
            }
            return false;
        }
        return false;
    }
}