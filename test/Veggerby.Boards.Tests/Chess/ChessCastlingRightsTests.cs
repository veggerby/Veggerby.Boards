using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Verifies castling rights revocation semantics for independent rook movement (non-castling moves).
/// </summary>
public class ChessCastlingRightsTests
{
    [Fact]
    public void GivenInitialPosition_WhenWhiteMovesKingsideRook_ThenOnlyKingSideRightRevoked()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var extrasBefore = progress.State.GetExtras<ChessStateExtras>();
        extrasBefore.WhiteCanCastleKingSide.Should().BeTrue();
        extrasBefore.WhiteCanCastleQueenSide.Should().BeTrue();

        // Path initially blocked by own pawn at h2; clear it first and alternate turns.
        progress = progress.Move("white-pawn-8", "h4"); // clears h2 & h3
        progress = progress.Move("black-pawn-5", "e6"); // black reply
        progress = progress.Move("white-rook-2", "h2"); // rook moves from original square

        // assert
        var extras = progress.State.GetExtras<ChessStateExtras>();
        extras.WhiteCanCastleKingSide.Should().BeFalse();
        extras.WhiteCanCastleQueenSide.Should().BeTrue();
        extras.BlackCanCastleKingSide.Should().BeTrue();
        extras.BlackCanCastleQueenSide.Should().BeTrue();
    }

    [Fact]
    public void GivenInitialPosition_WhenWhiteMovesQueensideRook_ThenOnlyQueenSideRightRevoked()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        // Clear path: pawn a2 forward double; alternate turn; then move rook a1->a2
        progress = progress.Move("white-pawn-1", "a4");
        progress = progress.Move("black-pawn-5", "e6");
        progress = progress.Move("white-rook-1", "a2");

        // assert
        var extras = progress.State.GetExtras<ChessStateExtras>();
        extras.WhiteCanCastleQueenSide.Should().BeFalse();
        extras.WhiteCanCastleKingSide.Should().BeTrue();
    }

    [Fact]
    public void GivenBothRooksMoved_WhenWhiteAttemptsCastling_ThenInvalid()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        // Clear both rook paths and move each rook from its starting square.
        progress = progress.Move("white-pawn-8", "h4"); // clear kingside
        progress = progress.Move("black-pawn-5", "e6");
        progress = progress.Move("white-rook-2", "h2"); // move kingside rook
        progress = progress.Move("black-pawn-4", "d6");
        progress = progress.Move("white-pawn-1", "a4"); // clear queenside
        progress = progress.Move("black-pawn-3", "c6");
        progress = progress.Move("white-rook-1", "a2"); // move queenside rook
        progress = progress.Move("black-pawn-6", "f6"); // ensure turn returns to white for castle attempt

        // act
        var exKingSide = Record.Exception(() => progress = progress.Castle(ChessIds.Players.White, kingSide: true));
        var exQueenSide = Record.Exception(() => progress = progress.Castle(ChessIds.Players.White, kingSide: false));

        // assert
        exKingSide.Should().NotBeNull();
        exQueenSide.Should().NotBeNull();
    }
}