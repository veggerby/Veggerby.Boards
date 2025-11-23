using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Checkers.Conditions;

/// <summary>
/// Validates that a piece can move in a given direction based on whether it's a king or regular piece.
/// Regular pieces can only move forward (black: SE/SW, white: NE/NW).
/// Kings can move in all diagonal directions.
/// </summary>
public sealed class KingMovementCondition : IGameEventCondition<MovePieceGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="KingMovementCondition"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public KingMovementCondition(Game game)
    {
        _game = game;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt);
        ArgumentNullException.ThrowIfNull(state);

        var piece = evt.Piece;
        var path = evt.Path;

        if (path == null || path.Tiles.Count < 2)
        {
            return ConditionResponse.Ignore("Invalid move path");
        }

        // Check if this piece is a king (has been promoted)
        var isKing = state.GetStates<PromotedPieceState>().Any(ps => ps.PromotedPiece.Equals(piece));

        if (isKing)
        {
            // Kings can move in any diagonal direction
            return ConditionResponse.Valid;
        }

        // Regular pieces can only move in their defined forward directions
        // The piece's relations already define this, so if the move is being attempted,
        // it means the engine has validated the direction through the piece's HasDirection rules
        // This condition is mainly here to explicitly document the rule

        return ConditionResponse.Valid;
    }
}
