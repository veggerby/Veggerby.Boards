using System;
using System.Linq;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

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
                var piece1 = new Piece("piece1", player1, new [] { new DirectionPattern(Direction.North) });
                var piece2 = new Piece("piece2", player2, new [] { new DirectionPattern(Direction.North) });

                // act
                var actual = new Game(board, new [] { player1, player2 }, new [] { piece1, piece2 });

                // assert
                actual.Board.ShouldBe(board);
                actual.Pieces.ShouldBe(new [] { piece1, piece2 });
                actual.Players.ShouldBe(new [] { player1, player2 });
            }

            [Fact]
            public void Should_throw_when_null_board_is_specified()
            {
                // arrange
                var player1 = new Player("player1");
                var player2 = new Player("player2");
                var piece1 = new Piece("piece1", player1, new [] { new DirectionPattern(Direction.North) });
                var piece2 = new Piece("piece2", player2, new [] { new DirectionPattern(Direction.North) });

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new Game(null, new [] { player1, player2 }, new [] { piece1, piece2 }));

                // assert
                actual.ParamName.ShouldBe("board");
            }

            [Fact]
            public void Should_throw_when_null_players_are_specified()
            {
                // arrange
                var board = new TestBoard();
                var player1 = new Player("player1");
                var player2 = new Player("player2");
                var piece1 = new Piece("piece1", player1, new [] { new DirectionPattern(Direction.North) });
                var piece2 = new Piece("piece2", player2, new [] { new DirectionPattern(Direction.North) });

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new Game(board, null, new [] { piece1, piece2 }));

                // assert
                actual.ParamName.ShouldBe("players");
            }

            [Fact]
            public void Should_throw_when_empty_players_are_specified()
            {
                // arrange
                var board = new TestBoard();
                var player1 = new Player("player1");
                var player2 = new Player("player2");
                var piece1 = new Piece("piece1", player1, new [] { new DirectionPattern(Direction.North) });
                var piece2 = new Piece("piece2", player2, new [] { new DirectionPattern(Direction.North) });

                // act
                var actual = Should.Throw<ArgumentException>(() => new Game(board, Enumerable.Empty<Player>(), new [] { piece1, piece2 }));

                // assert
                actual.ParamName.ShouldBe("players");
            }

            [Fact]
            public void Should_throw_when_null_pieces_are_specified()
            {
                // arrange
                var board = new TestBoard();
                var player1 = new Player("player1");
                var player2 = new Player("player2");

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new Game(board, new [] { player1, player2 }, null));

                // assert
                actual.ParamName.ShouldBe("pieces");
            }

            [Fact]
            public void Should_throw_when_empty_pieces_are_specified()
            {
                // arrange
                var board = new TestBoard();
                var player1 = new Player("player1");
                var player2 = new Player("player2");

                // act
                var actual = Should.Throw<ArgumentException>(() => new Game(board, new [] { player1, player2 }, Enumerable.Empty<Piece>()));

                // assert
                actual.ParamName.ShouldBe("pieces");
            }
        }
    }
}