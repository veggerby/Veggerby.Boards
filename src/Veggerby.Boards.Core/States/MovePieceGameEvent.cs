namespace Veggerby.Boards.Core.States
{
    public class MovePieceGameEvent : IGameEvent
    {
        public Piece Piece { get; }
        public Tile From { get; }
        public Tile To { get; }

        public MovePieceGameEvent(Piece piece, Tile from, Tile to)
        {
            Piece = piece;
            From = from;
            To = to;
        }
    }
}