using System.Linq;

namespace Veggerby.Boards.Core.Artifacts
{
    public static class GameExtensions
    {
        public static Piece GetPiece(this Game game, string id)
        {
            return game.ChildArtifacts.SingleOrDefault(x => string.Equals(x.Id, id));
        }
        
        public static Tile GetTile(this Game game, string id)
        {
            return game.Board.GetTile(id);
        }
    }
}