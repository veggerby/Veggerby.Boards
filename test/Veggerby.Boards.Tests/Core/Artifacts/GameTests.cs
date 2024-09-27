using System;
using System.Linq;

using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.Artifacts
{
    public class GameTests
    {
        public class Constructor
        {
            [Fact]
            public void Should_initialize_properties()
            {
                // arrange
                var board = new TestBoard();
                var player1 = new Player("player1");
                var player2 = new Player("player2");
                var piece1 = new Piece("piece1", player1, [new DirectionPattern(Direction.North)]);
                var piece2 = new Piece("piece2", player2, [new DirectionPattern(Direction.North)]);

                // act
                var actual = new Game(board, [player1, player2], [piece1, piece2]);

                // assert
                actual.Board.Should().Be(board);
                actual.Artifacts.Should().Equal([piece1, piece2]);
                actual.Players.Should().Equal([player1, player2]);
            }

            [Fact]
            public void Should_throw_when_null_board_is_specified()
            {
                // arrange
                var player1 = new Player("player1");
                var player2 = new Player("player2");
                var piece1 = new Piece("piece1", player1, [new DirectionPattern(Direction.North)]);
                var piece2 = new Piece("piece2", player2, [new DirectionPattern(Direction.North)]);

                // act
                var actual = () => new Game(null, [player1, player2], [piece1, piece2]);

                // assert
                actual.Should().Throw<ArgumentNullException>().WithParameterName("board");
            }

            [Fact]
            public void Should_throw_when_null_players_are_specified()
            {
                // arrange
                var board = new TestBoard();
                var player1 = new Player("player1");
                var player2 = new Player("player2");
                var piece1 = new Piece("piece1", player1, [new DirectionPattern(Direction.North)]);
                var piece2 = new Piece("piece2", player2, [new DirectionPattern(Direction.North)]);

                // act
                var actual = () => new Game(board, null, [piece1, piece2]);

                // assert
                actual.Should().Throw<ArgumentNullException>().WithParameterName("players");
            }

            [Fact]
            public void Should_throw_when_empty_players_are_specified()
            {
                // arrange
                var board = new TestBoard();
                var player1 = new Player("player1");
                var player2 = new Player("player2");
                var piece1 = new Piece("piece1", player1, [new DirectionPattern(Direction.North)]);
                var piece2 = new Piece("piece2", player2, [new DirectionPattern(Direction.North)]);

                // act
                var actual = () => new Game(board, Enumerable.Empty<Player>(), [piece1, piece2]);

                // assert
                actual.Should().Throw<ArgumentException>().WithParameterName("players");
            }

            [Fact]
            public void Should_throw_when_null_pieces_are_specified()
            {
                // arrange
                var board = new TestBoard();
                var player1 = new Player("player1");
                var player2 = new Player("player2");

                // act
                var actual = () => new Game(board, [player1, player2], null);

                // assert
                actual.Should().Throw<ArgumentNullException>().WithParameterName("artifacts");
            }

            [Fact]
            public void Should_throw_when_empty_pieces_are_specified()
            {
                // arrange
                var board = new TestBoard();
                var player1 = new Player("player1");
                var player2 = new Player("player2");

                // act
                var actual = () => new Game(board, [player1, player2], Enumerable.Empty<Piece>());

                // assert
                actual.Should().Throw<ArgumentException>().WithParameterName("artifacts");
            }
        }
    }
}