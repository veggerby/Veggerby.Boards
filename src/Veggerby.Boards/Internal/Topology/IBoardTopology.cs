using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Internal.Topology;

/// <summary>
/// Internal abstraction over board geometry exposing stable tile indexing and neighbor traversal without leaking underlying layouts.
/// </summary>
internal interface IBoardTopology
{
    bool TryGetTileIndex(Tile tile, out short index);
    /// <summary>Returns true if a neighbor exists in the given direction; sets <paramref name="neighbor"/> when found.</summary>
    bool TryGetNeighbor(Tile from, Artifacts.Relations.Direction direction, out Tile neighbor);
}