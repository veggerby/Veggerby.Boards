using System.Linq;

namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class BoardDefinition
    {
        public string BoardId { get; set; }
        public TileDefinition[] Tiles { get; set; }
        public PieceDefinition[] Pieces { get; set; }

        public TileDefinition GetTile(string tileId)
        {
            return Tiles.FirstOrDefault(x => string.Equals(x.TileId, tileId));
        }
    }
}
