using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.PropertyTests;

// Early placeholder property tests - these will be expanded with richer generators.
using FsCheck.Xunit;

// Minimal placeholder property until richer generators are implemented.
public class ChessInvariants
{
    [Property(MaxTest = 10)]
    public bool ChessGameBuilderProducesInitialState()
    {
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        return progress.Game is not null && progress.State is not null;
    }

    [Property(MaxTest = 25)]
    public bool PieceMoveDoesNotMutatePreviousState()
    {
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var pawn = progress.Game.GetPiece("white-pawn-5"); // e2
        var fromTile = progress.Game.GetTile("e2");
        var toTile = progress.Game.GetTile("e4");
        if (pawn is null || fromTile is null || toTile is null) { return true; } // vacuous success
        var path = new ResolveTilePathPatternVisitor(progress.Game.Board, fromTile, toTile).ResultPath;
        if (path is null) { return true; }
        var before = progress.State;
        var beforePieceState = before.GetStates<PieceState>().Single(ps => ps.Artifact == pawn);
        var after = progress.HandleEvent(new MovePieceGameEvent(pawn, path));
        // previous state piece must still reference original tile
        var stillOriginal = beforePieceState.CurrentTile == fromTile;
        // new state piece moved
        var afterPieceState = after.State.GetStates<PieceState>().Single(ps => ps.Artifact == pawn);
        var moved = afterPieceState.CurrentTile == toTile;
        return stillOriginal && moved;
    }

    [Property(MaxTest = 25)]
    public bool ReapplyingSameMoveIsDeterministicOrRejected()
    {
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var pawn = progress.Game.GetPiece("white-pawn-5");
        var fromTile = progress.Game.GetTile("e2");
        var toTile = progress.Game.GetTile("e4");
        if (pawn is null || fromTile is null || toTile is null) { return true; }
        var path = new ResolveTilePathPatternVisitor(progress.Game.Board, fromTile, toTile).ResultPath;
        if (path is null) { return true; }
        var moveEvent = new MovePieceGameEvent(pawn, path);
        var first = progress.HandleEvent(moveEvent);
        // attempt the same event again from the original progress should yield identical result OR be ignored
        var second = progress.HandleEvent(moveEvent);
        var deterministic = first.State.Equals(second.State);
        return deterministic;
    }

    [Property(MaxTest = 25)]
    public bool StateHistoryChainLengthMatchesMoves()
    {
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var pawnIds = new[] { "white-pawn-1", "white-pawn-2", "white-pawn-3", "white-pawn-4", "white-pawn-5", "white-pawn-6", "white-pawn-7", "white-pawn-8" };
        int applied = 0;
        foreach (var pid in pawnIds)
        {
            var pawn = progress.Game.GetPiece(pid);
            var file = pid[^1]; // last char 1..8 maps to a..h (simplistic)
            var colIndex = (int)char.GetNumericValue(file) - 1;
            var fileChar = (char)('a' + colIndex);
            var fromName = $"{fileChar}2";
            var toName = $"{fileChar}3"; // single step to keep always legal
            var fromTile = progress.Game.GetTile(fromName);
            var toTile = progress.Game.GetTile(toName);
            if (pawn is null || fromTile is null || toTile is null) { continue; }
            var path = new ResolveTilePathPatternVisitor(progress.Game.Board, fromTile, toTile).ResultPath;
            if (path is null) { continue; }
            progress = progress.HandleEvent(new MovePieceGameEvent(pawn, path));
            applied++;
        }
        // walk history via previous references (not exposed directly; emulate by replay count)
        // The number of events recorded should match applied
        return progress.Events.Count() == applied;
    }

    [Property(MaxTest = 25)]
    public bool OpeningMovesDoNotReduceWhitePieceCount()
    {
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var initialWhiteCount = progress.State.GetStates<PieceState>().Count(ps => ps.Artifact.Owner.Id == "white");
        var pawn = progress.Game.GetPiece("white-pawn-5");
        var fromTile = progress.Game.GetTile("e2");
        var toTile = progress.Game.GetTile("e4");
        if (pawn is null || fromTile is null || toTile is null) { return true; }
        var path = new ResolveTilePathPatternVisitor(progress.Game.Board, fromTile, toTile).ResultPath;
        if (path is null) { return true; }
        progress = progress.HandleEvent(new MovePieceGameEvent(pawn, path));
        var afterWhiteCount = progress.State.GetStates<PieceState>().Count(ps => ps.Artifact.Owner.Id == "white");
        return afterWhiteCount == initialWhiteCount;
    }
}