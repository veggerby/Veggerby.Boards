using Veggerby.Boards.Builder.Fluent;
using Veggerby.Boards.Chess.Conditions;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Reusable condition groups for common chess move validation patterns.
/// </summary>
/// <remarks>
/// These groups reduce duplication in ChessGameBuilder by extracting frequently-used
/// condition combinations into named, discoverable helpers.
/// </remarks>
public static class ChessConditions
{
    /// <summary>
    /// Verifies the piece being moved belongs to the active player.
    /// </summary>
    public static ConditionGroup<MovePieceGameEvent> ActivePlayerMove =>
        ConditionGroup<MovePieceGameEvent>.Create()
            .Require<PieceIsActivePlayerGameEventCondition>();

    /// <summary>
    /// Verifies the path is unobstructed and the destination is empty.
    /// </summary>
    /// <remarks>
    /// Common for normal (non-capture) moves of all pieces except pawns.
    /// </remarks>
    public static ConditionGroup<MovePieceGameEvent> UnobstructedToEmpty =>
        ConditionGroup<MovePieceGameEvent>.Create()
            .Require<PathNotObstructedGameEventCondition>()
            .Require<DestinationIsEmptyGameEventCondition>();

    /// <summary>
    /// Verifies the piece is not a pawn.
    /// </summary>
    /// <remarks>
    /// Used to separate generic piece movement from pawn-specific rules.
    /// </remarks>
    public static ConditionGroup<MovePieceGameEvent> NonPawn =>
        ConditionGroup<MovePieceGameEvent>.Create()
            .Require<NonPawnGameEventCondition>();

    /// <summary>
    /// Verifies the path is unobstructed and the destination has an opponent piece.
    /// </summary>
    /// <remarks>
    /// Common for capture moves of all pieces except pawns.
    /// </remarks>
    public static ConditionGroup<MovePieceGameEvent> UnobstructedToOpponentPiece =>
        ConditionGroup<MovePieceGameEvent>.Create()
            .Require<PathNotObstructedGameEventCondition>()
            .Require<DestinationHasOpponentPieceGameEventCondition>();

    /// <summary>
    /// Verifies pawn diagonal capture conditions (distance one, diagonal direction).
    /// </summary>
    public static ConditionGroup<MovePieceGameEvent> PawnDiagonalCapture =>
        ConditionGroup<MovePieceGameEvent>.Create()
            .Require<DistanceOneGameEventCondition>()
            .Require<DiagonalPawnDirectionGameEventCondition>();

    /// <summary>
    /// Verifies pawn normal move conditions (distance one, forward direction).
    /// </summary>
    public static ConditionGroup<MovePieceGameEvent> PawnNormalMove =>
        ConditionGroup<MovePieceGameEvent>.Create()
            .Require<DistanceOneGameEventCondition>()
            .Require<ForwardPawnDirectionGameEventCondition>();
}
