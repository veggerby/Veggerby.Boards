using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veggerby.Boards.Core.Contracts.Models.Definitions.Builder;

namespace Veggerby.Boards.Core.Contracts.Builders
{
    public class BackgammonBoardDefinitionBuilder : BoardDefinitionBuilder
    {
        public override void Build()
        {
            BoardId = "backgammon";

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

            AddPieceDefinition("white");
            AddPieceDefinition("black");

            AddPieceDirectionPatternDefinition("white", true, "clockwise");
            AddPieceDirectionPatternDefinition("black", true, "counterclockwise");
        }
    }
}
