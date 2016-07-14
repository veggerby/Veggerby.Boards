using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Board : CompositeArtifact<Tile>
    {
        private readonly IEnumerable<TileRelation> _tileRelations;

        public Board(string id, IEnumerable<Tile> tiles, IEnumerable<TileRelation> tileRelations) : base(id, tiles)
        {
            _tileRelations = (tileRelations ?? Enumerable.Empty<TileRelation>()).ToList();
        }

        public Tile GetNextTile(Tile from, Direction direction)
        {
            var relation = _tileRelations.SingleOrDefault(x => x.From.Equals(from) && x.Direction.Equals(direction));
            return relation?.To;
        }
    }
}