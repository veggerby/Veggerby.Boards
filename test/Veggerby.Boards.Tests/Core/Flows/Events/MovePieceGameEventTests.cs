using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Tests.Core.Flows.Events;

public class MovePieceGameEventTests
{
    public class Create
    {
        [Fact]
        public void Should_create_event()
        {
            // arrange
            var piece = new Piece("piece", null, [new DirectionPattern(Direction.Clockwise, true)]);
            var from = new Tile("tile-1");
            var to = new Tile("tile-2");
            var path = new TilePath([new TileRelation(from, to, Direction.Clockwise)]);

            // act
            var actual = new MovePieceGameEvent(piece, path);

            // assert
            actual.Piece.Should().Be(piece);
            actual.From.Should().Be(from);
            actual.To.Should().Be(to);
            actual.Distance.Should().Be(1);
            actual.Path.Should().Be(path);
        }

        [Fact]
        public void Should_throw_with_null_piece()
        {
            // arrange
            var from = new Tile("tile-1");
            var to = new Tile("tile-2");
            var path = new TilePath([new TileRelation(from, to, Direction.Clockwise)]);

            // act
            var actual = () => new MovePieceGameEvent(null, path);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("piece");
        }

        [Fact]
        public void Should_throw_with_null_path()
        {
            // arrange
            var piece = new Piece("piece", null, [new DirectionPattern(Direction.Clockwise, true)]);

            // act
            var actual = () => new MovePieceGameEvent(piece, null);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("path");
        }
    }
}