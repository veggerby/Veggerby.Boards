using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Board : Artifact
    {
        private readonly IEnumerable<Tile> _tiles;
        private readonly IEnumerable<TileRelation> _tileRelations;

        public Board(string id, IEnumerable<TileRelation> tileRelations) : base(id)
        {
            _tileRelations = (tileRelations ?? Enumerable.Empty<TileRelation>()).ToList();
            _tiles = tileRelations.SelectMany(x => new [] { x.From, x.To }).Distinct().ToList();
        }

        public Tile GetTile(string tileId)
        {
            if (string.IsNullOrEmpty(tileId))
            {
                throw new ArgumentException(nameof(tileId));
            }

            return _tiles.SingleOrDefault(x => string.Equals(x.Id, tileId)); 
        }

        public TileRelation GetTileRelation(Tile from, Direction direction)
        {
            if (from == null)
            {
                throw new ArgumentException(nameof(from));
            }

            if (direction == null)
            {
                throw new ArgumentException(nameof(direction));
            }

            return _tileRelations.SingleOrDefault(x => x.From.Equals(from) && x.Direction.Equals(direction));
        }
    }
}