using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.Core;

public class GameExtensionsTests
{
    public class GetPiece
    {
        [Fact]
        public void Should_get_piece()
        {
            // arrange

            // act

            // assert

            var owner = new Player("player");
            var piece1 = new Piece("piece-1", owner, [new NullPattern()]);
            var piece2 = new Piece("piece-2", owner, [new NullPattern()]);
            var piece3 = new Piece("piece-3", owner, [new NullPattern()]);

            var board = new TestBoard();
            var game = new Game(
                board,
                [owner],
                [piece1, piece2, piece3]);

            // act
            var actual = game.GetPiece("piece-1");

            // assert
            actual.Should().Be(piece1);
        }

        [Fact]
        public void Should_return_null_for_non_existing_piece()
        {
            // arrange

            // act

            // assert

            var owner = new Player("player");
            var piece1 = new Piece("piece-1", owner, [new NullPattern()]);
            var piece2 = new Piece("piece-2", owner, [new NullPattern()]);
            var piece3 = new Piece("piece-3", owner, [new NullPattern()]);

            var board = new TestBoard();
            var game = new Game(
                board,
                [owner],
                [piece1, piece2, piece3]);

            // act
            var actual = game.GetPiece("piece-4");

            // assert
            actual.Should().BeNull();
        }
    }

    public class GetTile
    {
        [Fact]
        public void Should_get_tile()
        {
            // arrange

            // act

            // assert

            var owner = new Player("player");
            var piece1 = new Piece("piece-1", owner, [new NullPattern()]);
            var piece2 = new Piece("piece-2", owner, [new NullPattern()]);
            var piece3 = new Piece("piece-3", owner, [new NullPattern()]);

            var board = new TestBoard();
            var game = new Game(
                board,
                [owner],
                [piece1, piece2, piece3]);

            // act
            var actual = game.GetTile("tile-1").EnsureNotNull();

            // assert
            actual.Id.Should().Be("tile-1");
        }

        [Fact]
        public void Should_return_null_for_non_existing_tile()
        {
            // arrange

            // act

            // assert

            var owner = new Player("player");
            var piece1 = new Piece("piece-1", owner, [new NullPattern()]);
            var piece2 = new Piece("piece-2", owner, [new NullPattern()]);
            var piece3 = new Piece("piece-3", owner, [new NullPattern()]);

            var board = new TestBoard();
            var game = new Game(
                board,
                [owner],
                [piece1, piece2, piece3]);

            // act
            var actual = game.GetTile("not-existing");

            // assert
            actual.Should().BeNull();
        }
    }

    public class GetArtifact
    {
        [Fact]
        public void Should_return_artifact()
        {
            // arrange

            // act

            // assert

            var owner = new Player("player");
            var piece1 = new Piece("piece-1", owner, [new NullPattern()]);
            var piece2 = new Piece("piece-2", owner, [new NullPattern()]);
            var piece3 = new Piece("piece-3", owner, [new NullPattern()]);
            var dice1 = new Dice("dice-1");
            var dice2 = new Dice("dice-2");

            var board = new TestBoard();
            var game = new Game(
                board,
                [owner],
                [piece1, piece2, piece3, dice1, dice2]);

            // act
            var actual = game.GetArtifact<Dice>("dice-1");

            // assert
            actual.Should().Be(dice1);
        }

        [Fact]
        public void Should_not_return_artifact_not_in_list()
        {
            // arrange

            // act

            // assert

            var owner = new Player("player");
            var piece1 = new Piece("piece-1", owner, [new NullPattern()]);
            var piece2 = new Piece("piece-2", owner, [new NullPattern()]);
            var piece3 = new Piece("piece-3", owner, [new NullPattern()]);
            var dice1 = new Dice("dice-1");
            var dice2 = new Dice("dice-2");

            var board = new TestBoard();
            var game = new Game(
                board,
                [owner],
                [piece1, piece2, piece3, dice2]);

            // act
            var actual = game.GetArtifact<Dice>("dice-1");

            // assert
            actual.Should().BeNull();
        }

        [Fact]
        public void Should_not_return_artifact_incorrect_type()
        {
            // arrange

            // act

            // assert

            var owner = new Player("player");
            var piece1 = new Piece("piece-1", owner, [new NullPattern()]);
            var piece2 = new Piece("piece-2", owner, [new NullPattern()]);
            var piece3 = new Piece("piece-3", owner, [new NullPattern()]);
            var dice1 = new Dice("dice-1");
            var dice2 = new Dice("dice-2");

            var board = new TestBoard();
            var game = new Game(
                board,
                [owner],
                [piece1, piece2, piece3, dice1, dice2]);

            // act
            var actual = game.GetArtifact<Tile>("dice-1");

            // assert
            actual.Should().BeNull();
        }
    }
}
