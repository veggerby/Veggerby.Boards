using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public abstract class InitialGameStateBuilder
    {
        public abstract void Build(Game game);

        private IDictionary<string, string> _piecePositions = new Dictionary<string, string>();

        protected void AddPieceOnTile(string pieceId, string tileId)
        {
            _piecePositions.Add(pieceId, tileId);
        }

        public GameState Compile(Game game)
        {
            Build(game);

            var pieceStates = _piecePositions
                .Select(x => new PieceState(game.GetPiece(x.Key), game.GetTile(x.Value)))
                .ToList();

            return GameState.New(game, pieceStates.ToList());
        }
    }
}