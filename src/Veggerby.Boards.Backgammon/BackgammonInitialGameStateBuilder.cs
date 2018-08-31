using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon
{
    public class BackgammonInitialGameStateBuilder : InitialGameStateBuilder
    {
        protected override void Build(Game game)
        {
            AddPieceOnTile("white-1", "point-1");
            AddPieceOnTile("white-2", "point-1");

            AddPieceOnTile("white-3", "point-12");
            AddPieceOnTile("white-4", "point-12");
            AddPieceOnTile("white-5", "point-12");
            AddPieceOnTile("white-6", "point-12");
            AddPieceOnTile("white-7", "point-12");

            AddPieceOnTile("white-8", "point-17");
            AddPieceOnTile("white-9", "point-17");
            AddPieceOnTile("white-10", "point-17");

            AddPieceOnTile("white-11", "point-19");
            AddPieceOnTile("white-12", "point-19");
            AddPieceOnTile("white-13", "point-19");
            AddPieceOnTile("white-14", "point-19");
            AddPieceOnTile("white-15", "point-19");

            AddPieceOnTile("black-1", "point-24");
            AddPieceOnTile("black-2", "point-24");

            AddPieceOnTile("black-3", "point-13");
            AddPieceOnTile("black-4", "point-13");
            AddPieceOnTile("black-5", "point-13");
            AddPieceOnTile("black-6", "point-13");
            AddPieceOnTile("black-7", "point-13");

            AddPieceOnTile("black-8", "point-8");
            AddPieceOnTile("black-9", "point-8");
            AddPieceOnTile("black-10", "point-8");

            AddPieceOnTile("black-11", "point-6");
            AddPieceOnTile("black-12", "point-6");
            AddPieceOnTile("black-13", "point-6");
            AddPieceOnTile("black-14", "point-6");
            AddPieceOnTile("black-15", "point-6");

            AddNullDice("dice-1");
            AddNullDice("dice-2");
        }
    }
}
