namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class TileRelationDefinition
    {
        public TileRelationDefinition(TileDefinition sourceTile, TileDefinition destinationTile, DirectionDefinition direction, int distance = 1)
        {
            SourceTile = sourceTile;
            DestinationTile = destinationTile;
            Direction = direction;
            Distance = distance;
        }

        public TileDefinition SourceTile { get; }

        public TileDefinition DestinationTile { get; }

        public DirectionDefinition Direction { get; }
        
        public int Distance { get; }
    }
}