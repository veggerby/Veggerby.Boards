using Shouldly;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts.Relations
{
    public class TilePathTests
    {
        public class Ctor
        {
            [Fact]
            public void Should_create_tilepath()
            {
                // arrange
                var board = new TestBoard();
                var tile1 = board.GetTile("tile-1");
                var tile2 = board.GetTile("tile-2");
                var tile3 = board.GetTile("tile-3");
                var relation1to2 = board.GetTileRelation(tile1, Direction.Clockwise);
                var relation2to3 = board.GetTileRelation(tile2, Direction.Clockwise);

                // act
                var actual = new TilePath(new [] { relation1to2, relation2to3 });
                
                // assert
                actual.Relations.ShouldBe(new [] { relation1to2, relation2to3 });
                actual.Tiles.ShouldBe(new [] { tile1, tile2, tile3 });
                actual.Directions.ShouldBe(new [] { Direction.Clockwise, Direction.Clockwise });
                actual.From.ShouldBe(tile1);
                actual.To.ShouldBe(tile3);
                actual.Distance.ShouldBe(2);
            }
        }

        public class Add
        {
            [Fact]
            public void Should_add_step_to_tilepath()
            {
                // arrange
                var board = new TestBoard();
                var tile1 = board.GetTile("tile-1");
                var tile2 = board.GetTile("tile-2");
                var tile3 = board.GetTile("tile-3");
                var tile4 = board.GetTile("tile-4");
                var relation1to2 = board.GetTileRelation(tile1, Direction.Clockwise);
                var relation2to3 = board.GetTileRelation(tile2, Direction.Clockwise);
                var relation3to4 = board.GetTileRelation(tile3, Direction.Clockwise);
                var path = new TilePath(new [] { relation1to2, relation2to3 });

                // act
                var actual = path.Add(relation3to4);
                
                // assert
                actual.Relations.ShouldBe(new [] { relation1to2, relation2to3, relation3to4 });
                actual.Tiles.ShouldBe(new [] { tile1, tile2, tile3, tile4 });
                actual.Directions.ShouldBe(new [] { Direction.Clockwise, Direction.Clockwise, Direction.Clockwise });
                actual.From.ShouldBe(tile1);
                actual.To.ShouldBe(tile4);
                actual.Distance.ShouldBe(3);
            }
        }

        public class Create
        {
            [Fact]
            public void Should_create_tilepath_from_nothing()
            {
                // arrange
                var board = new TestBoard();
                var tile1 = board.GetTile("tile-1");
                var tile2 = board.GetTile("tile-2");
                var relation1to2 = board.GetTileRelation(tile1, Direction.Clockwise);

                // act
                var actual = TilePath.Create(null, relation1to2);
                
                // assert
                actual.Relations.ShouldBe(new [] { relation1to2 });
                actual.Tiles.ShouldBe(new [] { tile1, tile2 });
                actual.Directions.ShouldBe(new [] { Direction.Clockwise });
                actual.From.ShouldBe(tile1);
                actual.To.ShouldBe(tile2);
                actual.Distance.ShouldBe(1);
            }

            [Fact]
            public void Should_create_tilepath_from_existing()
            {
                // arrange
                var board = new TestBoard();
                var tile1 = board.GetTile("tile-1");
                var tile2 = board.GetTile("tile-2");
                var tile3 = board.GetTile("tile-3");
                var relation1to2 = board.GetTileRelation(tile1, Direction.Clockwise);
                var relation2to3 = board.GetTileRelation(tile2, Direction.Clockwise);
                var path = new TilePath(new [] { relation1to2  });

                // act
                var actual = TilePath.Create(path, relation2to3);

                // assert
                actual.Relations.ShouldBe(new [] { relation1to2, relation2to3 });
                actual.Tiles.ShouldBe(new [] { tile1, tile2, tile3 });
                actual.Directions.ShouldBe(new [] { Direction.Clockwise, Direction.Clockwise });
                actual.From.ShouldBe(tile1);
                actual.To.ShouldBe(tile3);
                actual.Distance.ShouldBe(2);
            }
        }
    }
}