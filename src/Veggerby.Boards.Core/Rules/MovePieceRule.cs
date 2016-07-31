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

        public override bool Check(GameEngine gameEngine, GameState currentState, MovePieceGameEvent @event)
        {
            var activeTurn = currentState.GetActiveTurn();

            // check if piece owner is current in turn
            if (activeTurn == null || !(@event.Piece?.Owner.Equals(activeTurn.Artifact) ?? true))
            {
                return false;
            }

            var pieceState = currentState.GetState<PieceState>(@event.Piece);
            var path = GetPath(currentState, pieceState, @event.From, @event.To);
            
            if (path == null || !path.From.Equals(@event.From) || !path.To.Equals(@event.To))
            {
                return false;
            }

            if (!CanMovePath(currentState, pieceState, path))
            {
                return false;
            }

            return true;
        }

        public override GameState Evaluate(GameEngine gameEngine, GameState currentState, MovePieceGameEvent @event)
        {
            var newPieceState = new PieceState(@event.Piece, @event.To);
            return currentState.Update(new IState[] { newPieceState, });
        }
    }
}