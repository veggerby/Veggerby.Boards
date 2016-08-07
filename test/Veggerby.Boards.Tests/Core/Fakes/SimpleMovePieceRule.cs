using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class SimpleMovePieceRule : MovePieceRule
    {
        private readonly Tile _from;
        private readonly Tile _to;
        private readonly Direction _direction;
        private readonly bool _canMovePath;

        public SimpleMovePieceRule(Tile from = null, Tile to = null, Direction direction = null, bool canMovePath = true)
        {
            _from = from;
            _to = to;
            _direction = direction;
            _canMovePath = canMovePath;
        }

        protected override bool CanMovePath(GameState currentState, State<Piece> piece, TilePath path)
        {
            return _canMovePath;
        }

        protected override TilePath GetPath(GameState currentState, State<Piece> piece, Tile from, Tile to)
        {
            return new TilePath(new [] { new TileRelation(_from ?? from, _to ?? to, _direction ?? Direction.Any) } );
        }
    }
}