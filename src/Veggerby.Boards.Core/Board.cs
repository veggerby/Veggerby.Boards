using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core
{
    public class Board : Artifact
    {
        private readonly IEnumerable<Tile> _tiles;

        private readonly IEnumerable<TileRelation> _tileRelations;

        public Board(string id, IEnumerable<Tile> tiles, IEnumerable<TileRelation> tileRelations) : base(id)
        {
            _tiles = (tiles ?? Enumerable.Empty<Tile>()).ToList();
            _tileRelations = (tileRelations ?? Enumerable.Empty<TileRelation>()).ToList();
        }

        public IEnumerable<Tile> GetTiles() 
        {
            return _tiles;
        }
    }
}