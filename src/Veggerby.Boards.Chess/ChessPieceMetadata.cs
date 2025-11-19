using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Chess-specific piece metadata containing role and color.
/// </summary>
/// <param name="Role">The chess role (pawn, knight, bishop, etc.).</param>
/// <param name="Color">The piece color (white or black).</param>
public sealed record ChessPieceMetadata(ChessPieceRole Role, ChessPieceColor Color) : IPieceMetadata;
