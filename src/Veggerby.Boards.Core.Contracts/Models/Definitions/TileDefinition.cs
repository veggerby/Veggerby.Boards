namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class TileDefinition
    {
        public TileDefinition(string tileId)
        { 
            TileId = tileId;
        }

        public string TileId { get; }

        public override string ToString()
        {
            return $"tile {TileId}";
        }
    }
}