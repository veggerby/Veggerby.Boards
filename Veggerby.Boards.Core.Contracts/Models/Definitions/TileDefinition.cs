namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class TileDefinition
    {
        public string TileId { get; set; }
        public TileRelationDefinition[] RelationsDefinition { get; set; }
    }
}