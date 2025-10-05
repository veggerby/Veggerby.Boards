namespace Veggerby.Boards.Chess;

/// <summary>
/// Helpers for color lookup.
/// </summary>
public static class ChessPieceColors
{
    /// <summary>Attempts to resolve color for a piece id.</summary>
    public static bool TryGetColor(ChessPieceColorsExtras extras, string pieceId, out ChessPieceColor color)
    {
        color = default;
        if (extras?.Colors is null)
        {
            return false;
        }

        return extras.Colors.TryGetValue(pieceId, out color);
    }
}