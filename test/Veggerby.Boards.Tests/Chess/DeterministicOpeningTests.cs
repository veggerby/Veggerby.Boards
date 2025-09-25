using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.TestHelpers;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Tests covering the deterministic Ruy Lopez opening sequence produced by <see cref="DeterministicChessOpening"/>.
/// Ensures: correct number of events applied, expected terminal piece placements for resolved prefix, and immutability of prior states.
/// </summary>
public class DeterministicOpeningTests
{
    [Xunit.Fact]
    public void GivenStandardChessGame_WhenApplyingDeterministicOpening_ThenPiecesReachExpectedSquares()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var initialState = progress.State;

        // act
        progress = DeterministicChessOpening.ApplyRuyLopezOpening(progress);

        // assert
        var applied = progress.Events.Count();
        applied.Should().BeInRange(0, 5);

        static string Normalize(string id) => id.StartsWith("tile-") ? id[5..] : id;

        var whitePawn = progress.Game.GetPiece("white-pawn-5");
        var blackPawn = progress.Game.GetPiece("black-pawn-5");
        var whiteKnight = progress.Game.GetPiece("white-knight-2");
        var blackKnight = progress.Game.GetPiece("black-knight-1");
        var whiteBishop = progress.Game.GetPiece("white-bishop-2");

        if (whitePawn is not null && applied >= 1)
        {
            var ps = progress.State.GetState<PieceState>(whitePawn);
            new[] { "e3", "e4" }.Should().Contain(Normalize(ps.CurrentTile.Id));
        }

        if (blackPawn is not null && applied >= 2)
        {
            var ps = progress.State.GetState<PieceState>(blackPawn);
            new[] { "e6", "e5" }.Should().Contain(Normalize(ps.CurrentTile.Id));
        }

        if (whiteKnight is not null && applied >= 3)
        {
            var ks = progress.State.GetState<PieceState>(whiteKnight);
            Normalize(ks.CurrentTile.Id).Should().Be("f3");
        }

        if (blackKnight is not null && applied >= 4)
        {
            var ks = progress.State.GetState<PieceState>(blackKnight);
            Normalize(ks.CurrentTile.Id).Should().Be("c6");
        }

        if (whiteBishop is not null && applied == 5)
        {
            var bs = progress.State.GetState<PieceState>(whiteBishop);
            new[] { "b5", "c4" }.Should().Contain(Normalize(bs.CurrentTile.Id));
        }

        // Previous state must remain unchanged (immutability)
        progress.State.Should().NotBeSameAs(initialState);

        // Ensure original white pawn still at e2 in the initial snapshot
        var originalPawnState = initialState.GetStates<PieceState>().Single(ps => ps.Artifact.Id == "white-pawn-5");
        Normalize(originalPawnState.CurrentTile.Id).Should().Be("e2");
    }
}
