using System;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;
using Xunit;

namespace Veggerby.Boards.Tests.Core.States
{
    public class ArtifactStateChangeTests
    {
        public class ctor
        {
            [Fact]
            public void Should_initial_state_change()
            {
                // arrange
                var piece = new Piece("piece", null, null);
                var from = new Tile("tile-from");
                var to = new Tile("tile-to");
                var stateFrom = new PieceState(piece, from);
                var stateTo = new PieceState(piece, to);

                // act
                var actual = new ArtifactStateChange(stateFrom, stateTo);

                // assert
                actual.From.ShouldBe(stateFrom);
                actual.To.ShouldBe(stateTo);
            }

            [Fact]
            public void Should_throw_with_null_states()
            {
                // arrange

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new ArtifactStateChange(null, null));

                // assert
                actual.ParamName.ShouldBe("to");
            }

            [Fact]
            public void Should_throw_with_different_artifacts()
            {
                // arrange
                var piece1 = new Piece("piece1", null, null);
                var piece2 = new Piece("piece2", null, null);
                var from = new Tile("tile-from");
                var to = new Tile("tile-to");
                var stateFrom = new PieceState(piece1, from);
                var stateTo = new PieceState(piece2, to);

                // act
                var actual = Should.Throw<ArgumentException>(() => new ArtifactStateChange(stateFrom, stateTo));

                // assert
                actual.ParamName.ShouldBe("to");
            }
        }
    }
}