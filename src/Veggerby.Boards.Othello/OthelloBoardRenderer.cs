using System;
using System.IO;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Othello;

/// <summary>
/// Renders an Othello board to a text stream.
/// </summary>
public static class OthelloBoardRenderer
{
    /// <summary>
    /// Writes the current Othello board state to the specified text writer.
    /// </summary>
    /// <param name="game">The game instance.</param>
    /// <param name="state">The current game state.</param>
    /// <param name="writer">The text writer to output to.</param>
    public static void Write(Game game, GameState state, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(writer);

        var columns = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };

        // Header
        writer.WriteLine("   a b c d e f g h");
        writer.WriteLine("  ┌─────────────────┐");

        // Rows 8 to 1 (top to bottom)
        for (int row = 8; row >= 1; row--)
        {
            writer.Write($"{row} │ ");

            for (int colIndex = 0; colIndex < 8; colIndex++)
            {
                var col = columns[colIndex];
                var tileId = $"{col}{row}";
                var tile = game.GetTile(tileId);

                if (tile == null)
                {
                    writer.Write("?");
                    continue;
                }

                var piecesOnTile = state.GetPiecesOnTile(tile).ToList();

                if (piecesOnTile.Any())
                {
                    var piece = piecesOnTile.First();
                    var currentColor = OthelloHelper.GetCurrentDiscColor(piece, state);

                    writer.Write(currentColor == OthelloDiscColor.Black ? "●" : "○");
                }
                else
                {
                    writer.Write("·");
                }

                if (colIndex < 7)
                {
                    writer.Write(" ");
                }
            }

            writer.WriteLine($" │ {row}");
        }

        // Footer
        writer.WriteLine("  └─────────────────┘");
        writer.WriteLine("   a b c d e f g h");

        // Disc count
        var blackCount = 0;
        var whiteCount = 0;

        foreach (var pieceState in state.GetStates<PieceState>())
        {
            var currentColor = OthelloHelper.GetCurrentDiscColor(pieceState.Artifact, state);
            if (currentColor == OthelloDiscColor.Black)
            {
                blackCount++;
            }
            else
            {
                whiteCount++;
            }
        }

        writer.WriteLine();
        writer.WriteLine($"Black ●: {blackCount}  White ○: {whiteCount}");
    }
}
