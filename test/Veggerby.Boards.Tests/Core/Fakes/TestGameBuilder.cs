using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class TestGameEngineBuilder : GameEngineBuilder
    {
        protected override void Build()
        {
            // Game
            BoardId = "test";

            AddDiceDefinition("dice");
            AddDiceDefinition("dice-secondary");

            AddPlayerDefinition("player-1");
            AddPlayerDefinition("player-2");

            AddTileDefinition("tile-1");
            AddTileDefinition("tile-2");
            AddTileDefinition("tile-3");

            AddPieceDefinition("piece-1", "player-1");
            AddPieceDefinition("piece-2", "player-2");
            AddPieceDefinition("piece-n");
            AddPieceDefinition("piece-x");
            AddPieceDefinition("piece-y");

            AddDirectionDefinition("clockwise");
            AddDirectionDefinition("counterclockwise");
            AddDirectionDefinition("up");

            AddTileRelationDefinition("tile-1", "tile-2", "clockwise");
            AddTileRelationDefinition("tile-2", "tile-1", "counterclockwise");
            AddTileRelationDefinition("tile-1", "tile-3", "up");

            AddPieceDirectionPatternDefinition("piece-1", true, "clockwise");
            AddPieceDirectionPatternDefinition("piece-2", false, "counterclockwise");
            AddPieceDirectionPatternDefinition("piece-n", true);
            AddPieceDirectionPatternDefinition("piece-x", true, "clockwise", "counterclockwise");
            AddPieceDirectionPatternDefinition("piece-y", false, "clockwise", "counterclockwise");

            // initial state state
            AddNullDice("dice");
            AddDiceValue("dice-secondary", 4);

            AddPieceOnTile("piece-1", "tile-1");
            AddPieceOnTile("piece-2", "tile-2");
            AddPieceOnTile("piece-n", "tile-1");        }
    }
}