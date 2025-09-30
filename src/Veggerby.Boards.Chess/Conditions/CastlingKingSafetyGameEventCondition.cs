using System;
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
        var rolesExtras = state.GetExtras<ChessPieceRolesExtras>();
        if (!ChessPieceRoles.TryGetRole(rolesExtras, @event.Piece.Id, out var movingRole) || movingRole != ChessPieceRole.King) { return ConditionResponse.Ignore("Not a king"); }
        var isWhite = @event.Piece.Owner?.Id == ChessIds.Players.White;
        var startId = isWhite ? ChessIds.Tiles.E1 : ChessIds.Tiles.E8;
        if (@event.From?.Id != startId) { return ConditionResponse.Ignore("Not from initial square"); }
        if (@event.To is null) { return ConditionResponse.Ignore("Missing destination"); }
        var destId = @event.To.Id;
        bool kingSide = destId == (isWhite ? ChessIds.Tiles.G1 : ChessIds.Tiles.G8);
        bool queenSide = destId == (isWhite ? ChessIds.Tiles.C1 : ChessIds.Tiles.C8);
        if (!kingSide && !queenSide) { return ConditionResponse.Ignore("Not a castling target"); }

        // define the three squares to validate
        var middle = kingSide ? (isWhite ? ChessIds.Tiles.F1 : ChessIds.Tiles.F8) : (isWhite ? ChessIds.Tiles.D1 : ChessIds.Tiles.D8);
        var targets = new string[3];
        targets[0] = startId; targets[1] = middle; targets[2] = destId;

        // quick lookup without HashSet: just linear scan of small fixed array
        bool IsTarget(string id) => id == targets[0] || id == targets[1] || id == targets[2];

        // Early exit search: iterate opponent pieces and return as soon as any target is attacked.
        var opponent = isWhite ? ChessIds.Players.Black : ChessIds.Players.White;
        foreach (var ps in state.GetStates<PieceState>())
        {
            if (ps.Artifact.Owner?.Id != opponent) { continue; }
            var id = ps.Artifact.Id;
            if (!ChessPieceRoles.TryGetRole(rolesExtras, id, out var pieceRole)) { continue; }
            var (file, rank) = Parse(ps.CurrentTile.Id);
            if (pieceRole == ChessPieceRole.Pawn)
            {
                int dir = opponent == ChessIds.Players.White ? 1 : -1;
                var pawnHit = CheckPawnAttack(file, rank, (char)(file + 1), rank + dir) ?? CheckPawnAttack(file, rank, (char)(file - 1), rank + dir);
                if (pawnHit is not null) { return ConditionResponse.Fail($"Castling path square {pawnHit} is under attack"); }
            }
            else if (pieceRole == ChessPieceRole.Knight)
            {
                foreach (var (df, dr) in KnightOffsets)
                {
                    var f = (char)(file + df); var r = rank + dr;
                    if (!IsOnBoard(f, r)) { continue; }
                    var tid = $"tile-{f}{r}";
                    if (IsTarget(tid)) { return ConditionResponse.Fail($"Castling path square {tid} is under attack"); }
                }
            }
            else if (pieceRole == ChessPieceRole.Bishop)
            {
                var hit = ScanSliding(engine, state, file, rank, DiagDirections, IsTarget);
                if (hit is not null) { return ConditionResponse.Fail($"Castling path square {hit} is under attack"); }
            }
            else if (pieceRole == ChessPieceRole.Rook)
            {
                var hit = ScanSliding(engine, state, file, rank, OrthoDirections, IsTarget);
                if (hit is not null) { return ConditionResponse.Fail($"Castling path square {hit} is under attack"); }
            }
            else if (pieceRole == ChessPieceRole.Queen)
            {
                var hit = ScanSliding(engine, state, file, rank, DiagDirections, IsTarget);
                hit ??= ScanSliding(engine, state, file, rank, OrthoDirections, IsTarget);
                if (hit is not null) { return ConditionResponse.Fail($"Castling path square {hit} is under attack"); }
            }
            else if (pieceRole == ChessPieceRole.King)
            {
                for (int df = -1; df <= 1; df++)
                {
                    for (int dr = -1; dr <= 1; dr++)
                    {
                        if (df == 0 && dr == 0) { continue; }
                        var f = (char)(file + df); var r2 = rank + dr;
                        if (!IsOnBoard(f, r2)) { continue; }
                        var tid = $"tile-{f}{r2}";
                        if (IsTarget(tid)) { return ConditionResponse.Fail($"Castling path square {tid} is under attack"); }
                    }
                }
            }
        }

        return ConditionResponse.Valid;

        // local helpers
        string CheckPawnAttack(char pawnFile, int pawnRank, char attackFile, int attackRank)
        {
            if (!IsOnBoard(attackFile, attackRank)) { return null; }
            var tid = $"tile-{attackFile}{attackRank}";
            return IsTarget(tid) ? tid : null;
        }
    }

    private static string ScanSliding(GameEngine engine, GameState state, char file, int rank, (int df, int dr)[] directions, Func<string, bool> isTarget)
    {
        foreach (var (df, dr) in directions)
        {
            var f = file; var r = rank;
            while (true)
            {
                f = (char)(f + df); r += dr;
                if (!IsOnBoard(f, r)) { break; }
                var tid = $"tile-{f}{r}";
                if (isTarget(tid)) { return tid; }
                var tile = engine.Game.GetTile(tid);
                if (state.GetPiecesOnTile(tile).Any()) { break; }
            }
        }
        return null;
    }

    private static (char file, int rank) Parse(string tileId) => (tileId[5], tileId[6] - '0');

    private static bool IsOnBoard(char file, int rank) => file >= 'a' && file <= 'h' && rank is >= 1 and <= 8;
}