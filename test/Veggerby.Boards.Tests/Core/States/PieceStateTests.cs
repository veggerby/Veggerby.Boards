using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.States;

public class PieceStateTests
{
    public class Create
    {
        [Fact]
        public void Should_create_piece_state()
        {
            // arrange

            // act

            // assert

            var owner = new Player("owner");
            var piece = new Piece("piece", owner, new[] { new AnyPattern() });
            var tile = new Tile("tile");

            // act
            var actual = new PieceState(piece, tile);

            // assert
            actual.Artifact.Should().Be(piece);
            actual.CurrentTile.Should().Be(tile);
        }

        [Fact]
        public void Should_throw_when_null_piece()
        {
            // arrange

            // act

            // assert

            var tile = new Tile("tile");

            // act
            var actual = () => new PieceState(null!, tile);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("artifact");
        }

        [Fact]
        public void Should_throw_when_null_tile()
        {
            // arrange

            // act

            // assert

            var owner = new Player("owner");
            var piece = new Piece("piece", owner, new[] { new AnyPattern() });

            // act
            var actual = () => new PieceState(piece, null!);

            // assert
            actual.Should().Throw<ArgumentNullException>().WithParameterName("currentTile");
        }
    }
    public class _Equals
    {
        [Fact]
        public void Should_equal_self()
        {
            // arrange

            // act

            // assert

            var owner = new Player("owner");
            var piece = new Piece("piece", owner, Array.Empty<IPattern>());
            var tile = new Tile("tile");
            var state = new PieceState(piece, tile);

            // act
            var actual = state.Equals(state);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_null()
        {
            // arrange

            // act

            // assert

            var owner = new Player("owner");
            var piece = new Piece("piece", owner, Array.Empty<IPattern>());
            var tile = new Tile("tile");
            var state = new PieceState(piece, tile);

            // act
            var actual = state.Equals(null);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_equal_other_null_state_same_artifact()
        {
            // arrange

            // act

            // assert

            var owner = new Player("owner");
            var piece = new Piece("piece", owner, Array.Empty<IPattern>());
            var tile = new Tile("tile");
            var state1 = new PieceState(piece, tile);
            var state2 = new PieceState(piece, tile);

            // act
            var actual = state1.Equals(state2);

            // assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void Should_not_equal_different_artifacts()
        {
            // arrange

            // act

            // assert

            var owner = new Player("owner");
            var piece1 = new Piece("piece-1", owner, Array.Empty<IPattern>());
            var piece2 = new Piece("piece-2", owner, Array.Empty<IPattern>());
            var tile = new Tile("tile");
            var state1 = new PieceState(piece1, tile);
            var state2 = new PieceState(piece2, tile);

            // act
            var actual = state1.Equals(state2);

            // assert
            actual.Should().BeFalse();
        }


        [Fact]
        public void Should_not_equal_different_tile()
        {
            // arrange

            // act

            // assert

            var owner = new Player("owner");
            var piece = new Piece("piece", owner, Array.Empty<IPattern>());
            var tile1 = new Tile("tile-1");
            var tile2 = new Tile("tile-2");
            var state1 = new PieceState(piece, tile1);
            var state2 = new PieceState(piece, tile2);

            // act
            var actual = state1.Equals(state2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_different_artifact_state_type()
        {
            // arrange

            // act

            // assert

            var tile = new Tile("tile");
            var dice = new Dice("dice");
            var owner = new Player("owner");
            var piece = new Piece("piece", owner, Array.Empty<IPattern>());
            var state1 = new PieceState(piece, tile);
            var state2 = new NullDiceState(dice);

            // act
            var actual = state1.Equals(state2);

            // assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void Should_not_equal_another_type()
        {
            // arrange

            // act

            // assert

            var owner = new Player("owner");
            var piece = new Piece("piece", owner, Array.Empty<IPattern>());
            var tile = new Tile("tile");
            var state = new PieceState(piece, tile);

            // act
            var actual = state.Equals("a string");

            // assert
            actual.Should().BeFalse();
        }
    }
}
