using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Internal.Attacks;

/// <summary>
/// Provides precomputed sliding attack rays for directional repeatable movement pieces.
/// </summary>
/// <remarks>
/// Implementations must be immutable and thread-safe. Absence of a concrete implementation (service not registered)
/// is a valid state; callers should fall back to compiled pattern / legacy resolution paths. Returned masks are
/// implementation specific bit encodings; higher layers should only interpret them via resolver logic.
/// </remarks>
internal interface IAttackRays
{
    /// <summary>
    /// Attempts to retrieve attack ray bitmasks for the given piece from the specified origin tile.
    /// </summary>
    /// <param name="piece">The sliding piece.</param>
    /// <param name="from">Origin tile.</param>
    /// <param name="rays">Returned collection of ray masks (may be empty) if available.</param>
    /// <returns><c>true</c> if rays are available; <c>false</c> if service cannot provide them (e.g., non-sliding piece).</returns>
    bool TryGetRays(Piece piece, Tile from, out ulong[] rays);
}