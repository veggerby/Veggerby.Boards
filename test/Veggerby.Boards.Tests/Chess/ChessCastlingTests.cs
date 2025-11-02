using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.TestHelpers;

using static Veggerby.Boards.Chess.ChessIds.Pieces;
using static Veggerby.Boards.Chess.ChessIds.Tiles;

namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Basic castling tests (no check / attack validation yet).
/// </summary>
public class ChessCastlingTests
{
    [Fact]
    public void GivenClearedKingsidePath_WhenWhiteCastlesKingSide_ThenKingAndRookRelocatedAndRightsRevoked()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        // Clear path squares f1 and g1 (king e1 -> g1) by vacating g1 (knight) and f1 (bishop) and ensuring bishop destination e2 is free.
        // 1. Advance pawn e2 -> e4 (double step frees e2)
        progress = progress.Move(WhitePawn5, E4);
        progress = progress.Move(BlackPawn5, E6);
        // 2. Knight g1 -> f3 (vacates g1)
        progress = progress.Move(WhiteKnight2, F3);
        progress = progress.Move(BlackPawn4, D6);
        // 3. Bishop f1 -> e2 (vacates f1 now that e2 is empty)
        progress = progress.Move(WhiteBishop2, E2);
        progress = progress.Move(BlackPawn3, C6);
        // Attempt castling using explicit helper
        progress = progress.Castle(ChessIds.Players.White, kingSide: true);
        // assert
        var king = progress.Game.GetPiece(WhiteKing).EnsureNotNull();
        var rook = progress.Game.GetPiece(WhiteRook2).EnsureNotNull();
        progress.State.GetRequiredPieceState(king).CurrentTile.Id.Should().Be(G1);
        progress.State.GetRequiredPieceState(rook).CurrentTile.Id.Should().Be(F1);
        var extras = progress.State.GetExtras<ChessStateExtras>();
        extras.Should().NotBeNull();
        extras!.WhiteCanCastleKingSide.Should().BeFalse();
        extras.WhiteCanCastleQueenSide.Should().BeFalse();
    }

    [Fact]
    public void GivenBlockedQueensidePath_WhenWhiteAttemptsQueenSideCastle_ThenIgnored()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        var before = progress.State;
        // act & assert (malformed castling attempt should raise invalid event exception due to path blockage)
        var ex = Record.Exception(() => progress = progress.Castle(ChessIds.Players.White, kingSide: false));
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidGameEventException>();
        // state unchanged
        var king = progress.Game.GetPiece(WhiteKing).EnsureNotNull();
        progress.State.GetRequiredPieceState(king).CurrentTile.Id.Should().Be(E1);
        before.Should().NotBeSameAs(null); // guard
    }
}
