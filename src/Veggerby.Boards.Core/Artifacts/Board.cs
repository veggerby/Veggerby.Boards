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
    }
}