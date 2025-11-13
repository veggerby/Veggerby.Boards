using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.TestHelpers;

using static Veggerby.Boards.Chess.ChessIds.Pieces;
using static Veggerby.Boards.Chess.ChessIds.Tiles;

namespace Veggerby.Boards.Tests.Chess.MoveGeneration;

public class ChessPawnMoveGenerationTests
{
    [Fact]
    public void GivenPawnOnStartingRank_WhenGeneratingMoves_ThenOneAndTwoStepMovesIncluded()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        var state = progress.State;
        var generator = new ChessMoveGenerator(game);

        // act
        var moves = generator.Generate(state);
        var e2Pawn = game.GetPiece(WhitePawn5).EnsureNotNull();
        var e2Moves = moves.Where(m => m.Piece == e2Pawn).ToList();

        // assert
        e2Moves.Count.Should().Be(2, "Pawn on e2 should have 2 moves (e3 and e4)");
        e2Moves.Should().Contain(m => m.To.Id == E3);
        e2Moves.Should().Contain(m => m.To.Id == E4);
    }

    [Fact]
    public void GivenPawnAfterSingleMove_WhenGeneratingMoves_ThenOnlyOneStepAvailable()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        progress = progress.Move(WhitePawn5, "e3");
        progress = progress.Move(BlackPawn5, "e6");
        var game = progress.Game;
        var state = progress.State;
        var generator = new ChessMoveGenerator(game);

        // act
        var moves = generator.Generate(state);
        var e3Pawn = game.GetPiece(WhitePawn5).EnsureNotNull();
        var e3Moves = moves.Where(m => m.Piece == e3Pawn).ToList();

        // assert
        e3Moves.Count.Should().Be(1, "Pawn on e3 should have only 1 move (e4)");
        e3Moves.Should().Contain(m => m.To.Id == E4);
    }

    [Fact]
    public void GivenBlockedPawn_WhenGeneratingMoves_ThenNoMovesGenerated()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        progress = progress.Move(WhitePawn5, "e4");
        progress = progress.Move(BlackPawn5, "e5");
        var game = progress.Game;
        var state = progress.State;
        var generator = new ChessMoveGenerator(game);

        // act
        var moves = generator.Generate(state);
        var e4Pawn = game.GetPiece(WhitePawn5).EnsureNotNull();
        var e4Moves = moves.Where(m => m.Piece == e4Pawn).ToList();

        // assert
        e4Moves.Should().BeEmpty("Pawn on e4 blocked by pawn on e5 should have no moves");
    }
}
