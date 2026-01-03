using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Othello.Conditions;

/// <summary>
/// Validates that a disc placement in Othello is legal.
/// A placement is valid if it flips at least one opponent disc.
/// </summary>
/// <remarks>
/// In Othello, a move is legal only if it results in at least one opponent disc being flipped.
/// Flipping occurs when placing a disc creates a straight line (horizontal, vertical, or diagonal)
/// of opponent discs bounded by the newly placed disc and another disc of the same color.
/// </remarks>
public sealed class ValidPlacementCondition : IGameEventCondition<PlaceDiscGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidPlacementCondition"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public ValidPlacementCondition(Game game)
    {
        _game = game;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, PlaceDiscGameEvent evt)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(evt);

        if (evt.Disc == null || evt.Target == null)
        {
            return ConditionResponse.Invalid("Disc and tile must be specified");
        }

        // Check if tile is already occupied
        var piecesOnTile = state.GetPiecesOnTile(evt.Target);
        if (piecesOnTile.Any())
        {
            return ConditionResponse.Invalid("Tile is already occupied");
        }

        // Check if this placement would flip at least one opponent disc
        var directions = _game.Directions;
        var opponentColor = GetOpponentColor(evt.Disc);
        var hasFlips = false;

        foreach (var direction in directions)
        {
            if (WouldFlipInDirection(state, evt.Target, evt.Disc, direction, opponentColor))
            {
                hasFlips = true;
                break;
            }
        }

        if (!hasFlips)
        {
            return ConditionResponse.Invalid("Placement must flip at least one opponent disc");
        }

        return ConditionResponse.Valid;
    }

    private bool WouldFlipInDirection(GameState state, Tile startTile, Piece piece, Direction direction, OthelloDiscColor opponentColor)
    {
        var currentTile = startTile;
        var opponentPiecesFound = new List<Piece>();

        // Move in the direction, collecting opponent pieces
        while (true)
        {
            var relation = _game.Board.GetRelation(currentTile, direction);
            if (relation == null)
            {
                // Reached edge of board without finding our piece
                return false;
            }

            currentTile = relation.To;
            var piecesOnTile = state.GetPiecesOnTile(currentTile).ToList();

            if (!piecesOnTile.Any())
            {
                // Empty tile - no flip possible
                return false;
            }

            var tilepiece = piecesOnTile.First();
            var currentColor = OthelloHelper.GetCurrentDiscColor(tilepiece, state);

            if (currentColor == opponentColor)
            {
                // Found an opponent piece, keep going
                opponentPiecesFound.Add(tilepiece);
            }
            else
            {
                // Found our own color piece
                // This is valid only if we have opponent pieces in between
                return opponentPiecesFound.Count > 0;
            }
        }
    }

    private OthelloDiscColor GetOpponentColor(Piece piece)
    {
        var metadata = piece.Metadata as OthelloDiscMetadata;
        if (metadata == null)
        {
            throw new InvalidOperationException("Piece does not have Othello metadata");
        }

        return metadata.Color == OthelloDiscColor.Black ? OthelloDiscColor.White : OthelloDiscColor.Black;
    }
}
