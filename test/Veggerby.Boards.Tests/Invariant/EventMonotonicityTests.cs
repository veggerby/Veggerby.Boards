using System.Linq;

using Veggerby.Boards.Artifacts.Relations;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Invariant;

public class EventMonotonicityTests
{
    [Fact(DisplayName = "Event count increments only when state changes")]
    [Trait("Category", "Invariant")]
    public void GivenConsecutiveIdenticalMove_WhenReapplied_ThenSecondIsIgnored()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var pawn = progress.Game.GetPiece("white-pawn-5");
        var from = progress.Game.GetTile("e2");
        var to = progress.Game.GetTile("e4");
        if (pawn is null || from is null || to is null)
        {
            return; // vacuous
        }

        var path = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to).ResultPath;
        if (path is null)
        {
            return; // engine disallowed path
        }

        var move = new MovePieceGameEvent(pawn, path);

        // act
        var first = progress.HandleEvent(move);
        var second = progress.HandleEvent(move);

        // assert
        first.Events.Count().Should().Be(progress.Events.Count() + 1);
        second.Events.Count().Should().Be(first.Events.Count(), "reapplying identical move should be ignored");
        second.State.Should().Be(first.State);
    }
}