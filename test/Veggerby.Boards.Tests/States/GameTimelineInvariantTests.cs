using System.Collections.Generic;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.States;

public class GameTimelineInvariantTests
{
    private static TilePath BuildSingleStep(GameProgress progress, string fromId, string toId)
    {
        // try common algebraic ids first, then legacy prefixed variant
        var from = TryGetTile(progress, fromId) ?? TryGetTile(progress, $"tile-{fromId}")!;
        var to = TryGetTile(progress, toId) ?? TryGetTile(progress, $"tile-{toId}")!;

        // direct relation fast-path
        var rel = progress.Game.Board.GetTileRelation(from, to);
        if (rel != null)
        {
            return new TilePath([rel]);
        }

        // fallback: pattern visitor (compiled-first resolver parity path)
        var path = new ResolveTilePathPatternVisitor(progress.Game.Board, from, to).ResultPath;
        path.Should().NotBeNull("expected non-null path between {0} -> {1}", from.Id, to.Id);
        return path!;
    }

    private static Artifacts.Tile TryGetTile(GameProgress progress, string id)
    {
        try
        {
            return progress.Game.GetTile(id);
        }
        catch
        {
            return null;
        }
    }

    [Fact]
    public void GivenTimeline_WhenUndoRedoSequenceApplied_ThenStateReferenceRestored()
    {
        // arrange
        using var _ = new FeatureFlagScope(hashing: true, compiledPatterns: true, slidingFastPath: false);
        var progress = new ChessGameBuilder().Compile();
        var initialState = progress.State;
        var timeline = GameTimeline.Create(initialState);

        var whitePawn = progress.Game.GetPiece("white-pawn-2");
        var path1 = BuildSingleStep(progress, "b2", "b3");
        var afterFirst = progress.HandleEvent(new MovePieceGameEvent(whitePawn, path1));

        var blackPawn = afterFirst.Game.GetPiece("black-pawn-2");
        var path2 = BuildSingleStep(afterFirst, "b7", "b6");
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
    public void GivenMultipleUndoRedoCycles_WhenRepeated_ThenHashesAndReferencesStable()
    {
        // arrange
        using var _ = new FeatureFlagScope(hashing: true, compiledPatterns: true, slidingFastPath: false);
        var progress = new ChessGameBuilder().Compile();
        var moves = new List<GameProgress>();
        var timeline = GameTimeline.Create(progress.State);

        // three alternating pawn moves (white b2->b3, black b7->b6, white g2->g3)
        var wPawnB = progress.Game.GetPiece("white-pawn-2");
        var move1 = progress.HandleEvent(new MovePieceGameEvent(wPawnB, BuildSingleStep(progress, "b2", "b3")));
        moves.Add(move1);

        var bPawnB = move1.Game.GetPiece("black-pawn-2");
        var move2 = move1.HandleEvent(new MovePieceGameEvent(bPawnB, BuildSingleStep(move1, "b7", "b6")));
        moves.Add(move2);

        var wPawnG = move2.Game.GetPiece("white-pawn-7"); // g-file pawn
        var move3 = move2.HandleEvent(new MovePieceGameEvent(wPawnG, BuildSingleStep(move2, "g2", "g3")));
        moves.Add(move3);

        var t = timeline;
        foreach (var m in moves)
        {
            t = t.Push(m.State);
        }

        // act & assert: perform two full undo/redo sweeps
        for (int cycle = 0; cycle < 2; cycle++)
        {
            var cursor = t;
            var undoStack = new Stack<GameState>();
            while (!cursor.Past.IsEmpty)
            {
                undoStack.Push(cursor.Present);
                cursor = cursor.Undo();
            }
            // at root
            cursor.Present.Should().BeSameAs(progress.State);

            // redo forward
            while (!cursor.Future.IsEmpty)
            {
                var before = cursor.Present;
                cursor = cursor.Redo();
                cursor.Present.Hash.Should().NotBeNull();
                if (before.Hash.HasValue)
                {
                    // hashes differ only if actual state content changed
                    // advancing along redo path should yield strictly different state reference until final cursor
                    cursor.Present.Should().NotBeSameAs(before);
                }
            }
            cursor.Present.Should().BeSameAs(t.Present);
            if (t.Present.Hash.HasValue)
            {
                cursor.Present.Hash.Should().Be(t.Present.Hash);
                cursor.Present.Hash128.Should().Be(t.Present.Hash128);
            }
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