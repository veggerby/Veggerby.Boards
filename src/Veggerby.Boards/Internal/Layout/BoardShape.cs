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
    public BoardTopology Topology { get; }

    private BoardShape(Tile[] tiles, Direction[] directions, short[] neighbors, Dictionary<Tile, int> t2i, Dictionary<Direction, int> d2i, BoardTopology topology)
    {
        Tiles = tiles;
        Directions = directions;
        Neighbors = neighbors;
        _tileToIndex = t2i;
        _directionToIndex = d2i;
        Topology = topology;
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

        var topology = ClassifyTopology(directions);
        return new BoardShape(tiles, directions, neighbors, t2i, d2i, topology);
    }

    private static BoardTopology ClassifyTopology(Direction[] directions)
    {
        // Heuristic classification based on common chess-like direction ids.
        // If both orthogonal (N,S,E,W variants) and diagonal (NE,NW,SE,SW variants) present -> OrthogonalAndDiagonal.
        // If only orthogonal subset present -> Orthogonal.
        // Otherwise -> Arbitrary.
        // Direction ids are arbitrary user strings; we use contains tests tolerant to hyphenation.
        static bool Has(Direction[] dirs, params string[] needles)
        {
            foreach (var d in dirs)
            {
                var id = d.Id;
                for (int i = 0; i < needles.Length; i++)
                {
                    if (id.IndexOf(needles[i], StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool orth = Has(directions, "north") || Has(directions, "south") || Has(directions, "east") || Has(directions, "west");
        bool diag = Has(directions, "north-east", "northeast", "south-east", "southeast", "north-west", "northwest", "south-west", "southwest");

        if (orth && diag)
        {
            return BoardTopology.OrthogonalAndDiagonal;
        }

        if (orth)
        {
            return BoardTopology.Orthogonal;
        }

        return BoardTopology.Arbitrary;
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