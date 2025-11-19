using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Centralized semantic predicates over piece identity using role and color metadata.
/// </summary>
public static class ChessPiece
{
    /// <summary>Determines whether the specified piece id has the given role using piece metadata.</summary>
    /// <param name="game">Game containing pieces.</param>
    /// <param name="pieceId">Piece identifier.</param>
    /// <param name="role">Role to test.</param>
    public static bool IsRole(Game game, string pieceId, ChessPieceRole role)
    {
        var piece = game.GetPiece(pieceId);
        return piece?.Metadata is ChessPieceMetadata meta && meta.Role == role;
    }

    /// <summary>Determines whether the specified piece id has the given color using piece metadata.</summary>
    /// <param name="game">Game containing pieces.</param>
    /// <param name="pieceId">Piece identifier.</param>
    /// <param name="color">Color to test.</param>
    public static bool IsColor(Game game, string pieceId, ChessPieceColor color)
    {
        var piece = game.GetPiece(pieceId);
        return piece?.Metadata is ChessPieceMetadata meta && meta.Color == color;
    }

    /// <summary>Returns true when the piece is a king (metadata-based).</summary>
    public static bool IsKing(Game game, string pieceId) => IsRole(game, pieceId, ChessPieceRole.King);

    /// <summary>Returns true when the piece is a queen (metadata-based).</summary>
    public static bool IsQueen(Game game, string pieceId) => IsRole(game, pieceId, ChessPieceRole.Queen);

    /// <summary>Returns true when the piece is a rook (metadata-based).</summary>
    public static bool IsRook(Game game, string pieceId) => IsRole(game, pieceId, ChessPieceRole.Rook);

    /// <summary>Returns true when the piece is a bishop (metadata-based).</summary>
    public static bool IsBishop(Game game, string pieceId) => IsRole(game, pieceId, ChessPieceRole.Bishop);

    /// <summary>Returns true when the piece is a knight (metadata-based).</summary>
    public static bool IsKnight(Game game, string pieceId) => IsRole(game, pieceId, ChessPieceRole.Knight);

    /// <summary>Returns true when the piece is a pawn (metadata-based).</summary>
    public static bool IsPawn(Game game, string pieceId) => IsRole(game, pieceId, ChessPieceRole.Pawn);

    /// <summary>Returns true when the piece belongs to the white side (metadata-based).</summary>
    public static bool IsWhite(Game game, string pieceId) => IsColor(game, pieceId, ChessPieceColor.White);

    /// <summary>Returns true when the piece belongs to the black side (metadata-based).</summary>
    public static bool IsBlack(Game game, string pieceId) => IsColor(game, pieceId, ChessPieceColor.Black);

    /// <summary>Returns true when the piece is a sliding piece (bishop, rook, queen) (metadata-based).</summary>
    public static bool IsSliding(Game game, string pieceId) => IsBishop(game, pieceId) || IsRook(game, pieceId) || IsQueen(game, pieceId);
}