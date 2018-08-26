using Shouldly;
using System;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.States;
using Xunit;

namespace Veggerby.Boards.Tests.Core.States
{
    public class PieceStateTests
    {
        public class ctor
        {
            [Fact]
            public void Should_create_piece_state()
            {
                // arrange
                var piece = new Piece("piece", null, new [] { new AnyPattern() });
                var tile = new Tile("tile");

                // act
                var actual = new PieceState(piece, tile);

                // assert
                actual.Artifact.ShouldBe(piece);
                actual.CurrentTile.ShouldBe(tile);
            }

            [Fact]
            public void Should_throw_when_null_piece()
            {
                // arrange
                var tile = new Tile("tile");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new PieceState(null, tile));

                // assert
                actual.ParamName.ShouldBe("artifact");
            }

            [Fact]
            public void Should_throw_when_null_tile()
            {
                // arrange
                var piece = new Piece("piece", null, new [] { new AnyPattern() });

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new PieceState(piece, null));

                // assert
                actual.ParamName.ShouldBe("currentTile");
            }
        }
    }
}