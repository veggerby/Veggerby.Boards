namespace Veggerby.Boards.Core.Builders
{
    public class ChessGameBuilder : GameBuilder
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

            AddPieceDefinition("white-queen");
            AddPieceDefinition("white-king");

            AddPieceDefinition("black-queen");
            AddPieceDefinition("black-king");


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
            
            for (int i = 1; i <= 8; i++)
            {
                AddPieceDefinition($"white-pawn-{i}");
                AddPieceDefinition($"black-pawn-{i}");
                
                AddPieceDirectionPatternDefinition($"white-pawn-{i}", true, "north");
                AddPieceDirectionPatternDefinition($"black-pawn-{i}", true, "south");
            }

            for (int i = 1; i <= 8; i++)
            {
                AddPieceDefinition($"white-rook-{i}");
                AddPieceDefinition($"white-knight-{i}");
                AddPieceDefinition($"white-bishop-{i}");

                AddPieceDefinition($"black-rook-{i}");
                AddPieceDefinition($"black-knight-{i}");
                AddPieceDefinition($"black-bishop-{i}");

                AddPieceDirectionPatternDefinition($"white-rook-{i}", true, "north");
                AddPieceDirectionPatternDefinition($"white-rook-{i}", true, "east");
                AddPieceDirectionPatternDefinition($"white-rook-{i}", true, "south");
                AddPieceDirectionPatternDefinition($"white-rook-{i}", true, "west");

                AddPieceDirectionPatternDefinition($"white-knight-{i}", false, "west", "north", "north");
                AddPieceDirectionPatternDefinition($"white-knight-{i}", false, "west", "south", "south");
                AddPieceDirectionPatternDefinition($"white-knight-{i}", false, "east", "north", "north");
                AddPieceDirectionPatternDefinition($"white-knight-{i}", false, "east", "south", "south");
                AddPieceDirectionPatternDefinition($"white-knight-{i}", false, "north", "east", "east");
                AddPieceDirectionPatternDefinition($"white-knight-{i}", false, "north", "west", "west");
                AddPieceDirectionPatternDefinition($"white-knight-{i}", false, "south", "east", "east");
                AddPieceDirectionPatternDefinition($"white-knight-{i}", false, "south", "west", "west");

                AddPieceDirectionPatternDefinition($"white-bishop-{i}", true, "north-east");
                AddPieceDirectionPatternDefinition($"white-bishop-{i}", true, "north-west");
                AddPieceDirectionPatternDefinition($"white-bishop-{i}", true, "south-east");
                AddPieceDirectionPatternDefinition($"white-bishop-{i}", true, "south-west");

                AddPieceDirectionPatternDefinition($"black-rook-{i}", true, "north");
                AddPieceDirectionPatternDefinition($"black-rook-{i}", true, "east");
                AddPieceDirectionPatternDefinition($"black-rook-{i}", true, "south");
                AddPieceDirectionPatternDefinition($"black-rook-{i}", true, "west");

                AddPieceDirectionPatternDefinition($"black-knight-{i}", false, "west", "north", "north");
                AddPieceDirectionPatternDefinition($"black-knight-{i}", false, "west", "south", "south");
                AddPieceDirectionPatternDefinition($"black-knight-{i}", false, "east", "north", "north");
                AddPieceDirectionPatternDefinition($"black-knight-{i}", false, "east", "south", "south");
                AddPieceDirectionPatternDefinition($"black-knight-{i}", false, "north", "east", "east");
                AddPieceDirectionPatternDefinition($"black-knight-{i}", false, "north", "west", "west");
                AddPieceDirectionPatternDefinition($"black-knight-{i}", false, "south", "east", "east");
                AddPieceDirectionPatternDefinition($"black-knight-{i}", false, "south", "west", "west");

                AddPieceDirectionPatternDefinition($"black-bishop-{i}", true, "north-east");
                AddPieceDirectionPatternDefinition($"black-bishop-{i}", true, "north-west");
                AddPieceDirectionPatternDefinition($"black-bishop-{i}", true, "south-east");
                AddPieceDirectionPatternDefinition($"black-bishop-{i}", true, "south-west");
            }
        }
    }
}