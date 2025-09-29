using System.Linq;

using AwesomeAssertions;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

using Xunit;

using static Veggerby.Boards.Chess.ChessIds.Pieces;

namespace Veggerby.Boards.Tests.Chess;

public class ChessBlockingTests
{
    [Fact]
    public void GivenInitialPosition_WhenWhiteQueenAttemptsToMoveThroughOwnPiece_ThenMoveIsInvalid()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var queen = progress.Game.GetPiece(WhiteQueen); // on d1
        var target = progress.Game.GetTile("tile-d4"); // path crosses d2 (occupied by pawn)
        var state = progress.State.GetState<PieceState>(queen);
        var path = queen.Patterns.Select(p =>
        {
            var v = new ResolveTilePathPatternVisitor(progress.Game.Board, state.CurrentTile, target);
            p.Accept(v);
            return v.ResultPath;
        }).FirstOrDefault(p => p is not null);

        var evt = new MovePieceGameEvent(queen, path);

        // act
        var updated = progress.HandleEvent(evt);

        // assert
        updated.State.GetState<PieceState>(queen).CurrentTile.Should().Be(state.CurrentTile); // unchanged
    }

}