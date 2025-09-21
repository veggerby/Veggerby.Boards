using System;
using System.Linq;

using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

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
    public Tile NewTile { get; }
    /// <summary>
    /// Gets which players' pieces are considered.
    /// </summary>
    public PlayerOption Player { get; }
    /// <summary>
    /// Gets the maximum number of pieces permitted (throws if exceeded).
    /// </summary>
    public int? MaxPieceCount { get; }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
    {
        var pieces = gameState.GetPiecesOnTile(@event.To);

        if (!pieces.Any())
        {
            return gameState;
        }

        if ((Player & PlayerOption.Any) == PlayerOption.Any)
        {
        }
        else if ((Player & PlayerOption.Self) != 0)
        {
            pieces = [.. pieces.Where(x => x.Owner.Equals(@event.Piece.Owner))];
        }
        else if ((Player & PlayerOption.Opponent) != 0)
        {
            pieces = [.. pieces.Where(x => !x.Owner.Equals(@event.Piece.Owner))];
        }

        if (MaxPieceCount is not null && pieces.Count() > MaxPieceCount.Value)
        {
            throw new BoardException($"Cannot clear more than {MaxPieceCount} pieces from To tile");
        }

        var newStates = pieces.Select(piece => new PieceState(piece, NewTile));

        return gameState.Next(newStates);
    }
}