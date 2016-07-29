using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Rules
{
    public class PatternMovePieceRule : MovePieceRule
    {
        protected override TilePath GetPath(GameState currentState, State<Piece> piece, Tile from, Tile to)
        {
            var board = currentState.Game.Board;

            foreach (var pattern in piece.Artifact.Patterns)
            {
                var visitor = new ResolveTilePathPatternVisitor(board, from, to);
                pattern.Accept(visitor);
                if (visitor.ResultPath != null)
                {
                    return visitor.ResultPath;
                }
            }

            return null;
        }
    }
}