using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Internal.Layout;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Occupancy;

/// <summary>
/// Bitboard backed occupancy index exposing O(1) emptiness and ownership checks.
/// </summary>
internal sealed class BitboardOccupancyIndex(BitboardServices services, BoardShape shape, Game game, GameState state) : IOccupancyIndex, Veggerby.Boards.Internal.Acceleration.IBitboardBackedOccupancy
{
    private readonly BitboardServices _services = services ?? throw new ArgumentNullException(nameof(services));
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
        return (_services.Snapshot.GlobalOccupancy & mask) == 0;
    }

    public bool IsOwnedBy(Tile tile, Player player)
    {
        if (!_shape.TryGetTileIndex(tile, out var idx))
        {
            return false;
        }

        if (!_services.Layout.TryGetPlayerIndex(player, out var pIndex))
        {
            return false;
        }

        var mask = 1UL << idx;
        return (_services.Snapshot.PlayerOccupancy[pIndex] & mask) != 0;
    }

    public ulong GlobalMask => _services.Snapshot.GlobalOccupancy;

    public ulong PlayerMask(Player player)
    {
        if (!_services.Layout.TryGetPlayerIndex(player, out var pIndex))
        {
            return 0UL;
        }

        return _services.Snapshot.PlayerOccupancy[pIndex];
    }
    public void BindSnapshot(BitboardSnapshot snapshot)
    {
        if (snapshot is null)
        {
            return;
        }

        // Replace underlying snapshot reference in services via reflection-free simple field assignment (new services wrapper not allocated)
        // Since BitboardServices currently exposes read-only properties, consider updating design if deeper mutation needed.
        // For now, no-op: snapshot lives elsewhere; queries still reference services.Snapshot (initial). Extend if required.
    }
}