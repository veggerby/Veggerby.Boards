using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts
{
    public class GameExtensionsTests
    {
        public class GetPiece
        {
            [Fact]
            public void Should_get_piece()
            {
                // arrange
                var piece1 = new Piece("piece-1", null, new [] { new NullPattern() });
                var piece2 = new Piece("piece-2", null, new [] { new NullPattern() });
                var piece3 = new Piece("piece-3", null, new [] { new NullPattern() });

                var board = new TestBoard();
                var game = new Game(
                    "id", 
                    board, 
                    new [] { new Player ("player") }, 
                    new [] { piece1, piece2, piece3 });

                // act
                var actual = game.GetPiece("piece-1");
                
                // assert
                actual.ShouldBe(piece1);
            }

            [Fact]
            public void Should_return_null_for_non_existing_piece()
            {
                // arrange
                var piece1 = new Piece("piece-1", null, new [] { new NullPattern() });
                var piece2 = new Piece("piece-2", null, new [] { new NullPattern() });
                var piece3 = new Piece("piece-3", null, new [] { new NullPattern() });

                var board = new TestBoard();
                var game = new Game(
                    "id", 
                    board, 
                    new [] { new Player ("player") }, 
                    new [] { piece1, piece2, piece3 });

                // act
                var actual = game.GetPiece("piece-4");
                
                // assert
                actual.ShouldBeNull();
            }
        }

        public class GetTile
        {
            [Fact]
            public void Should_get_tile()
            {
                // arrange
                var piece1 = new Piece("piece-1", null, new [] { new NullPattern() });
                var piece2 = new Piece("piece-2", null, new [] { new NullPattern() });
                var piece3 = new Piece("piece-3", null, new [] { new NullPattern() });

                var board = new TestBoard();
                var game = new Game(
                    "id", 
                    board, 
                    new [] { new Player ("player") }, 
                    new [] { piece1, piece2, piece3 });

                // act
                var actual = game.GetTile("tile-1");
                
                // assert
                actual.Id.ShouldBe("tile-1");
            }

            [Fact]
            public void Should_return_null_for_non_existing_tile()
            {
                // arrange
                var piece1 = new Piece("piece-1", null, new [] { new NullPattern() });
                var piece2 = new Piece("piece-2", null, new [] { new NullPattern() });
                var piece3 = new Piece("piece-3", null, new [] { new NullPattern() });

                var board = new TestBoard();
                var game = new Game(
                    "id", 
                    board, 
                    new [] { new Player ("player") }, 
                    new [] { piece1, piece2, piece3 });

                // act
                var actual = game.GetTile("not-existing");
                
                // assert
                actual.ShouldBeNull();
            }
        }

        public class FirstTurn
        {
            [Fact]
            public void Should_return_first_turn_state()
            {
                // arrange
                var player = new Player ("player");
                var piece1 = new Piece("piece-1", null, new [] { new NullPattern() });
                var piece2 = new Piece("piece-2", null, new [] { new NullPattern() });
                var piece3 = new Piece("piece-3", null, new [] { new NullPattern() });

                var board = new TestBoard();
                var game = new Game(
                    "id", 
                    board, 
                    new [] { player }, 
                    new [] { piece1, piece2, piece3 });

                // act
                var actual = game.FirstTurn();
                
                // assert
                actual.Artifact.ShouldBe(player);
                actual.Round.Number.ShouldBe(1);
                actual.Turn.Number.ShouldBe(1);
                actual.Turn.Round.ShouldBe(actual.Round);
            }
        }
    }
}