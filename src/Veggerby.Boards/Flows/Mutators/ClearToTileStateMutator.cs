using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Relocates pieces found on the destination tile of a move to a configured replacement tile (capture-style clearing).
/// </summary>
public class ClearToTileStateMutator : IStateMutator<MovePieceGameEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearToTileStateMutator"/> class.
    /// </summary>
    /// <param name="newTile">Tile to move cleared pieces onto.</param>
    /// <param name="player">Which players' pieces to clear.</param>
    /// <param name="maxPieceCount">Maximum number allowed to clear (null = unlimited).</param>
    public ClearToTileStateMutator(Tile newTile, PlayerOption player = PlayerOption.Opponent, int? maxPieceCount = 1)
    {
        ArgumentNullException.ThrowIfNull(newTile);

        if (maxPieceCount.HasValue && maxPieceCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPieceCount));
        }

        NewTile = newTile;
        Player = player;
        MaxPieceCount = maxPieceCount;
    }

    /// <summary>
    /// Gets the relocation target tile.
    /// </summary>
    public Tile NewTile
    {
        get;
    }
    /// <summary>
    /// Gets which players' pieces are considered.
    /// </summary>
    public PlayerOption Player
    {
        get;
    }
    /// <summary>
    /// Gets the maximum number of pieces permitted (throws if exceeded).
    /// </summary>
    public int? MaxPieceCount
    {
        get;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var allPieces = gameState.GetPiecesOnTile(@event.To);

        // Check if empty using explicit iteration
        var hasAny = false;
        foreach (var _ in allPieces)
        {
            hasAny = true;
            break;
        }

        if (!hasAny)
        {
            return gameState;
        }

        // Filter pieces by player ownership
        var pieces = new List<Piece>();
        if ((Player & PlayerOption.Any) == PlayerOption.Any)
        {
            foreach (var piece in allPieces)
            {
                pieces.Add(piece);
            }
        }
        else if ((Player & PlayerOption.Self) != 0)
        {
            foreach (var piece in allPieces)
            {
                if (piece.Owner.Equals(@event.Piece.Owner))
                {
                    pieces.Add(piece);
                }
            }
        }
        else if ((Player & PlayerOption.Opponent) != 0)
        {
            foreach (var piece in allPieces)
            {
                if (!piece.Owner.Equals(@event.Piece.Owner))
                {
                    pieces.Add(piece);
                }
            }
        }

        // Check max count constraint
        if (MaxPieceCount is not null && pieces.Count > MaxPieceCount.Value)
        {
            throw new BoardException($"Cannot clear more than {MaxPieceCount} pieces from To tile");
        }

        // Build new states
        var newStates = new List<PieceState>(pieces.Count);
        foreach (var piece in pieces)
        {
            newStates.Add(new PieceState(piece, NewTile));
        }

        return gameState.Next(newStates);
    }
}