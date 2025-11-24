using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Moves existing pieces from the target tile of a triggering move event to a configured destination tile.
/// </summary>
/// <remarks>
/// The mutator inspects the <see cref="MovePieceGameEvent.To"/> tile (the tile a piece has just been moved onto) and relocates
/// pieces already present there to <see cref="To"/>. This can be used to model capture, stacking rules, or displacement mechanics.
/// Player filtering is controlled via <see cref="Player"/>. If <see cref="MaxNumber"/> is specified and the number of matching pieces
/// exceeds the limit, no movement is performed to avoid partial ambiguous moves.
/// </remarks>
public class MovePiecesFromToTileStateMutator : IStateMutator<MovePieceGameEvent>
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="to">Destination tile to move pieces onto.</param>
    /// <param name="player">Player filter determining which owners' pieces are moved.</param>
    /// <param name="maxNumber">Optional maximum number of pieces allowed on the source tile for the move to occur.</param>
    public MovePiecesFromToTileStateMutator(Tile to, PlayerOption player, int? maxNumber = null)
    {
        ArgumentNullException.ThrowIfNull(to);

        To = to;
        Player = player;
        MaxNumber = maxNumber;
    }

    /// <summary>
    /// Gets the destination tile pieces are moved to.
    /// </summary>
    public Tile To
    {
        get;
    }

    /// <summary>
    /// Gets the player ownership filter controlling which pieces move.
    /// </summary>
    public PlayerOption Player
    {
        get;
    }

    /// <summary>
    /// Gets the optional maximum number of pieces that must not be exceeded for a move to be applied.
    /// </summary>
    public int? MaxNumber
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
        var pieces = new List<Piece>();

        // Filter pieces by player ownership
        if (Player == PlayerOption.Self)
        {
            foreach (var piece in allPieces)
            {
                if (piece.Owner.Equals(@event.Piece.Owner))
                {
                    pieces.Add(piece);
                }
            }
        }
        else if (Player == PlayerOption.Opponent)
        {
            foreach (var piece in allPieces)
            {
                if (!piece.Owner.Equals(@event.Piece.Owner))
                {
                    pieces.Add(piece);
                }
            }
        }
        else
        {
            foreach (var piece in allPieces)
            {
                pieces.Add(piece);
            }
        }

        // Check max count constraint
        if (MaxNumber is not null && pieces.Count > MaxNumber.Value)
        {
            return gameState;
        }

        // Build new states
        var newStates = new List<PieceState>(pieces.Count);
        foreach (var piece in pieces)
        {
            newStates.Add(new PieceState(piece, To));
        }

        return gameState.Next(newStates);
    }
}