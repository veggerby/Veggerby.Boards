using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Immutable chess-specific auxiliary state captured per <see cref="GameState"/> snapshot.
/// </summary>
/// <param name="WhiteCanCastleKingSide">White still retains king-side (short) castling right.</param>
/// <param name="WhiteCanCastleQueenSide">White still retains queen-side (long) castling right.</param>
/// <param name="BlackCanCastleKingSide">Black still retains king-side (short) castling right.</param>
/// <param name="BlackCanCastleQueenSide">Black still retains queen-side (long) castling right.</param>
/// <param name="EnPassantTargetTileId">If set, the tile id behind a just double-advanced pawn that can be captured en passant this ply (null otherwise).</param>
/// <param name="HalfmoveClock">Number of halfmoves since last pawn advance or capture (for fifty-move rule).</param>
/// <param name="FullmoveNumber">Current full move number (starts at 1, increments after Black's move).</param>
/// <param name="MovedPieceIds">Set of piece ids that have moved at least once (used to infer castling eligibility / future optimizations). Empty array initially.</param>
public sealed record ChessStateExtras(
    bool WhiteCanCastleKingSide,
    bool WhiteCanCastleQueenSide,
    bool BlackCanCastleKingSide,
    bool BlackCanCastleQueenSide,
    string EnPassantTargetTileId,
    int HalfmoveClock,
    int FullmoveNumber,
    string[] MovedPieceIds);