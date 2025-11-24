using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.TestHelpers;

using static Veggerby.Boards.Chess.Constants.ChessIds.Pieces;
using static Veggerby.Boards.Chess.Constants.ChessIds.Tiles;

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

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        var extrasBefore = progress.State.GetRequiredExtras<ChessStateExtras>();
        extrasBefore.WhiteCanCastleKingSide.Should().BeTrue();
        extrasBefore.WhiteCanCastleQueenSide.Should().BeTrue();

        // Path initially blocked by own pawn at h2; clear it first and alternate turns.
        progress = progress.Move(WhitePawn8, H4); // clears h2 & h3
        progress = progress.Move(BlackPawn5, E6); // black reply
        progress = progress.Move(WhiteRook2, H2); // rook moves from original square

        // assert
        var extras = progress.State.GetRequiredExtras<ChessStateExtras>();
        extras.WhiteCanCastleKingSide.Should().BeFalse();
        extras.WhiteCanCastleQueenSide.Should().BeTrue();
        extras.BlackCanCastleKingSide.Should().BeTrue();
        extras.BlackCanCastleQueenSide.Should().BeTrue();
    }

    [Fact]
    public void GivenInitialPosition_WhenWhiteMovesQueensideRook_ThenOnlyQueenSideRightRevoked()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        // Clear path: pawn a2 forward double; alternate turn; then move rook a1->a2
        progress = progress.Move(WhitePawn1, A4);
        progress = progress.Move(BlackPawn5, E6);
        progress = progress.Move(WhiteRook1, A2);

        // assert
        var extras = progress.State.GetRequiredExtras<ChessStateExtras>();
        extras.WhiteCanCastleQueenSide.Should().BeFalse();
        extras.WhiteCanCastleKingSide.Should().BeTrue();
    }

    [Fact]
    public void GivenBothRooksMoved_WhenWhiteAttemptsCastling_ThenInvalid()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        // Clear both rook paths and move each rook from its starting square.
        progress = progress.Move(WhitePawn8, H4); // clear kingside
        progress = progress.Move(BlackPawn5, E6);
        progress = progress.Move(WhiteRook2, H2); // move kingside rook
        progress = progress.Move(BlackPawn4, D6);
        progress = progress.Move(WhitePawn1, A4); // clear queenside
        progress = progress.Move(BlackPawn3, C6);
        progress = progress.Move(WhiteRook1, A2); // move queenside rook
        progress = progress.Move(BlackPawn6, F6); // ensure turn returns to white for castle attempt

        // act
        var exKingSide = Record.Exception(() => progress = progress.Castle(Veggerby.Boards.Chess.Constants.ChessIds.Players.White, kingSide: true));
        var exQueenSide = Record.Exception(() => progress = progress.Castle(Veggerby.Boards.Chess.Constants.ChessIds.Players.White, kingSide: false));

        // assert
        exKingSide.Should().NotBeNull();
        exQueenSide.Should().NotBeNull();
    }
}
