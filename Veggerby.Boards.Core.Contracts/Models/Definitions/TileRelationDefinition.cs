namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class TileRelationDefinition
    {
        private readonly TileDefinition _sourceTile;
        private readonly TileDefinition _destinationTile;
        private readonly DirectionDefinition _direction;

        public TileRelationDefinition(TileDefinition sourceTile, TileDefinition destinationTile, DirectionDefinition direction)
        {
            _sourceTile = sourceTile;
            _destinationTile = destinationTile;
            _direction = direction;
        }

        public TileDefinition SourceTile => _sourceTile;

        public TileDefinition DestinationTile => _destinationTile;

        public DirectionDefinition Direction => _direction;
    }
}