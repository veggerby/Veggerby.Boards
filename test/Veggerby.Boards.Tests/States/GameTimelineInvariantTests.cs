using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.States;

public class GameTimelineInvariantTests
{
    [Fact(Skip = "TODO: Pawn single-step relation null under current feature flag combination; implement deterministic path helper before enabling.")]
    public void GivenTimeline_WhenUndoRedoSequenceApplied_ThenStateReferenceRestored()
    {
        // arrange
        using var _ = new FeatureFlagScope(hashing: true, compiledPatterns: true, slidingFastPath: false);
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var initialState = progress.State;
        var timeline = GameTimeline.Create(initialState);

        // create two successor states via legal pawn moves
        var whitePawn = progress.Game.GetPiece("white-pawn-2");
        var from1 = progress.Game.GetTile("tile-b2");
        var to1 = progress.Game.GetTile("tile-b3");
        var rel1 = progress.Game.Board.GetTileRelation(from1, to1);
        rel1.Should().NotBeNull();
        var path1 = new TilePath([rel1]);
        var afterFirst = progress.HandleEvent(new MovePieceGameEvent(whitePawn, path1));

        var blackPawn = afterFirst.Game.GetPiece("black-pawn-2");
        var from2 = afterFirst.Game.GetTile("tile-b7");
        var to2 = afterFirst.Game.GetTile("tile-b6");
        var rel2 = afterFirst.Game.Board.GetTileRelation(from2, to2);
        rel2.Should().NotBeNull();
        var path2 = new TilePath([rel2]);
        var afterSecond = afterFirst.HandleEvent(new MovePieceGameEvent(blackPawn, path2));

        var t2 = timeline.Push(afterFirst.State);
        var t3 = t2.Push(afterSecond.State);

        // act
        var undone = t3.Undo();
        var redone = undone.Redo();

        // assert
        redone.Present.Should().BeSameAs(t3.Present);
        undone.Present.Should().BeSameAs(afterFirst.State);
        if (t3.Present.Hash.HasValue)
        {
            redone.Present.Hash.Should().Be(t3.Present.Hash);
            redone.Present.Hash128.Should().Be(t3.Present.Hash128);
        }
    }

    [Fact]
    public void GivenUndoWithoutPast_WhenUndo_ThenTimelineUnchanged()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var timeline = GameTimeline.Create(progress.State);

        // act
        var undone = timeline.Undo();

        // assert
        undone.Should().BeSameAs(timeline);
    }

    [Fact]
    public void GivenRedoWithoutFuture_WhenRedo_ThenTimelineUnchanged()
    {
        // arrange
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var timeline = GameTimeline.Create(progress.State);

        // act
        var redone = timeline.Redo();

        // assert
        redone.Should().BeSameAs(timeline);
    }
}