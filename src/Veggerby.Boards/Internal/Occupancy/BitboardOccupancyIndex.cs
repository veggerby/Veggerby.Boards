using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Internal.Layout;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Occupancy;

/// <summary>
/// Bitboard backed occupancy index exposing O(1) emptiness and ownership checks.
/// </summary>
internal sealed class BitboardOccupancyIndex(BitboardLayout layout, BitboardSnapshot snapshot, BoardShape shape, Game game, GameState state) : IOccupancyIndex, Veggerby.Boards.Internal.Acceleration.IBitboardBackedOccupancy
{
    private readonly BitboardLayout _layout = layout ?? throw new ArgumentNullException(nameof(layout));
    private BitboardSnapshot _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    private readonly BoardShape _shape = shape ?? throw new ArgumentNullException(nameof(shape));
    private readonly Game _game = game ?? throw new ArgumentNullException(nameof(game));
    private readonly GameState _state = state ?? throw new ArgumentNullException(nameof(state)); // retained to map tile → piece owner when needed (for robustness / future extensions)

    public bool IsEmpty(Tile tile)
    {
        if (!_shape.TryGetTileIndex(tile, out var idx))
        {
            return true; // unknown tile treated as empty
        }

        var mask = 1UL << idx;
        return (_snapshot.GlobalOccupancy & mask) == 0;
    }

    public bool IsOwnedBy(Tile tile, Player player)
    {
        if (!_shape.TryGetTileIndex(tile, out var idx))
        {
            return false;
        }

        if (!_layout.TryGetPlayerIndex(player, out var pIndex))
        {
            return false;
        }

        var mask = 1UL << idx;
        return (_snapshot.PlayerOccupancy[pIndex] & mask) != 0;
    }

    public ulong GlobalMask => _snapshot.GlobalOccupancy;

    public ulong PlayerMask(Player player)
    {
        if (!_layout.TryGetPlayerIndex(player, out var pIndex))
        {
            return 0UL;
        }

        return _snapshot.PlayerOccupancy[pIndex];
    }
    public void BindSnapshot(BitboardSnapshot snapshot)
    {
        if (snapshot is null)
        {
            return;
        }
        _snapshot = snapshot;
    }
}