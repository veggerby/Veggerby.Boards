using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
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

        // For regular pieces, check movement direction restrictions
        // Regular pieces can only move forward (black: SE/SW, white: NE/NW)
        // This applies to both normal moves AND capture jumps
        if (path.Directions.Any())
        {
            var direction = path.Directions[0]; // First direction in the path
            var isBlackPiece = piece.Owner.Id == CheckersIds.Players.Black;

            if (isBlackPiece)
            {
                // Black moves south (SE or SW only) for both moves and captures
                if (!direction.Equals(Direction.SouthEast) && 
                    !direction.Equals(Direction.SouthWest))
                {
                    return ConditionResponse.Fail("Regular black pieces can only move forward (SE/SW)");
                }
            }
            else
            {
                // White moves north (NE or NW only) for both moves and captures
                if (!direction.Equals(Direction.NorthEast) && 
                    !direction.Equals(Direction.NorthWest))
                {
                    return ConditionResponse.Fail("Regular white pieces can only move forward (NE/NW)");
                }
            }
        }

        return ConditionResponse.Valid;
    }
}
