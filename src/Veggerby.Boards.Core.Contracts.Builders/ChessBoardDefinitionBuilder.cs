using Veggerby.Boards.Core.Contracts.Models.Definitions.Builder;

namespace Veggerby.Boards.Core.Contracts.Builders
{
    public class ChessBoardDefinitionBuilder : GameBuilder
    {
        public override void Build()
        {
            BoardId = "chess";

            /*         N
             * (1,1) ----- (8,1)    BLACK
             *   |           |
             *   |           |
             * W |           | E
             *   |           |
             *   |           |
             * (1,8) ----- (8,8)    WHITE
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

            AddPieceDefinition("white-pawn");
            AddPieceDefinition("white-rook");
            AddPieceDefinition("white-knight");
            AddPieceDefinition("white-bishop");
            AddPieceDefinition("white-queen");
            AddPieceDefinition("white-king");

            AddPieceDefinition("black-pawn");
            AddPieceDefinition("black-rook");
            AddPieceDefinition("black-knight");
            AddPieceDefinition("black-bishop");
            AddPieceDefinition("black-queen");
            AddPieceDefinition("black-king");

            AddPieceDirectionPatternDefinition("white-pawn", true, "north");

            AddPieceDirectionPatternDefinition("white-rook", true, "north");
            AddPieceDirectionPatternDefinition("white-rook", true, "east");
            AddPieceDirectionPatternDefinition("white-rook", true, "south");
            AddPieceDirectionPatternDefinition("white-rook", true, "west");

            AddPieceDirectionPatternDefinition("white-knight", false, "west", "north", "north");
            AddPieceDirectionPatternDefinition("white-knight", false, "west", "south", "south");
            AddPieceDirectionPatternDefinition("white-knight", false, "east", "north", "north");
            AddPieceDirectionPatternDefinition("white-knight", false, "east", "south", "south");
            AddPieceDirectionPatternDefinition("white-knight", false, "north", "east", "east");
            AddPieceDirectionPatternDefinition("white-knight", false, "north", "west", "west");
            AddPieceDirectionPatternDefinition("white-knight", false, "south", "east", "east");
            AddPieceDirectionPatternDefinition("white-knight", false, "south", "west", "west");

            AddPieceDirectionPatternDefinition("white-bishop", true, "north-east");
            AddPieceDirectionPatternDefinition("white-bishop", true, "north-west");
            AddPieceDirectionPatternDefinition("white-bishop", true, "south-east");
            AddPieceDirectionPatternDefinition("white-bishop", true, "south-west");

            AddPieceDirectionPatternDefinition("white-queen", true, "north");
            AddPieceDirectionPatternDefinition("white-queen", true, "east");
            AddPieceDirectionPatternDefinition("white-queen", true, "west");
            AddPieceDirectionPatternDefinition("white-queen", true, "south");
            AddPieceDirectionPatternDefinition("white-queen", true, "north-east");
            AddPieceDirectionPatternDefinition("white-queen", true, "north-west");
            AddPieceDirectionPatternDefinition("white-queen", true, "south-east");
            AddPieceDirectionPatternDefinition("white-queen", true, "south-west");

            AddPieceDirectionPatternDefinition("white-king", false, "north");
            AddPieceDirectionPatternDefinition("white-king", false, "east");
            AddPieceDirectionPatternDefinition("white-king", false, "west");
            AddPieceDirectionPatternDefinition("white-king", false, "south");
            AddPieceDirectionPatternDefinition("white-king", false, "north-east");
            AddPieceDirectionPatternDefinition("white-king", false, "north-west");
            AddPieceDirectionPatternDefinition("white-king", false, "south-east");
            AddPieceDirectionPatternDefinition("white-king", false, "south-west");

            AddPieceDirectionPatternDefinition("black-pawn", true, "south");
                                                
            AddPieceDirectionPatternDefinition("black-rook", true, "north");
            AddPieceDirectionPatternDefinition("black-rook", true, "east");
            AddPieceDirectionPatternDefinition("black-rook", true, "south");
            AddPieceDirectionPatternDefinition("black-rook", true, "west");
                                                
            AddPieceDirectionPatternDefinition("black-knight", false, "west", "north", "north");
            AddPieceDirectionPatternDefinition("black-knight", false, "west", "south", "south");
            AddPieceDirectionPatternDefinition("black-knight", false, "east", "north", "north");
            AddPieceDirectionPatternDefinition("black-knight", false, "east", "south", "south");
            AddPieceDirectionPatternDefinition("black-knight", false, "north", "east", "east");
            AddPieceDirectionPatternDefinition("black-knight", false, "north", "west", "west");
            AddPieceDirectionPatternDefinition("black-knight", false, "south", "east", "east");
            AddPieceDirectionPatternDefinition("black-knight", false, "south", "west", "west");
                                                
            AddPieceDirectionPatternDefinition("black-bishop", true, "north-east");
            AddPieceDirectionPatternDefinition("black-bishop", true, "north-west");
            AddPieceDirectionPatternDefinition("black-bishop", true, "south-east");
            AddPieceDirectionPatternDefinition("black-bishop", true, "south-west");
                                                
            AddPieceDirectionPatternDefinition("black-queen", true, "north");
            AddPieceDirectionPatternDefinition("black-queen", true, "east");
            AddPieceDirectionPatternDefinition("black-queen", true, "west");
            AddPieceDirectionPatternDefinition("black-queen", true, "south");
            AddPieceDirectionPatternDefinition("black-queen", true, "north-east");
            AddPieceDirectionPatternDefinition("black-queen", true, "north-west");
            AddPieceDirectionPatternDefinition("black-queen", true, "south-east");
            AddPieceDirectionPatternDefinition("black-queen", true, "south-west");
                                                
            AddPieceDirectionPatternDefinition("black-king", false, "north");
            AddPieceDirectionPatternDefinition("black-king", false, "east");
            AddPieceDirectionPatternDefinition("black-king", false, "west");
            AddPieceDirectionPatternDefinition("black-king", false, "south");
            AddPieceDirectionPatternDefinition("black-king", false, "north-east");
            AddPieceDirectionPatternDefinition("black-king", false, "north-west");
            AddPieceDirectionPatternDefinition("black-king", false, "south-east");
            AddPieceDirectionPatternDefinition("black-king", false, "south-west");
        }
    }
}