using System.Collections.Generic;

using Veggerby.Boards.Artifacts.Relations;
// ChessIds lives directly in Veggerby.Boards.Chess namespace
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Chess-specific <see cref="GameProgress"/> extension methods.
/// </summary>
public static partial class GameExtensions
{
    /// <summary>
    /// Explicit chess castling helper (king side or queen side) removing reliance on synthetic path inside generic Move.
    /// </summary>
    /// <param name="progress">Game progress.</param>
    /// <param name="color">ChessIds.Players.White or ChessIds.Players.Black.</param>
    /// <param name="kingSide">True for king-side, false for queen-side.</param>
    public static GameProgress Castle(this GameProgress progress, string color, bool kingSide)
    {
        var kingId = color + ChessIds.PieceSuffixes.King;
        var game = progress.Game;
        var king = game.GetPiece(kingId);
        var kingState = progress.State.GetState<PieceState>(king);
        var start = kingState.CurrentTile.Id;
        var expectedStart = color == ChessIds.Players.White ? ChessIds.Tiles.E1 : ChessIds.Tiles.E8;
        if (start != expectedStart) { return progress; }
        var destination = color == ChessIds.Players.White
            ? (kingSide ? game.GetTile(ChessIds.Tiles.G1) : game.GetTile(ChessIds.Tiles.C1))
            : (kingSide ? game.GetTile(ChessIds.Tiles.G8) : game.GetTile(ChessIds.Tiles.C8));

        // Build horizontal path relations (two steps toward rook)
        char fromFile = start[5];
        char toFile = destination.Id[5];
        int step = fromFile < toFile ? 1 : -1;
        var rank = start[6];
        var relations = new List<TileRelation>();
    var dirId = step == 1 ? Constants.Directions.East : Constants.Directions.West;
    var direction = new Direction(dirId);
        var current = kingState.CurrentTile;
        for (char f = (char)(fromFile + step); ; f = (char)(f + step))
        {
            var nextTile = game.GetTile($"tile-{f}{rank}");
            relations.Add(new TileRelation(current, nextTile, direction));
            current = nextTile;
            if (f == toFile) { break; }
        }
        var path = new TilePath(relations);
        var @event = new MovePieceGameEvent(king, path);
        return progress.HandleEvent(@event);
    }
}