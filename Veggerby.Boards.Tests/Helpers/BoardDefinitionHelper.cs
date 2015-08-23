using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veggerby.Boards.Core.Contracts.Models.Definitions;

namespace Veggerby.Boards.Tests.Helpers
{
    public static class BoardDefinitionHelper
    {
        public static BoardDefinition GetBoardDefinition()
        {
            var tile1 = new TileDefinition { TileId = "tile1" };
            var tile2 = new TileDefinition { TileId = "tile2" };
            var tile3 = new TileDefinition { TileId = "tile3" };

            var clockWise = new DirectionDefinition { DirectionId = "clockWise" };
            var counterClockWise = new DirectionDefinition { DirectionId = "counterClockWise" };

            var relationTile1ToTile2 = new TileRelationDefinition
            {
                SourceTile = tile1,
                DestinationTile = tile2,
                Direction = clockWise
            };

            var relationTile2ToTile3 = new TileRelationDefinition
            {
                SourceTile = tile2,
                DestinationTile = tile3,
                Direction = clockWise
            };

            var relationTile3ToTile2 = new TileRelationDefinition
            {
                SourceTile = tile3,
                DestinationTile = tile2,
                Direction = counterClockWise
            };

            var relationTile2ToTile1 = new TileRelationDefinition
            {
                SourceTile = tile2,
                DestinationTile = tile1,
                Direction = counterClockWise
            };

            tile1.RelationsDefinition = new[] { relationTile1ToTile2 };
            tile2.RelationsDefinition = new[] { relationTile2ToTile3, relationTile2ToTile1 };
            tile3.RelationsDefinition = new[] { relationTile3ToTile2 };

            var boardDefinition = new BoardDefinition
            {
                BoardId = "board",
                Tiles = new[] { tile1, tile2, tile3 }
            };

            return boardDefinition;
        }

    }
}
