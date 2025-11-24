using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

using static Veggerby.Boards.Chess.Constants.ChessIds.Pieces;
using static Veggerby.Boards.Chess.Constants.ChessIds.Tiles;

using Veggerby.Boards.Chess.Extensions;
namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Tests for CastlingKingSafetyGameEventCondition (king cannot castle through or into check).
/// </summary>
public class ChessCastlingSafetyTests
{
    [Fact]
    public void GivenIntermediateSquareF1UnderAttackByKnight_WhenWhiteAttemptsKingSideCastle_ThenCastlingDeniedAndRightsIntact()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        progress = progress.Move(WhitePawn5, E4);     // 1
        progress = progress.Move(BlackKnight2, E7);    // 2 (g8 -> e7)
        progress = progress.Move(WhiteKnight2, F3);    // 3 (g1 -> f3)
        progress = progress.Move(BlackKnight2, F5);    // 4 (e7 -> f5)
        progress = progress.Move(WhiteBishop2, E2);    // 5 (f1 -> e2)
        progress = progress.Move(BlackKnight2, G3);    // 6 (f5 -> g3) now attacks f1

        // pre-assert sanity
        var extrasBefore = progress.State.GetExtras<ChessStateExtras>();
        extrasBefore.Should().NotBeNull();
        extrasBefore.WhiteCanCastleKingSide.Should().BeTrue();
        extrasBefore.WhiteCanCastleQueenSide.Should().BeTrue();

        // act
        var ex = Record.Exception(() => progress = progress.Castle(Veggerby.Boards.Chess.Constants.ChessIds.Players.White, kingSide: true));

        // assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidGameEventException>();
        // Current implementation does not surface attacked square id in exception message; future improvement could assert message details.
        var extrasAfter = progress.State.GetExtras<ChessStateExtras>();
        extrasAfter.Should().NotBeNull();
        // Rights must remain (castling was denied without moving king/rook)
        extrasAfter.WhiteCanCastleKingSide.Should().BeTrue();
        extrasAfter.WhiteCanCastleQueenSide.Should().BeTrue();
    }

    [Fact]
    public void GivenDestinationSquareC1UnderAttackByKnight_WhenWhiteAttemptsQueenSideCastle_ThenCastlingDeniedAndRightsIntact()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        // Sequence:
        // 1. White clears d1, c1 by moving obstructing pieces: bishop c1 -> e3 (needs path cleared by moving pawn e2 first? Actually c1 bishop path to e3 requires d2 & e3 squares; d2 blocked by pawn d2. We'll use bishop c1 -> b2 route after clearing b2 pawn.)
        // Simplify: move knight b1 -> d2 freeing b1 for later; then move bishop c1 -> b2 after moving pawn b2.
        progress = progress.Move(WhiteKnight1, D2);       // consume turn & vacate b1
        progress = progress.Move(BlackKnight2, E7);        // g8 -> e7 starting route to b3
        progress = progress.Move(WhitePawn2, B4);          // clear b2 for bishop c1 -> b2
        progress = progress.Move(BlackKnight2, C6);        // e7 -> c6
        progress = progress.Move(WhiteBishop1, B2);        // c1 -> b2 clearing c1
        progress = progress.Move(BlackKnight2, B4);        // c6 -> b4
        progress = progress.Move(WhiteQueen, E2);           // queen d1 -> e2 clearing d1 (also safe square)
        progress = progress.Move(BlackKnight2, D3);        // b4 -> d3
        progress = progress.Move(WhitePawn5, E4);          // filler; maintain rights
        progress = progress.Move(BlackKnight2, B2);        // d3 -> b2 (attacks c4,a4,d1,d? etc)
        progress = progress.Move(WhiteBishop2, E2);        // f1 -> e2 clearing f1 (not strictly needed but keeps symmetry)
        progress = progress.Move(BlackKnight2, B3);        // b2 -> b3 now attacking c1

        var extrasBefore = progress.State.GetExtras<ChessStateExtras>();
        extrasBefore.Should().NotBeNull();
        extrasBefore.WhiteCanCastleQueenSide.Should().BeTrue();
        extrasBefore.WhiteCanCastleKingSide.Should().BeTrue();

        // act
        var ex = Record.Exception(() => progress = progress.Castle(Veggerby.Boards.Chess.Constants.ChessIds.Players.White, kingSide: false));

        // assert
        ex.Should().NotBeNull();
        ex.Should().BeOfType<InvalidGameEventException>();
        var extrasAfter = progress.State.GetExtras<ChessStateExtras>();
        extrasAfter.Should().NotBeNull();
        extrasAfter.WhiteCanCastleQueenSide.Should().BeTrue(); // rights unchanged (attempt denied)
        extrasAfter.WhiteCanCastleKingSide.Should().BeTrue();
    }
}
