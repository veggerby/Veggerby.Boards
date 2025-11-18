namespace Veggerby.Boards.Chess.MoveGeneration;

/// <summary>
/// Classifies the type of pseudo-legal move.
/// </summary>
public enum PseudoMoveKind
{
    /// <summary>
    /// Normal piece movement (no special handling).
    /// </summary>
    Normal,

    /// <summary>
    /// Castling move (king and rook coordinate movement).
    /// </summary>
    Castle,

    /// <summary>
    /// En passant capture (pawn captures diagonally to empty square).
    /// </summary>
    EnPassant,

    /// <summary>
    /// Pawn promotion (reaches final rank).
    /// </summary>
    Promotion
}
