using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.Core.Artifacts;

public class BoardTests
{
    public class Constructor
    {
        [Fact]
        public void Should_initialize_properties()
        {
            // arrange

            // act

            // assert

            var tile1 = new Tile("tile1");
            var tile2 = new Tile("tile2");
            var relation = new TileRelation(tile1, tile2, Direction.Right);

            // act
            var actual = new Board("board", [relation]);

            // assert
            actual.Id.Should().Be("board");
            actual.Tiles.Should().Equal([tile1, tile2]);
            actual.TileRelations.Should().Equal([relation]);
        }

        [Fact]
        public void Should_throw_when_null_relations_are_specified()
        {
            // arrange

            // act

            // assert

            var actual = () => new Board("board", null!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("tileRelations");
        }

        [Fact]
        public void Should_throw_when_empty_relations_are_specified()
        {
            // arrange

            // act

            // assert

            var actual = () => new Board("board", Enumerable.Empty<TileRelation>());

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("tileRelations");
        }
    }

    public class GetTile
    {
        [Fact]
        public void Should_return_correct_tile()
        {
            // arrange

            // act

            // assert

            var board = new TestBoard();

            // act
            var actual = board.GetTile("tile-1").EnsureNotNull();

            // assert
            actual.Id.Should().Be("tile-1");
        }

        [Fact]
        public void Should_return_null_for_non_existing_tile()
        {
            // arrange

            // act

            // assert

            var board = new TestBoard();

            // act
            var actual = board.GetTile("unknown_tile");

            // assert
            actual.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_throw_with_null_or_empty(string? id)
        {
            // arrange
            var board = new TestBoard();

            // act + assert
            var actual = () => board.GetTile(id!);

            // assert
            actual.Should().Throw<ArgumentException>().WithParameterName("tileId");
        }
    }

    public class GetTileRelation_Tile_Direction
    {
        [Fact]
        public void Should_return_correct_tile_relation()
        {
            // arrange

            // act

            // assert

            var board = new TestBoard();
            var from = board.GetTile("tile-1").EnsureNotNull();

            // act
            var actual = board.GetTileRelation(from, Direction.Clockwise);

            // assert
            actual.Should().NotBeNull();
            actual!.From.Should().Be(from);
            actual.Direction.Should().Be(Direction.Clockwise);
        }

        [Fact]
        public void Should_return_null_for_non_existing_tile_relation()
        {
            // arrange

            // act

            // assert

            var board = new TestBoard();
            var from = board.GetTile("tile-1").EnsureNotNull();

            // act
            var actual = board.GetTileRelation(from, Direction.North);

            // assert
            actual.Should().BeNull();
        }

        [Theory]
        [InlineData("tile-1", null, "direction")]
        [InlineData(null, "clockwise", "from")]
        [InlineData(null, null, "from")]
        public void Should_throw_with_null_or_empty(string? fromId, string? directionId, string expected)
        {
            // arrange
            var board = new TestBoard();
            var from = fromId is not null ? board.GetTile(fromId).EnsureNotNull() : null;
            var direction = directionId is not null ? new Direction(directionId) : null;

            // act + assert
            var actual = () => board.GetTileRelation(from!, direction!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName(expected);
        }
    }

    public class GetTileRelation_Tile_Tile
    {
        [Fact]
        public void Should_return_correct_tile_relation()
        {
            // arrange

            // act

            // assert

            var board = new TestBoard();
            var from = board.GetTile("tile-1").EnsureNotNull();
            var to = board.GetTile("tile-2").EnsureNotNull();

            // act
            var actual = board.GetTileRelation(from, to!);

            // assert
            actual.Should().NotBeNull();
            actual!.From.Should().Be(from);
            actual.To.Should().Be(to);
        }

        [Fact]
        public void Should_return_null_for_non_existing_tile_relation()
        {
            // arrange

            // act

            // assert

            var board = new TestBoard();
            var from = board.GetTile("tile-1").EnsureNotNull();
            // select a tile that exists but has no direct relation from 'tile-1'
            // Relations defined in TestBoard only include clockwise (i -> (i+1)%16), across, and specific up/left links.
            // There is no relation from tile-1 to tile-3, so this pair should yield null.
            var to = board.GetTile("tile-3").EnsureNotNull();

            // act
            var actual = board.GetTileRelation(from, to!);

            // assert
            actual.Should().BeNull();
        }

        [Theory]
        [InlineData("tile-1", null, "to")]
        [InlineData(null, "tile-1", "from")]
        [InlineData(null, null, "from")]
        public void Should_throw_with_null_or_empty(string? fromId, string? toId, string expected)
        {
            // arrange
            var board = new TestBoard();
            var from = fromId is not null ? board.GetTile(fromId).EnsureNotNull() : null;
            var to = toId is not null ? board.GetTile(toId).EnsureNotNull() : null;

            // act + assert
            var actual = () => board.GetTileRelation(from!, to!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName(expected);
        }
    }
}
