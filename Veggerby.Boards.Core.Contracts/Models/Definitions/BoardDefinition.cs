using System.Linq;

namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class BoardDefinition
    {
        private readonly string _boardId;
        private readonly TileDefinition[] _tiles;
        private readonly TileRelationDefinition[] _tileRelations;
        private readonly PieceDefinition[] _pieces;

        public BoardDefinition(string boardId, TileDefinition[] tiles, TileRelationDefinition[] tileRelations, PieceDefinition[] pieces)
        {
            _boardId = boardId;
            _tiles = tiles;
            _tileRelations = tileRelations;
            _pieces = pieces;
        }

        public string BoardId => _boardId;

        public TileDefinition GetTile(string tileId)
        {
            return _tiles.FirstOrDefault(x => string.Equals(x.TileId, tileId));
        }

        public TileRelationDefinition GetTileRelation(string sourceTileId, string destinationTileId)
        {
            return _tileRelations.FirstOrDefault(x => string.Equals(x.SourceTile.TileId, sourceTileId) && string.Equals(x.DestinationTile.TileId, destinationTileId));
        }
    }
}
