using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public class PatternMovePieceRule : MovePieceRule
    {
        protected override bool CanMove(GameState currentState, State<Piece> piece, State<Tile> from, State<Tile> to)
        {
            var game = currentState.Artifact;
            var board = game.Board;

            piece.Artifact.Patterns
        }
    }
}