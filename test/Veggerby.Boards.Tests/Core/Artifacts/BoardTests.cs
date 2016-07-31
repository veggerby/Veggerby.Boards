using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts
{
    public class BoardTests
    {
        public class Constructor
        {
            [Fact]
            public void Should_initialize_properties()
            {
                // arrange
                var tile1 = new Tile("tile1");
                var tile2 = new Tile("tile2");
                var relation = new TileRelation(tile1, tile2, Direction.Right);

                // act
                var actual = new Board("board", new[] { relation });
                
                // assert
                Assert.Equal("board", actual.Id);
                Assert.Equal(new [] { tile1, tile2 }, actual.Tiles);
                Assert.Equal(new [] { relation }, actual.TileRelations);
            }
        }

        public class GetTile 
        {
            [Fact]
            public void Should_return_correct_tile()
            {
                // arrange
                var board = new TestBoard();
                
                // act
                var actual = board.GetTile("tile-1");
                
                // assert
                Assert.Equal("tile-1", actual.Id);
            }

            [Fact]
            public void Should_return_null_for_non_existing_tile()
            {
                // arrange
                var board = new TestBoard();
                
                // act
                var actual = board.GetTile("unknown_tile");
                
                // assert
                Assert.Null(actual);
            }

            [Theory]
            [InlineData("")]
            [InlineData(null)]
            public void Should_throw_with_null_or_empty(string id)
            {
                // arrange
                var board = new TestBoard();
                
                // act + assert
                var actual = Assert.Throws<ArgumentException>(() => board.GetTile(id));
                
                // assert
                Assert.Equal("tileId", actual.ParamName);
            }
        }

        public class GetTileRelation_Tile_Direction
        {
            [Fact]
            public void Should_return_correct_tile_relation()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");

                // act
                var actual = board.GetTileRelation(from, Direction.Clockwise);
                
                // assert
                Assert.Equal(from, actual.From);
                Assert.Equal(Direction.Clockwise, actual.Direction);
            }

            [Fact]
            public void Should_return_null_for_non_existing_tile_relation()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");

                // act
                var actual = board.GetTileRelation(from, Direction.North);
                
                // assert
                Assert.Null(actual);
            }

            [Theory]
            [InlineData("tile-1", null, "direction")]
            [InlineData(null, "clockwise", "from")]
            [InlineData(null, null, "from")]
            public void Should_throw_with_null_or_empty(string fromId, string directionId, string expected)
            {
                // arrange
                var board = new TestBoard();
                var from = fromId != null ? board.GetTile(fromId) : null;
                var direction = directionId != null ? new Direction(directionId) : null;
                
                // act + assert
                var actual = Assert.Throws<ArgumentNullException>(() => board.GetTileRelation(from, direction));
                
                // assert
                Assert.Equal(expected, actual.ParamName);
            }
        }

        public class GetTileRelation_Tile_Tile
        {
            [Fact]
            public void Should_return_correct_tile_relation()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-2");

                // act
                var actual = board.GetTileRelation(from, to);
                
                // assert
                Assert.Equal(from, actual.From);
                Assert.Equal(to, actual.To);
            }

            [Fact]
            public void Should_return_null_for_non_existing_tile_relation()
            {
                // arrange
                var board = new TestBoard();
                var from = board.GetTile("tile-1");
                var to = board.GetTile("tile-10");

                // act
                var actual = board.GetTileRelation(from, to);
                
                // assert
                Assert.Null(actual);
            }

            [Theory]
            [InlineData("tile-1", null, "to")]
            [InlineData(null, "tile-1", "from")]
            [InlineData(null, null, "from")]
            public void Should_throw_with_null_or_empty(string fromId, string toId, string expected)
            {
                // arrange
                var board = new TestBoard();
                var from = fromId != null ? board.GetTile(fromId) : null;
                var to = toId != null ? board.GetTile(toId) : null;
                
                // act + assert
                var actual = Assert.Throws<ArgumentNullException>(() => board.GetTileRelation(from, to));
                
                // assert
                Assert.Equal(expected, actual.ParamName);
            }
        }
    }
}