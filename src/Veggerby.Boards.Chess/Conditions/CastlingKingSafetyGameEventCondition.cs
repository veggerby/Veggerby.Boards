using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Validates that a castling king move is not performed while the king is in check, nor passes through or lands on an attacked square.
/// </summary>
/// <remarks>
/// Squares checked (standard chess):
///  * King-side: e-file start (e1/e8), intermediate f1/f8, destination g1/g8.
///  * Queen-side: e-file start, intermediate d1/d8, destination c1/c8.
/// Attack detection is structural: it enumerates opposing piece attack patterns without considering discovered pin legality (sufficient for castling safety).
/// </remarks>
public sealed class CastlingKingSafetyGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        // Only evaluate for king multi-square horizontal moves originating at starting square (filtered earlier by CastlingGameEventCondition).
        if (!@event.Piece.Id.EndsWith("-king")) { return ConditionResponse.Ignore("Not a king"); }
        var isWhite = @event.Piece.Owner?.Id == "white";
        var startId = isWhite ? "tile-e1" : "tile-e8";
        if (@event.From?.Id != startId) { return ConditionResponse.Ignore("Not from initial square"); }
        if (@event.To is null) { return ConditionResponse.Ignore("Missing destination"); }
        var destId = @event.To.Id;
        bool kingSide = destId == (isWhite ? "tile-g1" : "tile-g8");
        bool queenSide = destId == (isWhite ? "tile-c1" : "tile-c8");
        if (!kingSide && !queenSide) { return ConditionResponse.Ignore("Not a castling target"); }

        var squaresToCheck = new[]
        {
            startId,
            kingSide ? (isWhite ? "tile-f1" : "tile-f8") : (isWhite ? "tile-d1" : "tile-d8"),
            destId
        };

        // Build attacked set by opponent pieces.
        var attacked = ComputeAttackedSquares(engine, state, isWhite ? "black" : "white");
        foreach (var sq in squaresToCheck)
        {
            if (attacked.Contains(sq))
            {
                return ConditionResponse.Fail($"Castling path square {sq} is under attack");
            }
        }

        return ConditionResponse.Valid;
    }

    private static HashSet<string> ComputeAttackedSquares(GameEngine engine, GameState state, string attackerPlayerId)
    {
        var attacked = new HashSet<string>(StringComparer.Ordinal);
        var pieces = state.GetStates<PieceState>()
            .Where(ps => ps.Artifact.Owner?.Id == attackerPlayerId)
            .ToArray();
        foreach (var ps in pieces)
        {
            var id = ps.Artifact.Id;
            if (id.Contains("pawn", StringComparison.Ordinal))
            {
                AddPawnAttacks(engine, ps, attacked, attackerPlayerId == "white");
            }
            else if (id.Contains("knight", StringComparison.Ordinal))
            {
                AddKnightAttacks(engine, ps, attacked);
            }
            else if (id.Contains("bishop", StringComparison.Ordinal))
            {
                AddSlidingAttacks(engine, state, ps, attacked, diagonals: true, orthogonals: false);
            }
            else if (id.Contains("rook", StringComparison.Ordinal))
            {
                AddSlidingAttacks(engine, state, ps, attacked, diagonals: false, orthogonals: true);
            }
            else if (id.Contains("queen", StringComparison.Ordinal))
            {
                AddSlidingAttacks(engine, state, ps, attacked, diagonals: true, orthogonals: true);
            }
            else if (id.EndsWith("-king", StringComparison.Ordinal))
            {
                AddKingAttacks(engine, ps, attacked);
            }
        }
        return attacked;
    }

    private static void AddPawnAttacks(GameEngine engine, PieceState ps, HashSet<string> attacked, bool isWhite)
    {
        var (file, rank) = Parse(ps.CurrentTile.Id);
        int rankDir = isWhite ? 1 : -1; // pawns attack forward relative direction
        AddIfExists(engine, attacked, (char)(file + 1), rank + rankDir);
        AddIfExists(engine, attacked, (char)(file - 1), rank + rankDir);
    }

    private static void AddKnightAttacks(GameEngine engine, PieceState ps, HashSet<string> attacked)
    {
        var (file, rank) = Parse(ps.CurrentTile.Id);
        int[] df = { -2, -1, 1, 2 };
        int[] dr = { -2, -1, 1, 2 };
        foreach (var f in df)
        {
            foreach (var r in dr)
            {
                if (Math.Abs(f) + Math.Abs(r) == 3 && Math.Abs(f) != Math.Abs(r))
                {
                    AddIfExists(engine, attacked, (char)(file + f), rank + r);
                }
            }
        }
    }

    private static void AddKingAttacks(GameEngine engine, PieceState ps, HashSet<string> attacked)
    {
        var (file, rank) = Parse(ps.CurrentTile.Id);
        for (int df = -1; df <= 1; df++)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                if (df == 0 && dr == 0) { continue; }
                AddIfExists(engine, attacked, (char)(file + df), rank + dr);
            }
        }
    }

    private static void AddSlidingAttacks(GameEngine engine, GameState state, PieceState ps, HashSet<string> attacked, bool diagonals, bool orthogonals)
    {
        var directions = new List<(int df, int dr)>();
        if (orthogonals) { directions.AddRange([(1, 0), (-1, 0), (0, 1), (0, -1)]); }
        if (diagonals) { directions.AddRange([(1, 1), (-1, 1), (1, -1), (-1, -1)]); }
        var (file, rank) = Parse(ps.CurrentTile.Id);
        foreach (var (df, dr) in directions)
        {
            var f = file;
            var r = rank;
            while (true)
            {
                f = (char)(f + df);
                r += dr;
                if (!IsOnBoard(f, r)) { break; }
                var tileId = $"tile-{f}{r}";
                attacked.Add(tileId);
                // Stop ray if any piece occupies tile (cannot attack beyond)
                var tile = engine.Game.GetTile(tileId);
                if (state.GetPiecesOnTile(tile).Any()) { break; }
            }
        }
    }

    private static (char file, int rank) Parse(string tileId)
    {
        // tile-<file><rank>
        return (tileId[5], tileId[6] - '0');
    }

    private static void AddIfExists(GameEngine engine, HashSet<string> attacked, char file, int rank)
    {
        if (!IsOnBoard(file, rank)) { return; }
        var id = $"tile-{file}{rank}";
        if (engine.Game.GetTile(id) is not null) { attacked.Add(id); }
    }

    private static bool IsOnBoard(char file, int rank)
    {
        return file >= 'a' && file <= 'h' && rank >= 1 && rank <= 8;
    }
}