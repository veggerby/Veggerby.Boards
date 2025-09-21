using System.Linq;

using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

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
        System.ArgumentNullException.ThrowIfNull(to);

        To = to;
        Player = player;
        MaxNumber = maxNumber;
    }

    /// <summary>
    /// Gets the destination tile pieces are moved to.
    /// </summary>
    public Tile To { get; }

    /// <summary>
    /// Gets the player ownership filter controlling which pieces move.
    /// </summary>
    public PlayerOption Player { get; }

    /// <summary>
    /// Gets the optional maximum number of pieces that must not be exceeded for a move to be applied.
    /// </summary>
    public int? MaxNumber { get; }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
    {
        var pieces = gameState.GetPiecesOnTile(@event.To);

        if (Player == PlayerOption.Self)
        {
            pieces = [.. pieces.Where(x => x.Owner.Equals(@event.Piece.Owner))];
        }
        else if (Player == PlayerOption.Opponent)
        {
            pieces = [.. pieces.Where(x => !x.Owner.Equals(@event.Piece.Owner))];
        }

        if (MaxNumber is not null && pieces.Count() > MaxNumber.Value)
        {
            return gameState;
        }

        var newStates = pieces.Select(x => new PieceState(x, To));

        return gameState.Next(newStates);
    }
}