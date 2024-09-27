namespace Veggerby.Boards.Api.Models;

public class GameModel
{
    public BoardModel Board { get; set; } = new BoardModel();

    public DiceModel[] Dice { get; set; } = [];
}