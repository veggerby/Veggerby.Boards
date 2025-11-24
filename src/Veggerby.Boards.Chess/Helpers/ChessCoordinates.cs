using System;

namespace Veggerby.Boards.Chess.Helpers;

/// <summary>
/// Utility helpers for parsing and constructing chess tile identifiers (tile-e4 etc.).
/// </summary>
internal static class ChessCoordinates
{
    /// <summary>
    /// Attempts to parse a tile id of format "tile-[file][rank]" (e.g., tile-e4) into file char and rank number.
    /// </summary>
    public static bool TryParse(string tileId, out char file, out int rank)
    {
        file = '\0';
        rank = 0;
        if (string.IsNullOrWhiteSpace(tileId) || !tileId.StartsWith("tile-", StringComparison.Ordinal))
        {
            return false;
        }

        var span = tileId.AsSpan();
        if (span.Length != 7) // tile- + file + rank => length 7 (e.g., tile-e4)
        {
            return false;
        }

        file = span[5];
        if (file < 'a' || file > 'h')
        {
            return false;
        }

        var rankChar = span[6];
        if (rankChar < '1' || rankChar > '8')
        {
            return false;
        }
        rank = rankChar - '0';
        return true;
    }

    /// <summary>
    /// Builds a tile id from file char and rank number.
    /// </summary>
    public static string BuildTileId(char file, int rank)
    {
        return $"tile-{file}{rank}";
    }
}