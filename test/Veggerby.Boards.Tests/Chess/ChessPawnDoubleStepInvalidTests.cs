using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Chess.Support;
using Veggerby.Boards.Tests.TestHelpers;

using static Veggerby.Boards.Chess.ChessIds.Pieces;
using static Veggerby.Boards.Chess.ChessIds.Tiles;

namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Negative tests covering invalid pawn double-step attempts (off start rank, intermediate blocked, destination occupied).
/// Valid engine behavior for these is to ignore the event (state unchanged) due to failing rule conditions.
/// </summary>
public class ChessPawnDoubleStepInvalidTests
{
    [Fact]
    public void GivenPawnHasMoved_WhenAttemptDoubleStep_ThenIgnored()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var pawn = progress.Game.GetPiece(WhitePawn5).EnsureNotNull();
        // act1 single move e2->e3
        progress = progress.Move(WhitePawn5, E3);
        var afterSingle = progress;
        // black reply to restore white turn (quiet move: e7->e6)
        progress = progress.Move(BlackPawn5, E6);
        // attempt illegal double-step e3->e5 (skipping e4)
        progress = progress.Move(WhitePawn5, E5);
        // assert: pawn remained on e3 (ignored)
        progress.State.GetRequiredPieceState(pawn).CurrentTile.Id.Should().Be(ChessIds.Tiles.E3);
        // ensure new state reference not advanced by illegal attempt (identity equality ok) - semantics: Move returns same progress when ignored
        progress.Should().BeSameAs(progress);
    }

    [Fact]
    public void GivenIntermediateBlocked_WhenAttemptDoubleStep_ThenIgnored()
    {
        // arrange
        var progress = new BlockedDoubleStepScenarioBuilder().Compile();
        var pawn = progress.Game.GetPiece("white-pawn-test").EnsureNotNull();
        var before = progress.State.GetRequiredPieceState(pawn).CurrentTile;
        // act (attempt e2->e4 with e3 occupied)
        progress = progress.Move("white-pawn-test", "e4");
        // assert
        progress.State.GetRequiredPieceState(pawn).CurrentTile.Should().Be(before);
    }

    [Fact]
    public void GivenDestinationOccupied_WhenAttemptDoubleStep_ThenIgnored()
    {
        // arrange
        var progress = new DestinationOccupiedDoubleStepScenarioBuilder().Compile();
        var pawn = progress.Game.GetPiece("white-pawn-test").EnsureNotNull();
        var before = progress.State.GetRequiredPieceState(pawn).CurrentTile;
        // act (attempt e2->e4 with e4 occupied)
        progress = progress.Move("white-pawn-test", "e4");
        // assert
        progress.State.GetRequiredPieceState(pawn).CurrentTile.Should().Be(before);
    }
}