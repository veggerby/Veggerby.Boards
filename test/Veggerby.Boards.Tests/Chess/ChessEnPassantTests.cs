using AwesomeAssertions;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Chess;

public class ChessEnPassantTests
{
    [Fact]
    public void GivenWhiteDoubleStep_WhenBlackCapturesEnPassant_ThenWhitePawnCapturedAndTargetCleared()
    {
        // arrange (minimal scenario: white pawn e2, black pawn d4)
        var progress = new Tests.Chess.Support.EnPassantScenarioBuilder().Compile();
        var extras0 = progress.State.GetExtras<ChessStateExtras>();
        extras0.EnPassantTargetTileId.Should().BeNull();

        // act1: white double-step e2->e4 sets target e3
        progress = progress.Move("white-pawn-test", "e4");
        var extrasAfterDouble = progress.State.GetExtras<ChessStateExtras>();
        extrasAfterDouble.EnPassantTargetTileId.Should().Be(ChessIds.Tiles.E3);

        // act2: black captures en-passant d4xe3 (south-east one step)
        progress = progress.Move("black-pawn-test", "e3");

        // assert capture + target cleared
        var extrasAfterCapture = progress.State.GetExtras<ChessStateExtras>();
        extrasAfterCapture.EnPassantTargetTileId.Should().BeNull();
        var blackPawn = progress.Game.GetPiece("black-pawn-test");
        progress.State.GetState<PieceState>(blackPawn).CurrentTile.Id.Should().Be(ChessIds.Tiles.E3);
        var whitePawn = progress.Game.GetPiece("white-pawn-test");
        progress.State.GetCapturedState(whitePawn).Should().NotBeNull();
    }

    [Fact]
    public void GivenWhiteDoubleStep_WhenBlackMakesOtherMove_ThenEnPassantTargetCleared()
    {
        // arrange (include auxiliary pawn so black has alternative move) white pawn e2, black pawn d4, black aux h7
        var progress = new Tests.Chess.Support.EnPassantScenarioBuilder(includeAuxiliaryBlackPawn: true).Compile();
        // white double step sets target
        progress = progress.Move("white-pawn-test", "e4");
        progress.State.GetExtras<ChessStateExtras>().EnPassantTargetTileId.Should().Be(ChessIds.Tiles.E3);

        // act: black moves auxiliary pawn h7->h6 (non-en-passant)
        progress = progress.Move("black-pawn-aux", "h6");

        // assert: target cleared, white double-step pawn still on e4
        progress.State.GetExtras<ChessStateExtras>().EnPassantTargetTileId.Should().BeNull();
        var whitePawn = progress.Game.GetPiece("white-pawn-test");
        progress.State.GetState<PieceState>(whitePawn).CurrentTile.Id.Should().Be(ChessIds.Tiles.E4);
    }
    [Fact]
    public void GivenInitialPosition_WhenWhitePawnDoubleSteps_ThenEnPassantTargetSet()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var extrasBefore = progress.State.GetExtras<ChessStateExtras>();
        extrasBefore.EnPassantTargetTileId.Should().BeNull();

        // act (white pawn e2 -> e4)
        progress = progress.Move("white-pawn-5", "e4");

        // assert
        var extrasAfter = progress.State.GetExtras<ChessStateExtras>();
        extrasAfter.EnPassantTargetTileId.Should().Be(ChessIds.Tiles.E3);
    }

    [Fact(Skip = "Replaced by canonical scenario: black captures white double-step using dedicated scenario builder")]
    public void GivenWhiteDoubleStep_WhenBlackExecutesEnPassant_ThenWhitePawnCapturedAndTargetCleared()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        // white: e2->e4
        progress = progress.Move("white-pawn-5", "e4");
        // black: d7->d5 (sets target tile-d6)
        progress = progress.Move("black-pawn-4", "d5");
        var extrasMid = progress.State.GetExtras<ChessStateExtras>();
        extrasMid.EnPassantTargetTileId.Should().Be(ChessIds.Tiles.D6);

        // act (white pawn captures en-passant e4xd5 onto d6)
        progress = progress.Move("white-pawn-5", "d6");

        // assert
        var extrasAfter = progress.State.GetExtras<ChessStateExtras>();
        extrasAfter.EnPassantTargetTileId.Should().BeNull();

        var whitePawn = progress.Game.GetPiece("white-pawn-5");
        progress.State.GetState<PieceState>(whitePawn).CurrentTile.Id.Should().Be(ChessIds.Tiles.D6);

        var blackPawn = progress.Game.GetPiece("black-pawn-4");
        progress.State.GetCapturedState(blackPawn).Should().NotBeNull();
    }

    [Fact]
    public void GivenDoubleStep_WhenIntermediateMoveOccurs_ThenEnPassantTargetClearedAndCaptureInvalid()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        // white: e2->e4
        progress = progress.Move("white-pawn-5", "e4");
        // black: a7->a6 (clears any en-passant opportunity before d-pawn moves)
        progress = progress.Move("black-pawn-1", "a6");
        var extrasMid = progress.State.GetExtras<ChessStateExtras>();
        extrasMid.EnPassantTargetTileId.Should().BeNull();

        // act attempt (white pawn tries en-passant style move anyway e4->d5)
        var before = progress.State;
        progress = progress.Move("white-pawn-5", "d5");

        // assert (should be ignored because no target & destination empty diagonal not allowed)
        progress.State.Should().Be(before);
    }
}