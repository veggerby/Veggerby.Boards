namespace Veggerby.Boards.Chess;

/// <summary>
/// Convenience helpers for querying chess piece roles from a <see cref="ChessPieceRolesExtras"/> map.
/// </summary>
public static class ChessPieceRoles
{
    /// <summary>
    /// Attempts to resolve the <see cref="ChessPieceRole"/> for a piece id.
    /// </summary>
    /// <param name="extras">The roles extras container.</param>
    /// <param name="pieceId">The piece identifier.</param>
    /// <param name="role">Resolved role when found.</param>
    /// <returns><c>true</c> if role located; otherwise <c>false</c>.</returns>
    public static bool TryGetRole(ChessPieceRolesExtras extras, string pieceId, out ChessPieceRole role)
    {
        role = default;
        if (extras?.Roles is null)
        {
            return false;
        }

        return extras.Roles.TryGetValue(pieceId, out role);
    }
}