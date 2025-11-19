using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess.MoveGeneration;

/// <summary>
/// Parses Standard Algebraic Notation (SAN) to identify matching legal chess moves.
/// </summary>
/// <remarks>
/// SAN is the standard notation for recording chess moves (e.g., "Nf3", "exd5", "O-O", "e8=Q#").
/// This parser resolves SAN strings to concrete legal moves by matching against candidate moves
/// from the current position, handling disambiguation, captures, promotions, and special moves.
/// </remarks>
public static class ChessSanParser
{
    /// <summary>
    /// Finds a legal move matching the given SAN notation.
    /// </summary>
    /// <param name="san">Standard Algebraic Notation string (e.g., "Nf3", "exd5", "O-O-O").</param>
    /// <param name="legalMoves">Collection of legal moves for the current position.</param>
    /// <param name="game">The chess game definition.</param>
    /// <param name="state">Current game state.</param>
    /// <returns>The matching legal move, or null if no match found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    /// <remarks>
    /// Handles:
    /// - Basic moves: "e4", "Nf3", "Bc4"
    /// - Captures: "exd5", "Nxf7", "Bxc6"
    /// - Castling: "O-O" (kingside), "O-O-O" (queenside)
    /// - Promotions: "e8=Q", "axb8=N"
    /// - Disambiguation: "Nbd2", "R1e1", "Qh4e1"
    /// - Check/checkmate symbols are stripped ("+", "#")
    /// </remarks>
    public static PseudoMove? ParseSan(string san, IReadOnlyCollection<PseudoMove> legalMoves, Game game, GameState state)
    {
        ArgumentNullException.ThrowIfNull(san);
        ArgumentNullException.ThrowIfNull(legalMoves);
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(state);

        // Strip check/checkmate symbols
        san = san.TrimEnd('+', '#');

        if (string.IsNullOrWhiteSpace(san))
        {
            return null;
        }

        // Handle castling
        if (san == "O-O" || san == "O-O-O")
        {
            return legalMoves.FirstOrDefault(m =>
                m.Kind == PseudoMoveKind.Castle &&
                (san == "O-O" ? m.To.Id.EndsWith("g1") || m.To.Id.EndsWith("g8")
                              : m.To.Id.EndsWith("c1") || m.To.Id.EndsWith("c8")));
        }

        // Parse SAN notation
        bool isCapture = san.Contains('x');
        string cleanSan = san.Replace("x", "");

        // Check for promotion
        string? promotionRole = null;
        if (cleanSan.Contains('=') || (cleanSan.Length > 2 && char.IsUpper(cleanSan[^1])))
        {
            char promoPiece = cleanSan.Contains('=') ? cleanSan[^1] : cleanSan[^1];
            promotionRole = promoPiece switch
            {
                'Q' => "queen",
                'R' => "rook",
                'B' => "bishop",
                'N' => "knight",
                _ => null
            };
            cleanSan = cleanSan.Contains('=') ? cleanSan[..^2] : cleanSan[..^1];
        }

        // Determine piece role
        ChessPieceRole role = ChessPieceRole.Pawn;
        int startIndex = 0;
        if (cleanSan.Length > 0 && char.IsUpper(cleanSan[0]))
        {
            role = cleanSan[0] switch
            {
                'K' => ChessPieceRole.King,
                'Q' => ChessPieceRole.Queen,
                'R' => ChessPieceRole.Rook,
                'B' => ChessPieceRole.Bishop,
                'N' => ChessPieceRole.Knight,
                _ => ChessPieceRole.Pawn
            };
            startIndex = 1;
        }

        // Extract destination square (last 2 characters)
        if (cleanSan.Length < startIndex + 2)
        {
            return null;
        }

        string destSquare = cleanSan[^2..];
        string? disambig = cleanSan.Length > startIndex + 2 ? cleanSan.Substring(startIndex, cleanSan.Length - startIndex - 2) : null;

        // Find matching moves
        var candidates = legalMoves.Where(m =>
            ChessPiece.IsRole(game, m.Piece.Id, role) &&
            m.To.Id == $"tile-{destSquare}" &&
            m.IsCapture == isCapture).ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        if (candidates.Count == 1)
        {
            return candidates[0];
        }

        // Apply disambiguation
        if (disambig != null)
        {
            if (disambig.Length == 1)
            {
                if (char.IsDigit(disambig[0]))
                {
                    // Rank disambiguation (e.g., "R1e1" means rook from rank 1)
                    candidates = candidates.Where(m => m.From.Id.EndsWith(disambig)).ToList();
                }
                else
                {
                    // File disambiguation (e.g., "Nfd4" means knight from f-file)
                    // Tile IDs are "tile-{file}{rank}", so extract the file character
                    candidates = candidates.Where(m =>
                    {
                        var fromId = m.From.Id;
                        if (fromId.StartsWith("tile-") && fromId.Length >= 7)
                        {
                            return fromId[5] == disambig[0]; // Character at position 5 is the file
                        }
                        return false;
                    }).ToList();
                }
            }
            else
            {
                // Full square disambiguation (e.g., "Qh4e1")
                candidates = candidates.Where(m => m.From.Id == $"tile-{disambig}").ToList();
            }
        }

        return candidates.FirstOrDefault();
    }
}
