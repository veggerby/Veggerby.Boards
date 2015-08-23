using Veggerby.Boards.Core.Contracts.Models.Definitions.Builder;

namespace Veggerby.Boards.Tests.Core.Models.Definitions.Builder
{
    public class SimpleBoardDefinitionBuilder : BoardDefinitionBuilder
    {
        public override void Build()
        {
            BoardId = "board";

            AddTileDefinition("tile1");
            AddTileDefinition("tile2");
            AddTileDefinition("tile3");

            AddDirectionDefinition("clockWise");
            AddDirectionDefinition("counterClockWise");

            AddTileRelationDefinition("tile1", "tile2", "clockWise");
            AddTileRelationDefinition("tile2", "tile3", "clockWise");
            AddTileRelationDefinition("tile3", "tile2", "counterClockWise");
            AddTileRelationDefinition("tile2", "tile1", "counterClockWise");
        }
    }
}
