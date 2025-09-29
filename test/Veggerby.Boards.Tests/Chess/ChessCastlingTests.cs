using AwesomeAssertions;

using Veggerby.Boards;
using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

using Xunit;

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
        var progress = new ChessGameBuilder().Compile();
        // Clear path squares f1 and g1 (king e1 -> g1) by vacating g1 (knight) and f1 (bishop) and ensuring bishop destination e2 is free.
        // 1. Advance pawn e2 -> e4 (double step frees e2)
        progress = progress.Move("white-pawn-5", "e4");
        progress = progress.Move("black-pawn-5", "e6");
        // 2. Knight g1 -> f3 (vacates g1)
        progress = progress.Move("white-knight-2", "f3");
        progress = progress.Move("black-pawn-4", "d6");
        // 3. Bishop f1 -> e2 (vacates f1 now that e2 is empty)
        progress = progress.Move("white-bishop-2", "e2");
        progress = progress.Move("black-pawn-3", "c6");
        // Attempt castling using explicit helper
        progress = progress.Castle("white", kingSide: true);
        // assert
        var king = progress.Game.GetPiece("white-king");
        var rook = progress.Game.GetPiece("white-rook-2");
        progress.State.GetState<PieceState>(king).CurrentTile.Id.Should().Be("tile-g1");
        progress.State.GetState<PieceState>(rook).CurrentTile.Id.Should().Be("tile-f1");
        var extras = progress.State.GetExtras<ChessStateExtras>();
        extras.WhiteCanCastleKingSide.Should().BeFalse();
        extras.WhiteCanCastleQueenSide.Should().BeFalse();
    }

    [Fact]
    public void GivenBlockedQueensidePath_WhenWhiteAttemptsQueenSideCastle_ThenIgnored()
    {
        // arrange (initial pieces block: bishop c1, queen d1, knight b1)
        var progress = new ChessGameBuilder().Compile();
        var before = progress.State;
        // act & assert (malformed castling attempt should raise invalid event exception due to path blockage)
        var ex = Record.Exception(() => progress = progress.Castle("white", kingSide: false));
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidGameEventException>();
        // state unchanged
        var king = progress.Game.GetPiece("white-king");
        progress.State.GetState<PieceState>(king).CurrentTile.Id.Should().Be("tile-e1");
        before.Should().NotBeSameAs(null); // guard
    }
}