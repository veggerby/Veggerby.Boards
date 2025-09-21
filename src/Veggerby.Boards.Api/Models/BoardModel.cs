namespace Veggerby.Boards.Api.Models;

/// <summary>
/// DTO representing a board projection.
/// </summary>
public class BoardModel
{
    /// <summary>
    /// Gets or sets the board identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the tiles belonging to the board.
    /// </summary>
    public TileModel[] Tiles { get; set; } = [];
}