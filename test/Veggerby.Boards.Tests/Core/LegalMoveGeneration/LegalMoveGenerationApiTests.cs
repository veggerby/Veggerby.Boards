using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.LegalMoveGeneration;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.LegalMoveGeneration;

/// <summary>
/// Tests for the core Legal Move Generation API.
/// </summary>
public class LegalMoveGenerationApiTests
{
    private static MovePieceGameEvent CreateMoveEvent(GameProgress progress, string pieceId, string toTileId)
    {
        var piece = progress.Game.GetPiece(pieceId);
        piece.Should().NotBeNull();
        var fromState = progress.State.GetState<PieceState>(piece!);
        fromState.Should().NotBeNull();
        var toTile = progress.Game.GetTile(toTileId);
        toTile.Should().NotBeNull();

        // Use pattern resolution like production code
        var path = piece.Patterns
            .Select(p =>
            {
                var v = new ResolveTilePathPatternVisitor(progress.Game.Board, fromState!.CurrentTile, toTile!);
                p.Accept(v);
                return v.ResultPath;
            })
            .FirstOrDefault(p => p is not null);

        path.Should().NotBeNull($"expected a resolvable path for test from {fromState.CurrentTile.Id} to {toTileId}");

        return new MovePieceGameEvent(piece!, path!);
    }

    [Fact]
    public void GivenGameProgress_WhenGettingLegalMoveGenerator_ThenReturnsNonNull()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();

        // act
        var generator = progress.GetLegalMoveGenerator();

        // assert
        generator.Should().NotBeNull();
        generator.Should().BeAssignableTo<ILegalMoveGenerator>();
    }

    [Fact]
    public void GivenChessMove_WhenValidatingLegalMove_ThenReturnsLegal()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var generator = progress.GetLegalMoveGenerator();

        // Create a legal move event (e2-e4 pawn advance)
        var move = CreateMoveEvent(progress, Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhitePawn5, Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E4);

        // act
        var validation = generator.Validate(move, progress.State);

        // assert
        validation.Should().NotBeNull();
        validation.IsLegal.Should().BeTrue("e2-e4 is a legal opening move");
        validation.Reason.Should().Be(RejectionReason.None);
        validation.Explanation.Should().BeEmpty();
    }

    [Fact]
    public void GivenChessMove_WhenValidatingIllegalMove_ThenReturnsIllegal()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var generator = progress.GetLegalMoveGenerator();

        // Create an illegal move (trying to move opponent's piece)
        var illegalMove = CreateMoveEvent(progress, Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackPawn5, Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E5);

        // act
        var validation = generator.Validate(illegalMove, progress.State);

        // assert
        validation.IsLegal.Should().BeFalse("Cannot move opponent's piece");
        validation.Reason.Should().NotBe(RejectionReason.None);
    }

    [Fact]
    public void GivenEndedGame_WhenValidatingMove_ThenReturnsGameEnded()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();

        // Create a legal move
        var move = CreateMoveEvent(progress, Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhitePawn5, Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E4);

        // End the game
        var endedState = progress.State.Next([new GameEndedState()]);
        var generator = progress.GetLegalMoveGenerator();

        // act
        var validation = generator.Validate(move, endedState);

        // assert
        validation.IsLegal.Should().BeFalse("Move should be illegal after game ends");
        validation.Reason.Should().Be(RejectionReason.GameEnded);
        validation.Explanation.Should().Contain("ended");
    }
}
