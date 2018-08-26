using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Relations;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class TestGameBuilder : GameBuilder
    {
        public override void Build()
        {
            BoardId = "test";

            AddPlayerDefinition("player-1");
            AddPlayerDefinition("player-2");

            AddTileDefinition("tile-1");
            AddTileDefinition("tile-2");

            AddPieceDefinition("piece-1", "player-1");
            AddPieceDefinition("piece-2", "player-2");
            AddPieceDefinition("piece-n");
            AddPieceDefinition("piece-x");
            AddPieceDefinition("piece-y");

            AddDirectionDefinition("clockwise");
            AddDirectionDefinition("counterclockwise");

            AddTileRelationDefinition("tile-1", "tile-2", "clockwise");
            AddTileRelationDefinition("tile-2", "tile-1", "counterclockwise");

            AddPieceDirectionPatternDefinition("piece-1", true, "clockwise");
            AddPieceDirectionPatternDefinition("piece-2", false, "counterclockwise");
            AddPieceDirectionPatternDefinition("piece-n", true);
            AddPieceDirectionPatternDefinition("piece-x", true, "clockwise", "counterclockwise");
            AddPieceDirectionPatternDefinition("piece-y", false, "clockwise", "counterclockwise");
        }
    }
}