using System;
using System.IO;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Checkers.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Checkers;

/// <summary>
/// Renders a checkers board (current piece placement) to a <see cref="TextWriter"/> in ASCII form.
/// </summary>
/// <remarks>
/// Board displays 8x8 grid with dark squares only (32 playable squares numbered 1-32).
/// Pieces are rendered as: b/B (black piece/king), w/W (white piece/king).
/// Light squares shown as spaces, dark squares as dots when empty.
/// </remarks>
public static class CheckersBoardRenderer
{
    /// <summary>
    /// Writes the ASCII board to the provided <paramref name="writer"/>.
    /// </summary>
    /// <param name="game">Compiled checkers game (artifact definitions).</param>
    /// <param name="state">Current game state (piece placements).</param>
    /// <param name="writer">Destination text writer.</param>
    public static void Write(Game game, GameState state, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(writer);

        // Pre-index piece states by tile id for O(1) lookup
        var pieceStates = state.GetStates<PieceState>()
            .ToDictionary(ps => ps.CurrentTile.Id, ps => ps.Artifact);

        // Check for promoted pieces
        var promotedPieces = state.GetStates<PromotedPieceState>()
            .Select(ps => ps.PromotedPiece.Id)
            .ToHashSet();

        writer.WriteLine("     Black (b/B)");
        writer.WriteLine("  +----------------+");

        // Checkers board numbering (standard notation)
        // Row 8: 29 30 31 32 (top, black side in standard orientation but WHITE pieces here)
        // Row 7: 25 26 27 28
        // ...
        // Row 2:  5  6  7  8
        // Row 1:  1  2  3  4 (bottom, WHITE side in standard orientation but BLACK pieces here)

        // We'll render from top (row 8) to bottom (row 1)
        // Tile mapping by row:
        var tilesByRow = new[]
        {
            new[] { 29, 30, 31, 32 }, // Row 8
            new[] { 25, 26, 27, 28 }, // Row 7
            new[] { 21, 22, 23, 24 }, // Row 6
            new[] { 17, 18, 19, 20 }, // Row 5
            new[] { 13, 14, 15, 16 }, // Row 4
            new[] { 9, 10, 11, 12 },  // Row 3
            new[] { 5, 6, 7, 8 },     // Row 2
            new[] { 1, 2, 3, 4 }      // Row 1
        };

        for (int row = 0; row < 8; row++)
        {
            writer.Write($"{8 - row} |");

            var tilesInRow = tilesByRow[row];
            int tileIdx = 0;

            for (int col = 1; col <= 8; col++)
            {
                // Determine if this is a dark square (playable)
                // Dark squares are on odd rows at even columns, and even rows at odd columns
                bool isDarkSquare = ((8 - row) % 2 == 0 && col % 2 == 1) || ((8 - row) % 2 == 1 && col % 2 == 0);

                if (!isDarkSquare)
                {
                    writer.Write("  ");
                    continue;
                }

                var tileId = $"tile-{tilesInRow[tileIdx]}";
                tileIdx++;

                if (!pieceStates.TryGetValue(tileId, out var piece))
                {
                    writer.Write(". ");
                    continue;
                }

                var parts = piece.Id.Split('-'); // e.g. black-piece-5 or white-piece-3
                var isBlack = parts[0] == CheckersIds.Players.Black;
                var isKing = promotedPieces.Contains(piece.Id);

                char glyph;
                if (isBlack)
                {
                    glyph = isKing ? 'B' : 'b';
                }
                else
                {
                    glyph = isKing ? 'W' : 'w';
                }

                writer.Write($"{glyph} ");
            }

            writer.WriteLine("|");
        }

        writer.WriteLine("  +----------------+");
        writer.WriteLine("     White (w/W)");
        writer.WriteLine();
        writer.WriteLine("Legend: b=black piece, B=black king");
        writer.WriteLine("        w=white piece, W=white king");
    }
}
