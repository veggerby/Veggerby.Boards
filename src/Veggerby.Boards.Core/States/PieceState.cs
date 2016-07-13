namespace Veggerby.Boards.Core.States
{
    public class PieceState : State<Piece>
    {
        public Tile CurrentTile { get; }

        public PieceState(Piece piece, Tile currentTile) : base(piece)
        {
            CurrentTile = currentTile;
        }
    }
}