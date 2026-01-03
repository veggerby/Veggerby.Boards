using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Othello.Mutators;

/// <summary>
/// Flips opponent discs when a new disc is placed in Othello.
/// </summary>
/// <remarks>
/// When a disc is placed, all opponent discs in straight lines (horizontal, vertical, diagonal)
/// that are sandwiched between the newly placed disc and another disc of the same color are flipped.
/// </remarks>
public sealed class FlipDiscsStateMutator : IStateMutator<PlaceDiscGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlipDiscsStateMutator"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public FlipDiscsStateMutator(Game game)
    {
        _game = game;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, PlaceDiscGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        if (@event.Disc == null || @event.Target == null)
        {
            return gameState;
        }

        var piecesToFlip = new List<Piece>();
        var playerColor = GetDiscColor(@event.Disc);
        var opponentColor = playerColor == OthelloDiscColor.Black ? OthelloDiscColor.White : OthelloDiscColor.Black;

        // Check all eight directions for discs to flip
        var relations = _game.Board.TileRelations.Where(r => r.From.Id == @event.Target.Id);

        foreach (var relation in relations)
        {
            var discsToFlipInDirection = GetDiscsToFlipInDirection(gameState, @event.Target, playerColor, relation.Direction, opponentColor);
            piecesToFlip.AddRange(discsToFlipInDirection);
        }

        if (piecesToFlip.Count == 0)
        {
            return gameState;
        }

        // Create flip states for each flipped disc
        var flipStates = piecesToFlip.Select(p => new FlippedDiscState(p, playerColor)).ToArray();

        return gameState.Next(flipStates);
    }

    private List<Piece> GetDiscsToFlipInDirection(GameState state, Tile startTile, OthelloDiscColor playerColor, Direction direction, OthelloDiscColor opponentColor)
    {
        var currentTile = startTile;
        var opponentPiecesFound = new List<Piece>();

        // Move in the direction, collecting opponent pieces
        while (true)
        {
            var relation = _game.Board.GetTileRelation(currentTile, direction);
            if (relation == null)
            {
                // Reached edge of board without finding our piece - no flips in this direction
                return new List<Piece>();
            }

            currentTile = relation.To;
            var piecesOnTile = state.GetPiecesOnTile(currentTile).ToList();

            if (!piecesOnTile.Any())
            {
                // Empty tile - no flips in this direction
                return new List<Piece>();
            }

            var piece = piecesOnTile.First();
            var metadata = piece.Metadata as OthelloDiscMetadata;

            if (metadata == null)
            {
                return new List<Piece>();
            }

            if (metadata.Color == opponentColor)
            {
                // Found an opponent piece, collect it
                opponentPiecesFound.Add(piece);
            }
            else if (metadata.Color == playerColor)
            {
                // Found our own color piece - return all opponent pieces found
                return opponentPiecesFound;
            }
        }
    }

    private OthelloDiscColor GetDiscColor(Piece piece)
    {
        var metadata = piece.Metadata as OthelloDiscMetadata;
        if (metadata == null)
        {
            throw new InvalidOperationException("Piece does not have Othello metadata");
        }

        return metadata.Color;
    }
}
