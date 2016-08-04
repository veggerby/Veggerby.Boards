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
                Assert.Equal(new [] { relation1to2, relation2to3 }, actual.Relations);
                Assert.Equal(new [] { tile1, tile2, tile3 }, actual.Tiles);
                Assert.Equal(new [] { Direction.Clockwise, Direction.Clockwise }, actual.Directions);
                Assert.Equal(tile1, actual.From);
                Assert.Equal(tile3, actual.To);
                Assert.Equal(2, actual.Distance);
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
                Assert.Equal(new [] { relation1to2, relation2to3, relation3to4 }, actual.Relations);
                Assert.Equal(new [] { tile1, tile2, tile3, tile4 }, actual.Tiles);
                Assert.Equal(new [] { Direction.Clockwise, Direction.Clockwise, Direction.Clockwise }, actual.Directions);
                Assert.Equal(tile1, actual.From);
                Assert.Equal(tile4, actual.To);
                Assert.Equal(3, actual.Distance);
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
                Assert.Equal(new [] { relation1to2 }, actual.Relations);
                Assert.Equal(new [] { tile1, tile2 }, actual.Tiles);
                Assert.Equal(new [] { Direction.Clockwise }, actual.Directions);
                Assert.Equal(tile1, actual.From);
                Assert.Equal(tile2, actual.To);
                Assert.Equal(1, actual.Distance);
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
                Assert.Equal(new [] { relation1to2, relation2to3 }, actual.Relations);
                Assert.Equal(new [] { tile1, tile2, tile3 }, actual.Tiles);
                Assert.Equal(new [] { Direction.Clockwise, Direction.Clockwise }, actual.Directions);
                Assert.Equal(tile1, actual.From);
                Assert.Equal(tile3, actual.To);
                Assert.Equal(2, actual.Distance);
            }
        }
    }
}