using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Chess
{
    public class ChessInitialGameStateBuilder : InitialGameStateBuilder
    {
        protected override void Build(Game game)
        {
            AddPieceOnTile("white-rook-1", "tile-a1");
            AddPieceOnTile("white-knight-1", "tile-b1");
            AddPieceOnTile("white-bishop-1", "tile-c1");
            AddPieceOnTile("white-king", "tile-d1");
            AddPieceOnTile("white-queen", "tile-e1");
            AddPieceOnTile("white-bishop-2", "tile-f1");
            AddPieceOnTile("white-knight-2", "tile-g1");
            AddPieceOnTile("white-rook-2", "tile-h1");

            AddPieceOnTile("white-pawn-1", "tile-a2");
            AddPieceOnTile("white-pawn-2", "tile-b2");
            AddPieceOnTile("white-pawn-3", "tile-c2");
            AddPieceOnTile("white-pawn-4", "tile-d2");
            AddPieceOnTile("white-pawn-5", "tile-e2");
            AddPieceOnTile("white-pawn-6", "tile-f2");
            AddPieceOnTile("white-pawn-7", "tile-g2");
            AddPieceOnTile("white-pawn-8", "tile-h2");

            AddPieceOnTile("black-pawn-1", "tile-a7");
            AddPieceOnTile("black-pawn-2", "tile-b7");
            AddPieceOnTile("black-pawn-3", "tile-c7");
            AddPieceOnTile("black-pawn-4", "tile-d7");
            AddPieceOnTile("black-pawn-5", "tile-e7");
            AddPieceOnTile("black-pawn-6", "tile-f7");
            AddPieceOnTile("black-pawn-7", "tile-g7");
            AddPieceOnTile("black-pawn-8", "tile-h7");

            AddPieceOnTile("black-rook-1", "tile-a8");
            AddPieceOnTile("black-knight-1", "tile-b8");
            AddPieceOnTile("black-bishop-1", "tile-c8");
            AddPieceOnTile("black-king", "tile-d8");
            AddPieceOnTile("black-queen", "tile-e8");
            AddPieceOnTile("black-bishop-2", "tile-f8");
            AddPieceOnTile("black-knight-2", "tile-g8");
            AddPieceOnTile("black-rook-2", "tile-h8");
        }
    }
}
