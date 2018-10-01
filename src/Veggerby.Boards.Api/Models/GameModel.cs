namespace Veggerby.Boards.Api.Models
{
    public class GameModel
    {
        public BoardModel Board { get; set; }

        public DiceModel[] Dice { get; set; }
    }
}