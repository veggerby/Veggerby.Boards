using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators
{
    public class MovePiecesFromToTileStateMutator : IStateMutator<MovePieceGameEvent>
    {
        public MovePiecesFromToTileStateMutator(Tile to, PlayerOption player, int? maxNumber = null)
        {
            if (to == null)
            {
                throw new System.ArgumentNullException(nameof(to));
            }

            To = to;
            Player = player;
            MaxNumber = maxNumber;
        }

        public Tile To { get; }
        public PlayerOption Player { get; }
        public int? MaxNumber { get; }

        public GameState MutateState(GameEngine engine, GameState gameState, MovePieceGameEvent @event)
        {
            var pieces = gameState.GetPiecesOnTile(@event.To);

            if (Player == PlayerOption.Self)
            {
                pieces = pieces.Where(x => x.Owner.Equals(@event.Piece.Owner)).ToList();
            }
            else if (Player == PlayerOption.Opponent)
            {
                pieces = pieces.Where(x => !x.Owner.Equals(@event.Piece.Owner)).ToList();
            }

            if (MaxNumber != null && pieces.Count() > MaxNumber.Value)
            {
                return gameState;
            }

            var newStates = pieces.Select(x => new PieceState(x, To));

            return gameState.Next(newStates);
        }
    }
}