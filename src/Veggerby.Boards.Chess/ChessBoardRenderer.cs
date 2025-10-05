using System;
using System.IO;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Renders a chess board (current piece placement) to a <see cref="TextWriter"/> in a simple ASCII form.
/// </summary>
/// <remarks>
/// Board coordinates follow algebraic orientation: rank 8 at the top to rank 1 at the bottom; files a..h left to right.
/// Pieces are rendered using standard Unicode chess glyphs:
/// White: ♔♕♖♗♘♙  Black: ♚♛♜♝♞♟.
/// Empty squares are rendered as '.'; output remains deterministic. If a consuming environment cannot
/// display Unicode, replace glyph mapping with ASCII letters (KQRBNP/kqrbnp).
/// </remarks>
public static class ChessBoardRenderer
{
    /// <summary>
    /// Writes the ASCII board to the provided <paramref name="writer"/>.
    /// </summary>
    /// <param name="game">Compiled chess game (artifact definitions).</param>
    /// <param name="state">Current game state (piece placements).</param>
    /// <param name="writer">Destination text writer.</param>
    public static void Write(Game game, GameState state, TextWriter writer)
    {
        // Environment override: set BOARDS_CHESS_ASCII=1 to force ASCII (helps terminals with double-width glyph rendering).
        var asciiEnv = Environment.GetEnvironmentVariable("BOARDS_CHESS_ASCII");
        var useUnicode = !string.Equals(asciiEnv, "1", StringComparison.Ordinal);
        Write(game, state, writer, useUnicode);
    }

    /// <summary>
    /// Writes the board using either Unicode chess glyphs or ASCII letters.
    /// </summary>
    /// <param name="game">Compiled chess game (artifact definitions).</param>
    /// <param name="state">Current game state (piece placements).</param>
    /// <param name="writer">Destination text writer.</param>
    /// <param name="useUnicode">If true uses ♔♕♖♗♘♙ / ♚♛♜♝♞♟; otherwise uses KQRBNP/kqrbnp.</param>
    public static void Write(Game game, GameState state, TextWriter writer, bool useUnicode)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(writer);

        // Pre-index piece states by tile id for O(1) lookup
        var pieceStates = state.GetStates<PieceState>()
            .ToDictionary(ps => ps.CurrentTile.Id, ps => ps.Artifact);

        writer.WriteLine("  +------------------------+");
        for (int rank = 8; rank >= 1; rank--)
        {
            writer.Write(rank);
            writer.Write(' ');
            writer.Write('|');

            for (int file = 1; file <= 8; file++)
            {
                var fileChar = (char)('a' + file - 1);
                var tileId = $"tile-{fileChar}{rank}";

                if (!pieceStates.TryGetValue(tileId, out var piece))
                {
                    writer.Write(" . ");
                    continue;
                }

                var parts = piece.Id.Split('-'); // e.g. white-pawn-5 or white-king
                var role = parts.Length > 1 ? parts[1] : string.Empty;
                var isWhite = parts[0] == ChessIds.Players.White;
                char glyph;

                if (useUnicode)
                {
                    glyph = role switch
                    {
                        "king" => isWhite ? '♔' : '♚',
                        "queen" => isWhite ? '♕' : '♛',
                        "rook" => isWhite ? '♖' : '♜',
                        "bishop" => isWhite ? '♗' : '♝',
                        "knight" => isWhite ? '♘' : '♞',
                        "pawn" => isWhite ? '♙' : '♟',
                        _ => '?'
                    };
                }
                else
                {
                    glyph = role switch
                    {
                        "king" => 'k',
                        "queen" => 'q',
                        "rook" => 'r',
                        "bishop" => 'b',
                        "knight" => 'n',
                        "pawn" => 'p',
                        _ => '?'
                    };

                    if (isWhite)
                    {
                        glyph = char.ToUpperInvariant(glyph);
                    }
                }

                writer.Write(' ');
                writer.Write(glyph);
                writer.Write(' ');
            }

            writer.Write('|');
            writer.WriteLine();
        }

        writer.WriteLine("  +------------------------+");
        writer.WriteLine("    a  b  c  d  e  f  g  h\n");
    }
}