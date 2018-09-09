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
        public class _Equals
        {
            [Fact]
            public void Should_equal_self()
            {
                // arrange
                var piece = new Piece("piece", null, null);
                var tile = new Tile("tile");
                var state = new PieceState(piece, tile);

                // act
                var actual = state.Equals(state);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_null()
            {
                // arrange
                var piece = new Piece("piece", null, null);
                var tile = new Tile("tile");
                var state = new PieceState(piece, tile);

                // act
                var actual = state.Equals(null);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_equal_other_null_state_same_artifact()
            {
                // arrange
                var piece = new Piece("piece", null, null);
                var tile = new Tile("tile");
                var state1 = new PieceState(piece, tile);
                var state2 = new PieceState(piece, tile);

                // act
                var actual = state1.Equals(state2);

                // assert
                actual.ShouldBeTrue();
            }

            [Fact]
            public void Should_not_equal_different_artifacts()
            {
                // arrange
                var piece1 = new Piece("piece-1", null, null);
                var piece2 = new Piece("piece-2", null, null);
                var tile = new Tile("tile");
                var state1 = new PieceState(piece1, tile);
                var state2 = new PieceState(piece2, tile);

                // act
                var actual = state1.Equals(state2);

                // assert
                actual.ShouldBeFalse();
            }


            [Fact]
            public void Should_not_equal_different_tile()
            {
                // arrange
                var piece = new Piece("piece", null, null);
                var tile1 = new Tile("tile-1");
                var tile2 = new Tile("tile-2");
                var state1 = new PieceState(piece, tile1);
                var state2 = new PieceState(piece, tile2);

                // act
                var actual = state1.Equals(state2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_different_artifact_state_type()
            {
                // arrange
                var tile = new Tile("tile");
                var dice = new Dice("dice");
                var piece = new Piece("piece", null, null);
                var state1 = new PieceState(piece, tile);
                var state2 = new NullDiceState(dice);

                // act
                var actual = state1.Equals(state2);

                // assert
                actual.ShouldBeFalse();
            }

            [Fact]
            public void Should_not_equal_another_type()
            {
                // arrange
                var piece = new Piece("piece", null, null);
                var tile = new Tile("tile");
                var state = new PieceState(piece, tile);

                // act
                var actual = state.Equals("a string");

                // assert
                actual.ShouldBeFalse();
            }
        }

        public class _GetHashCode
        {
            [Fact]
            public void Should_equal_self()
            {
                // arrange
                var piece = new Piece("piece", null, null);
                var tile = new Tile("tile");
                var expected = ((typeof(PieceState).GetHashCode() * 397) ^ piece.GetHashCode()) * 397 ^ tile.GetHashCode();
                var state = new PieceState(piece, tile);

                // act
                var actual = state.GetHashCode();

                // assert
                actual.ShouldBe(expected);
            }
        }
    }
}