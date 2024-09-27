using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Tests.Core.Fakes;

public class TestBoard : Board
{
    public TestBoard() : base("test", GetRelations())
    {
    }

    private static IEnumerable<Tile> GetTiles()
    {
        var _tiles = new List<Tile>();
        for (int i = 0; i < 16; i++)
        {
            _tiles.Add(new Tile($"tile-{i}"));
        }

        _tiles.Add(new Tile("tile-x"));
        _tiles.Add(new Tile("tile-y"));

        return _tiles;
    }

    private static IEnumerable<TileRelation> GetRelations()
    {
        var tiles = GetTiles();
        var _tileRelations = new List<TileRelation>();

        for (int i = 0; i < 16; i++)
        {
            var from = tiles.Single(x => x.Id == $"tile-{i}");
            var to = tiles.Single(x => x.Id == $"tile-{(i + 1) % 16}");
            var relation = new TileRelation(from, to, Direction.Clockwise, 3);
            //System.Console.WriteLine($"Relation: {relation}");
            _tileRelations.Add(relation);
        }

        for (int i = 0; i < 4; i++)
        {
            var from = tiles.Single(x => x.Id == $"tile-{((i + 1) * 4) % 16}");
            var to = tiles.Single(x => x.Id == $"tile-{i * 4}");
            var relation = new TileRelation(from, to, Direction.Across, 2);
            //System.Console.WriteLine($"Relation: {relation}");
            _tileRelations.Add(relation);
        }

        _tileRelations.Add(new TileRelation(
            tiles.Single(x => x.Id == "tile-4"),
            tiles.Single(x => x.Id == "tile-12"),
            Direction.Up,
            1
        ));

        _tileRelations.Add(new TileRelation(
            tiles.Single(x => x.Id == "tile-8"),
            tiles.Single(x => x.Id == "tile-0"),
            Direction.Up,
            1
        ));

        _tileRelations.Add(new TileRelation(
            tiles.Single(x => x.Id == "tile-x"),
            tiles.Single(x => x.Id == "tile-y"),
            Direction.Left
        ));

        return _tileRelations;
    }
}