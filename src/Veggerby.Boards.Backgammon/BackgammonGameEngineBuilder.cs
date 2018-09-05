using Veggerby.Boards.Core;

namespace Veggerby.Boards.Backgammon
{
    public class BackgammonGameEngineBuilder : GameEngineBuilder
    {
        protected override void Build()
        {
            // Game
            BoardId = "backgammon";

            AddPlayerDefinition("white");
            AddPlayerDefinition("black");

            for (int i = 1; i <= 24; i++)
            {
                AddTileDefinition($"point-{i}");

                if (i > 1)
                {
                    // black movement direction
                    AddTileRelationDefinition($"point-{i}", $"point-{i - 1}", "counterclockwise");
                }

                if (i < 24)
                {
                    // white movement direction
                    AddTileRelationDefinition($"point-{i}", $"point-{i + 1}", "clockwise");
                }
            }

            AddTileDefinition("bar");
            AddTileDefinition("home-white");
            AddTileDefinition("home-black");

            AddTileRelationDefinition("point-24", "home-white", "clockwise"); // white move home
            AddTileRelationDefinition("point-1", "home-black", "counterclockwise"); // black move home

            AddTileRelationDefinition("bar", "point-1", "clockwise"); // white off the bar
            AddTileRelationDefinition("bar", "point-24", "counterclockwise"); // black off the bar

            AddDirectionDefinition("clockwise");
            AddDirectionDefinition("counterclockwise");

            for (int i = 1; i <= 15; i++)
            {
                AddPieceDefinition($"white-{i}", "white");
                AddPieceDefinition($"black-{i}", "black");

                AddPieceDirectionPatternDefinition($"white-{i}", true, "clockwise");
                AddPieceDirectionPatternDefinition($"black-{i}", true, "counterclockwise");
            }

            AddDiceDefinition("dice-1");
            AddDiceDefinition("dice-2");
            AddDiceDefinition("doubling-dice");

            // State
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
