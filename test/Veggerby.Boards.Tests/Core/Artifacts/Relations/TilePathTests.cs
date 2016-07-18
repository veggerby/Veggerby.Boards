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
    }
}