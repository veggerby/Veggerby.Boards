using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class MovePieceRule : Rule<MovePieceGameEvent>
    {
        protected abstract TilePath GetPath(GameState currentState, State<Piece> piece, Tile from, Tile to);

        protected virtual bool CanMovePath(GameState currentState, State<Piece> piece, TilePath path)
        {
            return true;
        }

        public override GameState GetState(GameEngine gameEngine, GameState currentState, MovePieceGameEvent @event)
        {
            // check if piece owner is current in turn
            if (!(@event.Piece?.Owner.Equals(currentState.ActiveTurn.Turn.Player) ?? true))
            {
                return null;
            }

            var pieceState = currentState.GetState(@event.Piece);
            var path = GetPath(currentState, pieceState, @event.From, @event.To);
            
            if (path == null)
            {
                return null;
            }

            if (!CanMovePath(currentState, pieceState, path))
            {
                return null;
            }

            var newPieceState = new PieceState(@event.Piece, @event.To);

            return currentState.Update(new IState[] { newPieceState, });
        }
    }
}