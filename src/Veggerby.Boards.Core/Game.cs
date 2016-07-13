namespace Veggerby.Boards.Core
{
    public class Game : Artifact
    {
        public Board Board { get; }

        public Game(string id, Board board) : base(id)
        {
            Board = board;
        }
    }
}