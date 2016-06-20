using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class BoardDefinition
    {
        private readonly IEnumerable<TileDefinition> _tiles;
        private readonly IEnumerable<TileRelationDefinition> _tileRelations;
        private readonly IEnumerable<PieceDefinition> _pieces;

        public BoardDefinition(string boardId, IEnumerable<TileDefinition> tiles, IEnumerable<TileRelationDefinition> tileRelations, IEnumerable<PieceDefinition> pieces)
        {
            BoardId = boardId;
            _tiles = (tiles ?? Enumerable.Empty<TileDefinition>()).ToList();
            _tileRelations = (tileRelations ?? Enumerable.Empty<TileRelationDefinition>()).ToList();;
            _pieces = (pieces ?? Enumerable.Empty<PieceDefinition>()).ToList();;
        }

        public string BoardId { get; }

        public TileDefinition GetTile(string tileId)
        {
            return _tiles.FirstOrDefault(x => string.Equals(x.TileId, tileId));
        }

        public TileRelationDefinition GetTileRelation(string sourceTileId, string destinationTileId)
        {
            return _tileRelations.FirstOrDefault(x => string.Equals(x.SourceTile.TileId, sourceTileId) && string.Equals(x.DestinationTile.TileId, destinationTileId));
        }

        public IEnumerable<TileRelationDefinition> GetTileRelationsFromSource(string sourceTileId)
        {
            return _tileRelations.Where(x => string.Equals(x.SourceTile.TileId, sourceTileId)).ToList();
        }

        public PieceDefinition GetPiece(string pieceId)
        {
            return _pieces.FirstOrDefault(x => string.Equals(x.PieceId, pieceId));
        }
    }
}
