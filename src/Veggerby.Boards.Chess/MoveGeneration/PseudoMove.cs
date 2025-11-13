using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Chess.MoveGeneration;

/// <summary>
/// Represents a candidate move before legality filtering (king safety check).
/// </summary>
/// <param name="Piece">The piece to be moved.</param>
/// <param name="From">Source tile.</param>
/// <param name="To">Destination tile.</param>
/// <param name="Kind">Special move classification.</param>
/// <param name="PromotionRole">Promotion target role (e.g., "queen") if Kind is Promotion, otherwise null.</param>
/// <param name="IsCapture">True if this move captures an opponent piece.</param>
/// <remarks>
/// Pseudo-legal moves may leave the player's king in check and must be filtered
/// through a legality validator before being considered legal.
/// </remarks>
public sealed record PseudoMove(
    Piece Piece,
    Tile From,
    Tile To,
    PseudoMoveKind Kind,
    string? PromotionRole,
    bool IsCapture);
