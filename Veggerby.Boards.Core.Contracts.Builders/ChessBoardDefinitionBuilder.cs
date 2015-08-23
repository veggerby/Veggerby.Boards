using Veggerby.Boards.Core.Contracts.Models.Definitions.Builder;

namespace Veggerby.Boards.Core.Contracts.Builders
{
    public class ChessBoardDefinitionBuilder : BoardDefinitionBuilder
    {
        public override void Build()
        {
            BoardId = "chess";

            /*         N
             * (1,1) ----- (8,1)
             *   |           |
             *   |           |
             * W |           | E
             *   |           |
             *   |           |
             * (1,8) ----- (8,8)
             *         S
             */
            for (int x = 1; x <= 8; x++)
            {
                for (int y = 1; y <= 8; y++)
                {
                    AddTileDefinition($"tile-{x}-{y}");
                    if (x > 1)
                    {
                        AddTileRelationDefinition($"tile-{x}-{y}", $"tile-{x - 1}-{y}", "west");
                    }

                    if (x < 8)
                    {
                        AddTileRelationDefinition($"tile-{x}-{y}", $"tile-{x + 1}-{y}", "east");
                    }

                    if (y > 1)
                    {
                        AddTileRelationDefinition($"tile-{x}-{y}", $"tile-{x}-{y - 1}", "north");
                    }

                    if (y < 8)
                    {
                        AddTileRelationDefinition($"tile-{x}-{y}", $"tile-{x}-{y + 1}", "south");
                    }
                    if (x > 1 && y > 1)
                    {
                        AddTileRelationDefinition($"tile-{x}-{y}", $"tile-{x - 1}-{y - 1}", "north-west");
                    }

                    if (x < 8 && y < 8)
                    {
                        AddTileRelationDefinition($"tile-{x}-{y}", $"tile-{x + 1}-{y + 1}", "south-east");
                    }

                    if (x > 1 && y < 8)
                    {
                        AddTileRelationDefinition($"tile-{x}-{y}", $"tile-{x - 1}-{y + 1}", "south-west");
                    }

                    if (x < 8 && y > 1)
                    {
                        AddTileRelationDefinition($"tile-{x}-{y}", $"tile-{x + 1}-{y - 1}", "south-east");
                    }

                }
            }

            AddDirectionDefinition("north");
            AddDirectionDefinition("east");
            AddDirectionDefinition("south");
            AddDirectionDefinition("west");
            AddDirectionDefinition("north-east");
            AddDirectionDefinition("north-west");
            AddDirectionDefinition("south-east");
            AddDirectionDefinition("south-west");
        }
    }
}