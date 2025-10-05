using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Occupancy;

/// <summary>
/// Internal capability abstraction for querying tile occupancy and ownership in O(1) or near O(1) time.
/// Implementations must be pure (no side effects) and deterministic given the associated <see cref="GameState"/> snapshot.
/// </summary>
internal interface IOccupancyIndex
{
    /// <summary>Returns true if the tile has no piece occupying it.</summary>
    bool IsEmpty(Tile tile);

    /// <summary>Returns true if the specified tile is currently occupied by a piece owned by <paramref name="player"/>.</summary>
    bool IsOwnedBy(Tile tile, Player player);

    /// <summary>Gets a bit mask of globally occupied tiles (low-bit index ordering equals board tile ordering). Implementations may return 0 when unsupported.</summary>
    ulong GlobalMask { get; }

    /// <summary>Gets a bit mask of tiles occupied by the specified player. Implementations may return 0 when unsupported.</summary>
    ulong PlayerMask(Player player);
}