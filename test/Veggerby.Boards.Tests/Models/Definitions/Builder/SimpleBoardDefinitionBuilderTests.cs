using System.Linq;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Models.Definitions.Builder
{
    public class SimpleBoardDefinitionBuilderTests
    {
        public class Compile
        {
            [Fact]
            public void Should_return_correct_tile_definitions()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();

                // act
                var boardDefinition = builder.Compile();

                // should be 1->2->3 - clockwise
                // and 3->2->1 - counter clock wise

                var tile1 = boardDefinition.GetTile("tile1");
                var tile2 = boardDefinition.GetTile("tile2");
                var tile3 = boardDefinition.GetTile("tile3");
                var foo = boardDefinition.GetTile("foo");

                var relation12 = boardDefinition.GetTileRelation("tile1", "tile2");
                var relation23 = boardDefinition.GetTileRelation("tile2", "tile3");
                var relation32 = boardDefinition.GetTileRelation("tile3", "tile2");
                var relation21 = boardDefinition.GetTileRelation("tile2", "tile1");
                var bar = boardDefinition.GetTileRelation("foo", "bar");

                // assert
                Assert.Equal("board", boardDefinition.BoardId);

                Assert.Equal("tile1", tile1.TileId);
                Assert.Equal("tile2", tile2.TileId);
                Assert.Equal("tile3", tile3.TileId);
                Assert.Null(foo);

                // 1-2
                Assert.Same(tile1, relation12.SourceTile);
                Assert.Same(tile2, relation12.DestinationTile);
                Assert.Equal("clockWise", relation12.Direction.DirectionId);
                Assert.Equal(1, relation12.Distance);

                // 2-3
                Assert.Same(tile2, relation23.SourceTile);
                Assert.Same(tile3, relation23.DestinationTile);
                Assert.Equal("clockWise", relation23.Direction.DirectionId);
                Assert.Equal(1, relation23.Distance);

                // 3-2
                Assert.Same(tile3, relation32.SourceTile);
                Assert.Same(tile2, relation32.DestinationTile);
                Assert.Equal("counterClockWise", relation32.Direction.DirectionId);
                Assert.Equal(2, relation32.Distance);

                // 2-1
                Assert.Same(tile2, relation21.SourceTile);
                Assert.Same(tile1, relation21.DestinationTile);
                Assert.Equal("counterClockWise", relation21.Direction.DirectionId);
                Assert.Equal(2, relation21.Distance);

                Assert.Null(bar);
            }

            [Fact]
            public void Should_return_correct_piece_definitions()
            {
                // arrange
                var builder = new SimpleBoardDefinitionBuilder();

                // act
                var boardDefinition = builder.Compile();
                var white = boardDefinition.GetPiece("white");
                var black = boardDefinition.GetPiece("black");
                var bogus = boardDefinition.GetPiece("bogus");

                // assert
                Assert.Equal("white", white.PieceId);
                Assert.Equal("black", black.PieceId);
                Assert.Null(bogus);
            }
        }
    }
}
