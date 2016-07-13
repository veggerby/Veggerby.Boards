/*
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Contracts.Models.Definitions;

namespace Veggerby.Boards.Core.Contracts.Models.Navigation
{
    public class TilePath
    {
        private readonly TileDefinition _sourceTileDefinition;
        private readonly TileDefinition _destinationTileDefinition;
        private readonly IEnumerable<TileRelationDefinition> _relations;

        public TilePath(TileDefinition sourceTileDefinition, TileDefinition destinationTileDefinition, IEnumerable<TileRelationDefinition> relations)
        {
            _sourceTileDefinition = sourceTileDefinition;
            _destinationTileDefinition = destinationTileDefinition;
            _relations = (relations ?? Enumerable.Empty<TileRelationDefinition>()).ToList();

            if (_sourceTileDefinition != SourceTileDefinition || _destinationTileDefinition != DestinationTileDefinition)
            {
                throw new TilePathException("Tile Path it not between soure and destination");
            }
        }

        public int GetDistance()
        {
            return _relations == null || !_relations.Any() ? 0 : _relations.Sum(x => x.Distance);
        }

        public TileDefinition SourceTileDefinition => _relations?.FirstOrDefault()?.SourceTile ?? _sourceTileDefinition;
        public TileDefinition DestinationTileDefinition => _relations?.LastOrDefault()?.DestinationTile ?? _destinationTileDefinition;

        public IEnumerable<TileDefinition> GetTileDefinitions()
        {
            if (_relations != null && _relations.Any())
            {
                var result = new List<TileDefinition>();
                result.Add(_relations.First().SourceTile);
                result.AddRange(_relations.Select(relation => relation.DestinationTile));
                return result;
            }

            return new[] { _sourceTileDefinition }; // if no relations _destinationTileDefinition must be the same
        }

        public bool HasPassedOver(TileDefinition tile)
        {
            return GetTileDefinitions().Contains(tile);
        }

        public TilePath Append(TileRelationDefinition tileRelation)
        {
            return new TilePath(SourceTileDefinition, tileRelation.DestinationTile, (_relations ?? Enumerable.Empty<TileRelationDefinition>()).Concat(new[] { tileRelation }).ToArray());
        }
    }
}
*/