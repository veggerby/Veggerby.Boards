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

        public override RuleCheckState Check(Game game, GameState currentState, MovePieceGameEvent @event)
        {
            var activeTurn = currentState.GetActiveTurn();

            // check if piece owner is active in turn
            if (activeTurn == null || !(@event.Piece?.Owner.Equals(activeTurn.Artifact) ?? true))
            {
                return RuleCheckState.Fail("InvalidTurn");
            }

            var pieceState = currentState.GetState<PieceState>(@event.Piece);

            if (pieceState == null)
            {
                return RuleCheckState.Fail("InvalidPiece");
            }

            if (!pieceState.CurrentTile.Equals(@event.From))
            {
                return RuleCheckState.Fail("InvalidEventFrom");
            }

            var path = GetPath(currentState, pieceState, @event.From, @event.To);
            
            if (path == null || !path.From.Equals(@event.From) || !path.To.Equals(@event.To))
            {
                return RuleCheckState.Fail("InvalidPath");
            }

            if (!CanMovePath(currentState, pieceState, path))
            {
                return RuleCheckState.Fail("NoMovePath");
            }

            return RuleCheckState.Valid;
        }

        public override GameState Evaluate(Game game, GameState currentState, MovePieceGameEvent @event)
        {
            var newPieceState = new PieceState(@event.Piece, @event.To);
            return currentState.Update(newPieceState);
        }
    }
}