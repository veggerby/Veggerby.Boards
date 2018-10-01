namespace Veggerby.Boards.Api.Models
{
    public class TileModel
    {
        public string TileId { get; set; }
        public PieceModel[] Pieces { get; set; }
    }
}