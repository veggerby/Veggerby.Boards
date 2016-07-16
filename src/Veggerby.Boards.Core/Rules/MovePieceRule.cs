using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public abstract class MovePieceRule : Rule<MovePieceGameEvent>
    {
        protected abstract TilePath GetPath(GameState currentState, State<Piece> piece, Tile from, Tile to);

        public override GameState GetState(GameState currentState, MovePieceGameEvent @event)
        {
            var pieceState = currentState.GetState(@event.Piece);
            var path = GetPath(currentState, pieceState, @event.From, @event.To);
            
            if (path == null)
            {
                return null;
            }

            var newPieceState = new PieceState(@event.Piece, @event.To);

            return currentState.Update(new IState[] { newPieceState, });
        }
    }
}