namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class TileRelationDefinition
    {
        public TileDefinition SourceTile { get; set; }
        public TileDefinition DestinationTile { get; set; }
        public DirectionDefinition Direction { get; set; }
    }
}