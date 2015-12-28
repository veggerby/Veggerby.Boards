namespace Veggerby.Boards.Core.Contracts.Models.Definitions
{
    public class TileDefinition
    {
        private readonly string _tileId;

        public TileDefinition(string tileId)
        { 
            _tileId = tileId;
        }

        public string TileId => _tileId;

        public override string ToString()
        {
            return $"tile {TileId}";
        }
    }
}