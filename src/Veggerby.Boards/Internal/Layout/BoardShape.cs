using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;

namespace Veggerby.Boards.Internal.Layout;

/// <summary>
/// Cache-friendly adjacency layout for a board. Assigns stable indices (tile order by id, direction order by id) and
/// stores a dense neighbor index array of length (TileCount * DirectionCount) with -1 for no neighbor.
/// </summary>
internal sealed class BoardShape
{
    public Tile[] Tiles { get; }
    public Direction[] Directions { get; }
    public short[] Neighbors { get; } // (tileIndex * DirectionCount + directionIndex) => neighbor tile index or -1
    private readonly Dictionary<Tile, int> _tileToIndex;
    private readonly Dictionary<Direction, int> _directionToIndex;

    private BoardShape(Tile[] tiles, Direction[] directions, short[] neighbors, Dictionary<Tile, int> t2i, Dictionary<Direction, int> d2i)
    {
        Tiles = tiles;
        Directions = directions;
        Neighbors = neighbors;
        _tileToIndex = t2i;
        _directionToIndex = d2i;
    }

    public int TileCount => Tiles.Length;
    public int DirectionCount => Directions.Length;

    public static BoardShape Build(Board board)
    {
        ArgumentNullException.ThrowIfNull(board);
        var tiles = board
            .Tiles
            .OrderBy(t => t.Id, StringComparer.Ordinal)
            .ToArray();

        var directions = board
            .TileRelations
            .Select(r => r.Direction)
            .Distinct()
            .OrderBy(d => d.Id, StringComparer.Ordinal)
            .ToArray();

        var t2i = new Dictionary<Tile, int>(tiles.Length);
        var d2i = new Dictionary<Direction, int>(directions.Length);

        for (int i = 0; i < tiles.Length; i++)
        {
            t2i[tiles[i]] = i;
        }

        for (int i = 0; i < directions.Length; i++)
        {
            d2i[directions[i]] = i;
        }

        var neighbors = new short[tiles.Length * directions.Length];
        for (int i = 0; i < neighbors.Length; i++)
        {
            neighbors[i] = -1;
        }

        foreach (var rel in board.TileRelations)
        {
            var fromIdx = t2i[rel.From];
            var dirIdx = d2i[rel.Direction];
            var toIdx = t2i[rel.To];
            neighbors[fromIdx * directions.Length + dirIdx] = (short)toIdx;
        }

        return new BoardShape(tiles, directions, neighbors, t2i, d2i);
    }

    public bool TryGetNeighbor(Tile from, Direction direction, out Tile neighbor)
    {
        neighbor = null;

        if (!_tileToIndex.TryGetValue(from, out var f) || !_directionToIndex.TryGetValue(direction, out var d))
        {
            return false;
        }

        var toIdx = Neighbors[f * DirectionCount + d];

        if (toIdx < 0)
        {
            return false;
        }

        neighbor = Tiles[toIdx];

        return true;
    }

    public bool TryGetTileIndex(Tile tile, out int index)
    {
        return _tileToIndex.TryGetValue(tile, out index);
    }
}