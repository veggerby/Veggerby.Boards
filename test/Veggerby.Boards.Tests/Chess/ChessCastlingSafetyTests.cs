using AwesomeAssertions;

using Veggerby.Boards;
using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

using Xunit;

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
        // Deterministic sequence to create an attack on f1 by a black knight positioned at g3 while keeping castling rights true.
        // Sequence rationale:
        // 1. White frees e-pawn (needed later for bishop relocation path clarity, but mainly to consume turn).
        // 2. Black develops knight g8 -> e7.
        // 3. White vacates g1 (knight g1 -> f3) clearing destination path square g1.
        // 4. Black knight e7 -> f5 continuing route toward g3.
        // 5. White vacates f1 (bishop f1 -> e2) clearing intermediate path square f1; e2 was cleared in step 1.
        // 6. Black knight f5 -> g3 now attacks f1 (knight move (-1,-2)). Path squares for castling (f1,g1) are empty but f1 is attacked.
        var progress = new ChessGameBuilder().Compile();
        progress = progress.Move("white-pawn-5", "e4");     // 1
        progress = progress.Move("black-knight-2", "e7");    // 2 (g8 -> e7)
        progress = progress.Move("white-knight-2", "f3");    // 3 (g1 -> f3)
        progress = progress.Move("black-knight-2", "f5");    // 4 (e7 -> f5)
        progress = progress.Move("white-bishop-2", "e2");    // 5 (f1 -> e2)
        progress = progress.Move("black-knight-2", "g3");    // 6 (f5 -> g3) now attacks f1

        // pre-assert sanity
        var extrasBefore = progress.State.GetExtras<ChessStateExtras>();
        extrasBefore.WhiteCanCastleKingSide.Should().BeTrue();
        extrasBefore.WhiteCanCastleQueenSide.Should().BeTrue();

        // act
        var ex = Record.Exception(() => progress = progress.Castle("white", kingSide: true));

        // assert
        ex.Should().NotBeNull();
    ex.Should().BeOfType<InvalidGameEventException>();
    // Current implementation does not surface attacked square id in exception message; future improvement could assert message details.
        var extrasAfter = progress.State.GetExtras<ChessStateExtras>();
        // Rights must remain (castling was denied without moving king/rook)
        extrasAfter.WhiteCanCastleKingSide.Should().BeTrue();
        extrasAfter.WhiteCanCastleQueenSide.Should().BeTrue();
    }
}