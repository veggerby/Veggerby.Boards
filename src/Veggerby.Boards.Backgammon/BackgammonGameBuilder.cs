﻿using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Backgammon
{
    public class BackgammonGameBuilder : GameBuilder
    {
        protected override void Build()
        {
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
        }
    }
}
