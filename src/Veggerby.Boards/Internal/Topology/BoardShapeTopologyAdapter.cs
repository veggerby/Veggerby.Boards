using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Internal.Topology;

/// <summary>
/// Adapter exposing a minimal topology interface over the internal <see cref="BoardShape"/> layout.
/// </summary>
internal sealed class BoardShapeTopologyAdapter(BoardShape shape) : IBoardTopology
{
    private readonly BoardShape _shape = shape;

    public bool TryGetTileIndex(Tile tile, out short index)
    {
        if (_shape.TryGetTileIndex(tile, out var idx))
        {
            index = (short)idx;
            return true;
        }

        index = -1;
        return false;
    }

    public bool TryGetNeighbor(Tile from, Direction direction, out Tile neighbor)
    {
        return _shape.TryGetNeighbor(from, direction, out neighbor);
    }
}