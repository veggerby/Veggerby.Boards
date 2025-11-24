using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

using static Veggerby.Boards.Chess.Constants.ChessIds.Pieces;

namespace Veggerby.Boards.Tests.Chess;

public class ChessBlockingTests
{
    [Fact]
    public void GivenInitialPosition_WhenWhiteQueenAttemptsToMoveThroughOwnPiece_ThenMoveIsInvalid()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();
        var queen = progress.Game.GetPiece(WhiteQueen);
        queen.Should().NotBeNull();
        var target = progress.Game.GetTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D4);
        target.Should().NotBeNull();
        var state = progress.State.GetState<PieceState>(queen!);
        state.Should().NotBeNull();
        var path = queen.Patterns.Select(p =>
        {
            var v = new ResolveTilePathPatternVisitor(progress.Game.Board, state!.CurrentTile, target!);
            p.Accept(v);
            return v.ResultPath;
        }).FirstOrDefault(p => p is not null);
        path.Should().NotBeNull();
        var evt = new MovePieceGameEvent(queen!, path!);

        // act
        var updated = progress.HandleEvent(evt);

        // assert
        var updatedQueenState = updated.State.GetState<PieceState>(queen!);
        updatedQueenState.Should().NotBeNull();
        updatedQueenState!.CurrentTile.Should().Be(state!.CurrentTile); // unchanged
    }

}
