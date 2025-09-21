namespace Veggerby.Boards.Api.Models;

/// <summary>
/// DTO representing a piece located on a tile.
/// </summary>
public class PieceModel
{
    /// <summary>
    /// Gets or sets the piece identifier.
    /// </summary>
    public string PieceId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the owning player identifier.
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;
}