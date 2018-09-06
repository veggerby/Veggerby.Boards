using Veggerby.Boards.Core;
using Veggerby.Boards.Core.States.Conditions;

namespace Veggerby.Boards.Backgammon
{
    public class BackgammonGameEngineBuilder : GameEngineBuilder
    {
        protected override void Build()
        {
            // Game
            BoardId = "backgammon";

            AddPlayer("white");
            AddPlayer("black");

            AddDirection("clockwise");
            AddDirection("counterclockwise");

            for (int i = 1; i <= 24; i++)
            {
                var tile = AddTile($"point-{i}");

                if (i > 1)
                {
                    // black movement direction
                    tile
                        .WithRelationTo($"point-{i - 1}")
                        .InDirection("counterclockwise");
                }

                if (i < 24)
                {
                    // white movement direction
                    tile
                        .WithRelationTo($"point-{i + 1}")
                        .InDirection("clockwise");
                }
            }

            AddTile("bar")
                .WithRelationTo("point-1").InDirection("clockwise").Done() // white off the bar
                .WithRelationTo("point-24").InDirection("counterclockwise"); // black off the bar

            AddTile("home-white")
                .WithRelationFrom("point-24").InDirection("clockwise"); // white move home

            AddTile("home-black")
                .WithRelationFrom("point-1").InDirection("counterclockwise"); // black move home


            for (int i = 1; i <= 15; i++)
            {
                AddPiece($"white-{i}").WithOwner("white").HasDirection("clockwise").CanRepeat();
                AddPiece($"black-{i}").WithOwner("black").HasDirection("counterclockwise").CanRepeat();
            }

            AddDice("dice-1").HasNoValue();
            AddDice("dice-2").HasNoValue();
            AddDice("doubling-dice").HasValue(1);

            // State
            WithPiece("white-1").OnTile("point-1");
            WithPiece("white-2").OnTile("point-1");

            WithPiece("white-3").OnTile("point-12");
            WithPiece("white-4").OnTile("point-12");
            WithPiece("white-5").OnTile("point-12");
            WithPiece("white-6").OnTile("point-12");
            WithPiece("white-7").OnTile("point-12");

            WithPiece("white-8").OnTile("point-17");
            WithPiece("white-9").OnTile("point-17");
            WithPiece("white-10").OnTile("point-17");

            WithPiece("white-11").OnTile("point-19");
            WithPiece("white-12").OnTile("point-19");
            WithPiece("white-13").OnTile("point-19");
            WithPiece("white-14").OnTile("point-19");
            WithPiece("white-15").OnTile("point-19");

            WithPiece("black-1").OnTile("point-24");
            WithPiece("black-2").OnTile("point-24");

            WithPiece("black-3").OnTile("point-13");
            WithPiece("black-4").OnTile("point-13");
            WithPiece("black-5").OnTile("point-13");
            WithPiece("black-6").OnTile("point-13");
            WithPiece("black-7").OnTile("point-13");

            WithPiece("black-8").OnTile("point-8");
            WithPiece("black-9").OnTile("point-8");
            WithPiece("black-10").OnTile("point-8");

            WithPiece("black-11").OnTile("point-6");
            WithPiece("black-12").OnTile("point-6");
            WithPiece("black-13").OnTile("point-6");
            WithPiece("black-14").OnTile("point-6");
            WithPiece("black-15").OnTile("point-6");
        }
    }
}
