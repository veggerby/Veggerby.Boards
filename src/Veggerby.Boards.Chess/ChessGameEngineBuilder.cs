using System.Text;
using Veggerby.Boards.Core;

namespace Veggerby.Boards.Chess
{
    public class ChessGameEngineBuilder : GameEngineBuilder
    {
        private string GetChar(int i)
        {
            var b = Encoding.UTF8.GetBytes(new [] { 'a' })[0] + i - 1;
            return Encoding.UTF8.GetString(new [] { (byte)b });
        }

        protected override void Build()
        {
            // Game
            BoardId = "chess";

            AddPlayerDefinition("white");
            AddPlayerDefinition("black");

            /*         N
             * (a,1) ----- (h,1)    WHITE
             *   |           |
             *   |           |
             * W |           | E
             *   |           |
             *   |           |
             * (a,8) ----- (h,8)    BLACK
             *         S
             */
            for (int x = 1; x <= 8; x++)
            {
                for (int y = 1; y <= 8; y++)
                {
                    AddTileDefinition($"tile-{GetChar(x)}{y}");

                    if (x > 1)
                    {
                        AddTileRelationDefinition($"tile-{GetChar(x)}{y}", $"tile-{GetChar(x - 1)}{y}", "west");
                    }

                    if (x < 8)
                    {
                        AddTileRelationDefinition($"tile-{GetChar(x)}{y}", $"tile-{GetChar(x + 1)}{y}", "east");
                    }

                    if (y > 1)
                    {
                        AddTileRelationDefinition($"tile-{GetChar(x)}{y}", $"tile-{GetChar(x)}{y - 1}", "north");
                    }

                    if (y < 8)
                    {
                        AddTileRelationDefinition($"tile-{GetChar(x)}{y}", $"tile-{GetChar(x)}{y + 1}", "south");
                    }

                    if (x > 1 && y > 1)
                    {
                        AddTileRelationDefinition($"tile-{GetChar(x)}{y}", $"tile-{GetChar(x - 1)}{y - 1}", "north-west");
                    }

                    if (x < 8 && y < 8)
                    {
                        AddTileRelationDefinition($"tile-{GetChar(x)}{y}", $"tile-{GetChar(x + 1)}{y + 1}", "south-east");
                    }

                    if (x > 1 && y < 8)
                    {
                        AddTileRelationDefinition($"tile-{GetChar(x)}{y}", $"tile-{GetChar(x - 1)}{y + 1}", "south-west");
                    }

                    if (x < 8 && y > 1)
                    {
                        AddTileRelationDefinition($"tile-{GetChar(x)}{y}", $"tile-{GetChar(x + 1)}{y - 1}", "north-east");
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

            AddPieceDefinition("white-queen", "white");
            AddPieceDefinition("white-king", "white");

            AddPieceDefinition("black-queen", "black");
            AddPieceDefinition("black-king", "black");

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
                AddPieceDefinition($"white-pawn-{i}", "white");
                AddPieceDefinition($"black-pawn-{i}", "black");

                AddPieceDirectionPatternDefinition($"white-pawn-{i}", false, "south");
                AddPieceDirectionPatternDefinition($"black-pawn-{i}", false, "north");
            }

            for (int i = 1; i <= 8; i++)
            {
                AddPieceDefinition($"white-rook-{i}", "white");
                AddPieceDefinition($"white-knight-{i}", "white");
                AddPieceDefinition($"white-bishop-{i}", "white");

                AddPieceDefinition($"black-rook-{i}", "black");
                AddPieceDefinition($"black-knight-{i}", "black");
                AddPieceDefinition($"black-bishop-{i}", "black");

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

            // State
            AddPieceOnTile("white-rook-1", "tile-a1");
            AddPieceOnTile("white-knight-1", "tile-b1");
            AddPieceOnTile("white-bishop-1", "tile-c1");
            AddPieceOnTile("white-king", "tile-d1");
            AddPieceOnTile("white-queen", "tile-e1");
            AddPieceOnTile("white-bishop-2", "tile-f1");
            AddPieceOnTile("white-knight-2", "tile-g1");
            AddPieceOnTile("white-rook-2", "tile-h1");

            AddPieceOnTile("white-pawn-1", "tile-a2");
            AddPieceOnTile("white-pawn-2", "tile-b2");
            AddPieceOnTile("white-pawn-3", "tile-c2");
            AddPieceOnTile("white-pawn-4", "tile-d2");
            AddPieceOnTile("white-pawn-5", "tile-e2");
            AddPieceOnTile("white-pawn-6", "tile-f2");
            AddPieceOnTile("white-pawn-7", "tile-g2");
            AddPieceOnTile("white-pawn-8", "tile-h2");

            AddPieceOnTile("black-pawn-1", "tile-a7");
            AddPieceOnTile("black-pawn-2", "tile-b7");
            AddPieceOnTile("black-pawn-3", "tile-c7");
            AddPieceOnTile("black-pawn-4", "tile-d7");
            AddPieceOnTile("black-pawn-5", "tile-e7");
            AddPieceOnTile("black-pawn-6", "tile-f7");
            AddPieceOnTile("black-pawn-7", "tile-g7");
            AddPieceOnTile("black-pawn-8", "tile-h7");

            AddPieceOnTile("black-rook-1", "tile-a8");
            AddPieceOnTile("black-knight-1", "tile-b8");
            AddPieceOnTile("black-bishop-1", "tile-c8");
            AddPieceOnTile("black-king", "tile-d8");
            AddPieceOnTile("black-queen", "tile-e8");
            AddPieceOnTile("black-bishop-2", "tile-f8");
            AddPieceOnTile("black-knight-2", "tile-g8");
            AddPieceOnTile("black-rook-2", "tile-h8");
        }
    }
}