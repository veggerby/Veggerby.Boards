using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class TestBoard : Board
    {
        public TestBoard() : base("test", GetRelations())
        {
        }

        private static IEnumerable<Tile> GetTiles()
        {
            var _tiles = new List<Tile>();
            for (int i = 0; i < 32; i++)
            {
                _tiles.Add(new Tile($"tile-{i}"));
            }
            return _tiles;
        }

        private static IEnumerable<TileRelation> GetRelations()
        {
            var tiles = GetTiles();
            var _tileRelations = new List<TileRelation>();

            for (int i = 0; i < 32; i++)            
            {
                var from = tiles.Single(x => x.Id == $"tile-{i}");
                var to = tiles.Single(x => x.Id == $"tile-{(i+1) % 32}");
                var relation = new TileRelation(from, to, Direction.Clockwise);
                //System.Console.WriteLine($"Relation: {relation}");
                _tileRelations.Add(relation);
            }

            for (int i = 0; i < 4; i++)
            {
                var from = tiles.Single(x => x.Id == $"tile-{((i+1) * 8) % 32}");
                var to = tiles.Single(x => x.Id == $"tile-{i * 8}");
                var relation = new TileRelation(from, to, Direction.Across, 5);
                //System.Console.WriteLine($"Relation: {relation}");
                _tileRelations.Add(relation);
            }

            return _tileRelations;
        }
    }
}