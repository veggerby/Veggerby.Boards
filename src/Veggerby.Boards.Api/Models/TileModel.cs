namespace Veggerby.Boards.Api.Models;

/// <summary>
/// DTO representing a board tile with resident pieces.
/// </summary>
public class TileModel
{
    /// <summary>
    /// Gets or sets the tile identifier.
    /// </summary>
    public string TileId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the pieces currently on this tile.
    /// </summary>
    public PieceModel[] Pieces { get; set; } = [];
}