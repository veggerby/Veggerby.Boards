namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class TileRelationDefinition
    {
        private readonly TileDefinition _sourceTile;
        private readonly TileDefinition _destinationTile;
        private readonly DirectionDefinition _direction;
        private readonly int _distance;

        public TileRelationDefinition(TileDefinition sourceTile, TileDefinition destinationTile, DirectionDefinition direction, int distance = 1)
        {
            _sourceTile = sourceTile;
            _destinationTile = destinationTile;
            _direction = direction;
            _distance = distance;
        }

        public TileDefinition SourceTile => _sourceTile;

        public TileDefinition DestinationTile => _destinationTile;

        public DirectionDefinition Direction => _direction;
        public int Distance => _distance;
    }
}