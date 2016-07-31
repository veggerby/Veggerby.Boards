using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Board : Artifact
    {
        public IEnumerable<Tile> Tiles { get; }
        public IEnumerable<TileRelation> TileRelations { get; }

        public Board(string id, IEnumerable<TileRelation> tileRelations) : base(id)
        {
            TileRelations = (tileRelations ?? Enumerable.Empty<TileRelation>()).ToList().AsReadOnly();
            Tiles = tileRelations.SelectMany(x => new [] { x.From, x.To }).Distinct().ToList().AsReadOnly();
        }

        public Tile GetTile(string tileId)
        {
            if (string.IsNullOrEmpty(tileId))
            {
                throw new ArgumentException(nameof(tileId));
            }

            return Tiles.SingleOrDefault(x => string.Equals(x.Id, tileId)); 
        }

        public TileRelation GetTileRelation(Tile from, Direction direction)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (direction == null)
            {
                throw new ArgumentNullException(nameof(direction));
            }

            return TileRelations.SingleOrDefault(x => x.From.Equals(from) && x.Direction.Equals(direction));
        }

        public TileRelation GetTileRelation(Tile from, Tile to)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            return TileRelations.SingleOrDefault(x => x.From.Equals(from) && x.To.Equals(to));
        }
    }
}