using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Flows.Events
{
    public class MovePieceGameEventTests
    {
        public class ctor
        {
            [Fact]
            public void Should_create_event()
            {
                // arrange
                var piece = new Piece("piece", null, new [] { new DirectionPattern(Direction.Clockwise, true) });
                var from = new Tile("tile-1");
                var to = new Tile("tile-2");

                // act
                var actual = new MovePieceGameEvent(piece, from, to);

                // assert
                actual.Piece.ShouldBe(piece);
                actual.From.ShouldBe(from);
                actual.To.ShouldBe(to);
            }

            [Fact]
            public void Should_throw_with_null_piece()
            {
                // arrange
                var from = new Tile("tile-1");
                var to = new Tile("tile-2");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new MovePieceGameEvent(null, from, to));

                // assert
                actual.ParamName.ShouldBe("piece");
            }

            [Fact]
            public void Should_throw_with_null_from_tile()
            {
                // arrange
                var piece = new Piece("piece", null, new [] { new DirectionPattern(Direction.Clockwise, true) });
                var to = new Tile("tile-2");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new MovePieceGameEvent(piece, null, to));

                // assert
                actual.ParamName.ShouldBe("from");
            }

            [Fact]
            public void Should_throw_with_null_to_tile()
            {
                // arrange
                var piece = new Piece("piece", null, new [] { new DirectionPattern(Direction.Clockwise, true) });
                var from = new Tile("tile-1");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new MovePieceGameEvent(piece, from, null));

                // assert
                actual.ParamName.ShouldBe("to");
            }
        }
    }
}