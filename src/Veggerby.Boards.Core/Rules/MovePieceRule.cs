using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class MovePieceRule : Rule<MovePieceGameEvent>
    {
        protected abstract bool CanMove(GameState currentState, State<Piece> piece, State<Tile> from, State<Tile> to);

        public override GameState GetState(GameState currentState, MovePieceGameEvent @event)
        {
            var pieceState = currentState.GetState(@event.Piece);
            var fromState = currentState.GetState(@event.From);
            var toState = currentState.GetState(@event.To);

            if (!CanMove(currentState, pieceState, fromState, toState)) 
            {
                return null;
            }

            var newPieceState = new PieceState(@event.Piece, @event.To);
            var newFromState = new TileState(@event.From, (@fromState as TileState).CurrentPieces.Except(@event.Piece));
            var newToState = new TileState(@event.To,  (@toState as TileState).CurrentPieces.Append(@event.Piece));

            return currentState.Update(new IState[] { newPieceState, newFromState, newToState });
        }
    }
}