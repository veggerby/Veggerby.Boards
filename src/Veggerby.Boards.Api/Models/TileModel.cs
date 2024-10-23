namespace Veggerby.Boards.Api.Models;

public class TileModel
{
    public string TileId { get; set; } = string.Empty;
    public PieceModel[] Pieces { get; set; } = [];
}