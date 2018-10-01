namespace Veggerby.Boards.Api.Models
{
    public class BoardModel
    {
        public string Id { get; set; }
        public TileModel[] Tiles { get; set; }
    }
}