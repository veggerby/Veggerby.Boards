using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public static class GameStateExtensions
    {
        public static Player GetActivePlayer(this GameState gameState)
        {
            return gameState
                .GetStates<ActivePlayerState>()
                .Where(x => x.IsActive)
                .Select(x => x.Artifact)
                .Single();
        }

        public static IEnumerable<Piece> GetPiecesOnTile(this GameState gameState, Tile tile, Player owner = null)
        {
            return gameState
                .GetStates<PieceState>()
                .Where(x => x.CurrentTile == tile && (owner == null || (x.Artifact).Owner.Equals(owner)))
                .Select(x => x.Artifact)
                .ToList();
        }
    }
}