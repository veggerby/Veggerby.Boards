using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Chess.Builders;

using static Veggerby.Boards.Chess.ChessIds.Pieces;
using static Veggerby.Boards.Chess.ChessIds.Tiles;

namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Validates queen capturing a pawn in a straight unobstructed file using minimal chess capture scenario builder.
/// </summary>
public class ChessCaptureScaffoldingTests
{
    [Fact]
    public void GivenOpponentPieceInLine_WhenQueenMovesOntoIt_ThenCaptureOccurs()
    {
        // arrange (direct capture position: queen e1, pawn e7)
        var progress = new ChessCaptureScenarioBuilder().Compile();
        var whiteQueen = progress.Game.GetPiece(WhiteQueen);
        whiteQueen.Should().NotBeNull();
        var blackPawn = progress.Game.GetPiece(BlackPawn5);
        blackPawn.Should().NotBeNull();

        // act: queen moves to e7 capturing pawn
        progress = progress.Move(WhiteQueen, E7);

        // assert
        var queenState = progress.State.GetState<PieceState>(whiteQueen!);
        queenState.Should().NotBeNull();
        queenState!.CurrentTile.Should().NotBeNull();
        queenState!.CurrentTile!.Id.Should().Be(E7);
        progress.State.IsCaptured(blackPawn!).Should().BeTrue();
        progress.State.GetPiecesOnTile(queenState!.CurrentTile)
            .Should().NotContain(p => p.Equals(blackPawn));
    }
}