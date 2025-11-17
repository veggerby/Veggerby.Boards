using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess.MoveGeneration;
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
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(color);
        var kingId = color + ChessIds.PieceSuffixes.King;
        var game = progress.Game;
        var king = game.GetPiece(kingId);
        if (king is null)
        {
            return progress; // missing artifact
        }
        var kingState = progress.State.GetState<PieceState>(king);
        if (kingState?.CurrentTile is null)
        {
            return progress;
        }
        var start = kingState.CurrentTile.Id;
        var expectedStart = color == ChessIds.Players.White ? ChessIds.Tiles.E1 : ChessIds.Tiles.E8;

        if (start != expectedStart)
        {
            return progress;
        }

        var destination = color == ChessIds.Players.White
            ? (kingSide ? game.GetTile(ChessIds.Tiles.G1) : game.GetTile(ChessIds.Tiles.C1))
            : (kingSide ? game.GetTile(ChessIds.Tiles.G8) : game.GetTile(ChessIds.Tiles.C8));
        if (destination is null)
        {
            return progress;
        }

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
            if (nextTile is null)
            {
                return progress; // abort if topology incomplete
            }
            relations.Add(new TileRelation(current!, nextTile, direction));
            current = nextTile;

            if (f == toFile)
            {
                break;
            }
        }

        var path = new TilePath(relations);
        var @event = new MovePieceGameEvent(king, path);
        return progress.HandleEvent(@event);
    }

    /// <summary>
    /// Executes a chess move using Standard Algebraic Notation (SAN).
    /// </summary>
    /// <param name="progress">Current game progress.</param>
    /// <param name="san">Standard Algebraic Notation string (e.g., "e4", "Nf3", "O-O", "exd5").</param>
    /// <returns>Updated game progress with the move executed.</returns>
    /// <exception cref="ArgumentNullException">Thrown if progress or san is null.</exception>
    /// <exception cref="BoardException">Thrown if the SAN does not correspond to any legal move in the current position.</exception>
    /// <remarks>
    /// This method generates legal moves for the current position, parses the SAN notation to find
    /// a matching move, and executes it. If no legal move matches the SAN, an exception is thrown.
    ///
    /// Supported SAN notation includes:
    /// - Basic moves: "e4", "Nf3", "Bc4"
    /// - Captures: "exd5", "Nxf7", "Bxc6"
    /// - Castling: "O-O" (kingside), "O-O-O" (queenside)
    /// - Promotions: "e8=Q", "axb8=N"
    /// - Disambiguation: "Nbd2", "R1e1", "Qh4e1"
    /// - Check/checkmate symbols ("+", "#") are automatically stripped
    /// </remarks>
    public static GameProgress MoveSan(this GameProgress progress, string san)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(san);

        var filter = new ChessLegalityFilter(progress.Game);
        var legalMoves = filter.GenerateLegalMoves(progress.State);
        var move = ChessSanParser.ParseSan(san, legalMoves, progress.Game, progress.State);

        if (move is null)
        {
            throw new BoardException($"Invalid or illegal move in SAN notation: '{san}'");
        }

        return progress.Move(move.Piece.Id, move.To.Id);
    }
}