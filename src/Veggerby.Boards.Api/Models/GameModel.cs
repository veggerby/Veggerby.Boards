namespace Veggerby.Boards.Api.Models;

/// <summary>
/// DTO representing a game snapshot for API clients.
/// </summary>
public class GameModel
{
    /// <summary>
    /// Gets or sets the board model.
    /// </summary>
    public BoardModel Board { get; set; } = new BoardModel();

    /// <summary>
    /// Gets or sets dice states.
    /// </summary>
    public DiceModel[] Dice { get; set; } = [];
}