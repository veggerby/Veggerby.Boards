using System;
using System.Linq;

using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

public class ClearToTileStateMutator : IStateMutator<MovePieceGameEvent>
{
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

    public Tile NewTile { get; }
    public PlayerOption Player { get; }
    public int? MaxPieceCount { get; }

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
            pieces = pieces.Where(x => x.Owner.Equals(@event.Piece.Owner)).ToList();
        }
        else if ((Player & PlayerOption.Opponent) != 0)
        {
            pieces = pieces.Where(x => !x.Owner.Equals(@event.Piece.Owner)).ToList();
        }

        if (MaxPieceCount is not null && pieces.Count() > MaxPieceCount.Value)
        {
            throw new BoardException($"Cannot clear more than {MaxPieceCount} pieces from To tile");
        }

        var newStates = pieces.Select(piece => new PieceState(piece, NewTile));

        return gameState.Next(newStates);
    }
}