using System.Linq;
using Shouldly;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Utils;
using Xunit;

namespace Veggerby.Boards.Tests.Chess
{
    public class ChessInitialGameStateBuilderTests
    {
        [Fact]
        public void Should_initialize_game_state()
        {
            // arrange
            var chess = new ChessGameBuilder().Compile();

            // act
            var actual = new ChessInitialGameStateBuilder().Compile(chess);

            // assert
            actual.ShouldHavePieceState("white-rook-1", "tile-a1");
            actual.ShouldHavePieceState("white-knight-1", "tile-b1");
            actual.ShouldHavePieceState("white-bishop-1", "tile-c1");
            actual.ShouldHavePieceState("white-king", "tile-d1");
            actual.ShouldHavePieceState("white-queen", "tile-e1");
            actual.ShouldHavePieceState("white-bishop-2", "tile-f1");
            actual.ShouldHavePieceState("white-knight-2", "tile-g1");
            actual.ShouldHavePieceState("white-rook-2", "tile-h1");

            actual.ShouldHavePieceState("white-pawn-1", "tile-a2");
            actual.ShouldHavePieceState("white-pawn-2", "tile-b2");
            actual.ShouldHavePieceState("white-pawn-3", "tile-c2");
            actual.ShouldHavePieceState("white-pawn-4", "tile-d2");
            actual.ShouldHavePieceState("white-pawn-5", "tile-e2");
            actual.ShouldHavePieceState("white-pawn-6", "tile-f2");
            actual.ShouldHavePieceState("white-pawn-7", "tile-g2");
            actual.ShouldHavePieceState("white-pawn-8", "tile-h2");

            actual.ShouldHavePieceState("black-pawn-1", "tile-a7");
            actual.ShouldHavePieceState("black-pawn-2", "tile-b7");
            actual.ShouldHavePieceState("black-pawn-3", "tile-c7");
            actual.ShouldHavePieceState("black-pawn-4", "tile-d7");
            actual.ShouldHavePieceState("black-pawn-5", "tile-e7");
            actual.ShouldHavePieceState("black-pawn-6", "tile-f7");
            actual.ShouldHavePieceState("black-pawn-7", "tile-g7");
            actual.ShouldHavePieceState("black-pawn-8", "tile-h7");

            actual.ShouldHavePieceState("black-rook-1", "tile-a8");
            actual.ShouldHavePieceState("black-knight-1", "tile-b8");
            actual.ShouldHavePieceState("black-bishop-1", "tile-c8");
            actual.ShouldHavePieceState("black-king", "tile-d8");
            actual.ShouldHavePieceState("black-queen", "tile-e8");
            actual.ShouldHavePieceState("black-bishop-2", "tile-f8");
            actual.ShouldHavePieceState("black-knight-2", "tile-g8");
            actual.ShouldHavePieceState("black-rook-2", "tile-h8");
        }
    }
}