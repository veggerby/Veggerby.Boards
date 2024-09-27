namespace Veggerby.Boards.Api.Models;

public class BoardModel
{
    public string Id { get; set; } = string.Empty;
    public TileModel[] Tiles { get; set; } = [];
}