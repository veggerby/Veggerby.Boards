namespace Veggerby.Boards.Flows.LegalMoveGeneration;

/// <summary>
/// Structured rejection reasons for move validation diagnostics.
/// </summary>
/// <remarks>
/// <para>
/// This enum provides standardized rejection categories that can be mapped to user-friendly
/// error messages and localized strings. Each reason represents a common failure mode across
/// different game types.
/// </para>
/// <para>
/// For game-specific rejections not covered by these categories, use <see cref="RuleViolation"/>
/// with a detailed explanation string.
/// </para>
/// </remarks>
public enum RejectionReason
{
    /// <summary>
    /// No rejection occurred; move is legal.
    /// </summary>
    None,

    /// <summary>
    /// The artifact (piece, card, etc.) does not belong to the active player.
    /// </summary>
    /// <remarks>
    /// Example: Attempting to move opponent's piece in Chess.
    /// </remarks>
    PieceNotOwned,

    /// <summary>
    /// The movement path is blocked by another artifact.
    /// </summary>
    /// <remarks>
    /// Example: Attempting to move a rook through an occupied square in Chess.
    /// </remarks>
    PathObstructed,

    /// <summary>
    /// The destination location is occupied by a friendly artifact.
    /// </summary>
    /// <remarks>
    /// Example: Attempting to move a piece to a square occupied by own piece in Chess.
    /// </remarks>
    DestinationOccupied,

    /// <summary>
    /// The move does not match any valid movement pattern for the artifact.
    /// </summary>
    /// <remarks>
    /// Example: Attempting to move a bishop orthogonally in Chess.
    /// </remarks>
    InvalidPattern,

    /// <summary>
    /// The action is not allowed in the current game phase.
    /// </summary>
    /// <remarks>
    /// Example: Attempting to move a piece after the game has ended.
    /// </remarks>
    WrongPhase,

    /// <summary>
    /// Insufficient resources (dice values, action points, cards, etc.) to perform the move.
    /// </summary>
    /// <remarks>
    /// Example: Attempting to move 6 spaces with a dice roll of 3 in Backgammon.
    /// </remarks>
    InsufficientResources,

    /// <summary>
    /// The move violates a game-specific rule.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this for game-specific constraints not covered by other categories.
    /// Examples:
    /// <list type="bullet">
    /// <item><description>Chess: Move would leave or place own king in check</description></item>
    /// <item><description>Go: Move violates ko rule</description></item>
    /// <item><description>Go: Suicide move (removes own last liberty)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    RuleViolation,

    /// <summary>
    /// The game has already ended.
    /// </summary>
    /// <remarks>
    /// Example: Attempting any move after checkmate in Chess.
    /// </remarks>
    GameEnded
}
