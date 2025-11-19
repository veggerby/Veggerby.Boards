using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Checkers.Mutators;

/// <summary>
/// Mutator that promotes regular pieces to kings when they reach the opposite end of the board.
/// Black pieces promote on row 8 (tiles 29-32), white pieces on row 1 (tiles 1-4).
/// </summary>
public sealed class PromoteToKingMutator : IStateMutator<MovePieceGameEvent>
{
    private readonly Game _game;

    // Black promotion tiles (row 8)
    private static readonly HashSet<string> BlackPromotionTiles = new(StringComparer.Ordinal)
    {
        CheckersIds.Tiles.Tile29,
        CheckersIds.Tiles.Tile30,
        CheckersIds.Tiles.Tile31,
        CheckersIds.Tiles.Tile32
    };

    // White promotion tiles (row 1)
    private static readonly HashSet<string> WhitePromotionTiles = new(StringComparer.Ordinal)
    {
        CheckersIds.Tiles.Tile1,
        CheckersIds.Tiles.Tile2,
        CheckersIds.Tiles.Tile3,
        CheckersIds.Tiles.Tile4
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="PromoteToKingMutator"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public PromoteToKingMutator(Game game)
    {
        _game = game;
    }

    /// <summary>
    /// Mutates the state to promote pieces to kings if they reached the promotion row.
    /// </summary>
    /// <param name="engine">The game engine.</param>
    /// <param name="state">The current game state.</param>
    /// <param name="event">The move event that triggered this mutator.</param>
    /// <returns>New state with promoted piece if applicable.</returns>
    public GameState MutateState(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(state);

        var piece = @event.Piece;
        var destinationTile = @event.To;

        var metadata = piece.Metadata as CheckersPieceMetadata;

        if (metadata == null)
        {
            return state; // Not a checkers piece
        }

        // Already a king - no promotion needed
        if (metadata.Role == CheckersPieceRole.King)
        {
            return state;
        }

        // Check if destination tile is a promotion tile for this piece's color
        var shouldPromote = false;

        if (metadata.Color == CheckersPieceColor.Black && BlackPromotionTiles.Contains(destinationTile.Id))
        {
            shouldPromote = true;
        }
        else if (metadata.Color == CheckersPieceColor.White && WhitePromotionTiles.Contains(destinationTile.Id))
        {
            shouldPromote = true;
        }

        if (!shouldPromote)
        {
            return state; // Not on promotion tile
        }

        // Promote piece to king by updating its metadata
        // We need to replace the piece's metadata in the game definition
        // Since pieces are immutable, we create a new piece with king metadata
        
        // For now, we'll use a state marker to track promoted pieces
        // A more complete implementation would modify piece movement patterns dynamically
        
        // Add a promoted piece state marker
        var promotedState = new PromotedPieceState(piece);

        return state.Next(new IArtifactState[] { promotedState });
    }
}

/// <summary>
/// State marker indicating a piece has been promoted to king.
/// This affects the piece's available movement directions.
/// </summary>
public sealed class PromotedPieceState : IArtifactState
{
    /// <summary>
    /// Gets the promoted piece.
    /// </summary>
    public Piece PromotedPiece
    {
        get;
    }

    /// <summary>
    /// Gets the artifact (the promoted piece).
    /// </summary>
    public Artifact Artifact => PromotedPiece;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromotedPieceState"/> class.
    /// </summary>
    /// <param name="piece">The piece that was promoted.</param>
    public PromotedPieceState(Piece piece)
    {
        PromotedPiece = piece;
    }

    /// <summary>
    /// Checks equality with another artifact state.
    /// </summary>
    /// <param name="other">Other state to compare.</param>
    /// <returns>True if states are equal.</returns>
    public bool Equals(IArtifactState? other)
    {
        return other is PromotedPieceState promotedState
            && promotedState.PromotedPiece == PromotedPiece;
    }
}
