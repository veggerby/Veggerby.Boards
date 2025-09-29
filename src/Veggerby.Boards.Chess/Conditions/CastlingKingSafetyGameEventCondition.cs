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
    private static readonly (int df, int dr)[] KnightOffsets =
    [
        (1, 2), (2, 1), (2, -1), (1, -2),
        (-1, -2), (-2, -1), (-2, 1), (-1, 2)
    ];

    private static readonly (int df, int dr)[] OrthoDirections = [(1, 0), (-1, 0), (0, 1), (0, -1)];
    private static readonly (int df, int dr)[] DiagDirections = [(1, 1), (-1, 1), (1, -1), (-1, -1)];

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        if (!@event.Piece.Id.EndsWith("-king")) { return ConditionResponse.Ignore("Not a king"); }
        var isWhite = @event.Piece.Owner?.Id == "white";
        var startId = isWhite ? "tile-e1" : "tile-e8";
        if (@event.From?.Id != startId) { return ConditionResponse.Ignore("Not from initial square"); }
        if (@event.To is null) { return ConditionResponse.Ignore("Missing destination"); }
        var destId = @event.To.Id;
        bool kingSide = destId == (isWhite ? "tile-g1" : "tile-g8");
        bool queenSide = destId == (isWhite ? "tile-c1" : "tile-c8");
        if (!kingSide && !queenSide) { return ConditionResponse.Ignore("Not a castling target"); }

        // define the three squares to validate
        var middle = kingSide ? (isWhite ? "tile-f1" : "tile-f8") : (isWhite ? "tile-d1" : "tile-d8");
        var targets = new string[3];
        targets[0] = startId; targets[1] = middle; targets[2] = destId;

        // quick lookup without HashSet: just linear scan of small fixed array
        bool IsTarget(string id) => id == targets[0] || id == targets[1] || id == targets[2];

        // Early exit search: iterate opponent pieces and return as soon as any target is attacked.
        var opponent = isWhite ? "black" : "white";
        foreach (var ps in state.GetStates<PieceState>())
        {
            if (ps.Artifact.Owner?.Id != opponent) { continue; }
            var id = ps.Artifact.Id;
            var (file, rank) = Parse(ps.CurrentTile.Id);

            if (id.Contains("pawn", StringComparison.Ordinal))
            {
                int dir = opponent == "white" ? 1 : -1;
                if (CheckPawnAttack(file, rank, (char)(file + 1), rank + dir) || CheckPawnAttack(file, rank, (char)(file - 1), rank + dir))
                { return ConditionResponse.Fail("Castling path square under attack"); }
            }
            else if (id.Contains("knight", StringComparison.Ordinal))
            {
                foreach (var (df, dr) in KnightOffsets)
                {
                    var f = (char)(file + df); var r = rank + dr;
                    if (!IsOnBoard(f, r)) { continue; }
                    var tid = $"tile-{f}{r}";
                    if (IsTarget(tid)) { return ConditionResponse.Fail("Castling path square under attack"); }
                }
            }
            else if (id.Contains("bishop", StringComparison.Ordinal))
            {
                if (ScanSliding(engine, state, file, rank, DiagDirections, IsTarget)) { return ConditionResponse.Fail("Castling path square under attack"); }
            }
            else if (id.Contains("rook", StringComparison.Ordinal))
            {
                if (ScanSliding(engine, state, file, rank, OrthoDirections, IsTarget)) { return ConditionResponse.Fail("Castling path square under attack"); }
            }
            else if (id.Contains("queen", StringComparison.Ordinal))
            {
                if (ScanSliding(engine, state, file, rank, DiagDirections, IsTarget) || ScanSliding(engine, state, file, rank, OrthoDirections, IsTarget))
                { return ConditionResponse.Fail("Castling path square under attack"); }
            }
            else if (id.EndsWith("-king", StringComparison.Ordinal))
            {
                for (int df = -1; df <= 1; df++)
                {
                    for (int dr = -1; dr <= 1; dr++)
                    {
                        if (df == 0 && dr == 0) { continue; }
                        var f = (char)(file + df); var r2 = rank + dr;
                        if (!IsOnBoard(f, r2)) { continue; }
                        var tid = $"tile-{f}{r2}";
                        if (IsTarget(tid)) { return ConditionResponse.Fail("Castling path square under attack"); }
                    }
                }
            }
        }

        return ConditionResponse.Valid;

        // local helpers
        bool CheckPawnAttack(char pawnFile, int pawnRank, char attackFile, int attackRank)
        {
            if (!IsOnBoard(attackFile, attackRank)) { return false; }
            var tid = $"tile-{attackFile}{attackRank}";
            return IsTarget(tid);
        }
    }

    private static bool ScanSliding(GameEngine engine, GameState state, char file, int rank, (int df, int dr)[] directions, Func<string, bool> isTarget)
    {
        foreach (var (df, dr) in directions)
        {
            var f = file; var r = rank;
            while (true)
            {
                f = (char)(f + df); r += dr;
                if (!IsOnBoard(f, r)) { break; }
                var tid = $"tile-{f}{r}";
                if (isTarget(tid)) { return true; }
                var tile = engine.Game.GetTile(tid);
                if (state.GetPiecesOnTile(tile).Any()) { break; }
            }
        }
        return false;
    }

    private static (char file, int rank) Parse(string tileId) => (tileId[5], tileId[6] - '0');

    private static bool IsOnBoard(char file, int rank) => file >= 'a' && file <= 'h' && rank is >= 1 and <= 8;
}