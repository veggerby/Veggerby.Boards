using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Centralized semantic predicates over piece identity using role and color metadata.
/// </summary>
public static class ChessPiece
{
    /// <summary>Determines whether the specified piece id has the given role.</summary>
    /// <param name="state">Current immutable game state.</param>
    /// <param name="pieceId">Piece identifier.</param>
    /// <param name="role">Role to test.</param>
    public static bool IsRole(GameState state, string pieceId, ChessPieceRole role)
    {
        var roles = state.GetExtras<ChessPieceRolesExtras>();
        return ChessPieceRoles.TryGetRole(roles, pieceId, out var r) && r == role;
    }

    /// <summary>Determines whether the specified piece id has the given color.</summary>
    /// <param name="state">Current immutable game state.</param>
    /// <param name="pieceId">Piece identifier.</param>
    /// <param name="color">Color to test.</param>
    public static bool IsColor(GameState state, string pieceId, ChessPieceColor color)
    {
        var colors = state.GetExtras<ChessPieceColorsExtras>();
        return ChessPieceColors.TryGetColor(colors, pieceId, out var c) && c == color;
    }

    /// <summary>Returns true when the piece is a king.</summary>
    public static bool IsKing(GameState state, string pieceId) => IsRole(state, pieceId, ChessPieceRole.King);
    /// <summary>Returns true when the piece is a queen.</summary>
    public static bool IsQueen(GameState state, string pieceId) => IsRole(state, pieceId, ChessPieceRole.Queen);
    /// <summary>Returns true when the piece is a rook.</summary>
    public static bool IsRook(GameState state, string pieceId) => IsRole(state, pieceId, ChessPieceRole.Rook);
    /// <summary>Returns true when the piece is a bishop.</summary>
    public static bool IsBishop(GameState state, string pieceId) => IsRole(state, pieceId, ChessPieceRole.Bishop);
    /// <summary>Returns true when the piece is a knight.</summary>
    public static bool IsKnight(GameState state, string pieceId) => IsRole(state, pieceId, ChessPieceRole.Knight);
    /// <summary>Returns true when the piece is a pawn.</summary>
    public static bool IsPawn(GameState state, string pieceId) => IsRole(state, pieceId, ChessPieceRole.Pawn);
    /// <summary>Returns true when the piece belongs to the white side.</summary>
    public static bool IsWhite(GameState state, string pieceId) => IsColor(state, pieceId, ChessPieceColor.White);
    /// <summary>Returns true when the piece belongs to the black side.</summary>
    public static bool IsBlack(GameState state, string pieceId) => IsColor(state, pieceId, ChessPieceColor.Black);
    /// <summary>Returns true when the piece is a sliding piece (bishop, rook, queen).</summary>
    public static bool IsSliding(GameState state, string pieceId) => IsBishop(state, pieceId) || IsRook(state, pieceId) || IsQueen(state, pieceId);
}