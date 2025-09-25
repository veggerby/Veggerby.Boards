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
    private readonly ulong[] _perPieceMasks = FeatureFlags.EnablePerPieceMasks ? InitializePerPieceMasks(snapshot, shape, layout, state) : Array.Empty<ulong>();

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

    /// <summary>
    /// Gets the bit mask for a specific piece identity (single bit set for the occupied tile) when per-piece masks are enabled; otherwise returns 0.
    /// </summary>
    public ulong PieceMask(Piece piece)
    {
        if (!FeatureFlags.EnablePerPieceMasks || _perPieceMasks.Length == 0)
        {
            return 0UL;
        }

        if (!_layout.TryGetPieceIndex(piece, out var idx) || idx < 0 || idx >= _perPieceMasks.Length)
        {
            return 0UL;
        }

        return _perPieceMasks[idx];
    }

    public void BindSnapshot(BitboardSnapshot snapshot)
    {
        if (snapshot is null)
        {
            return;
        }

        _snapshot = snapshot;

        if (FeatureFlags.EnablePerPieceMasks && _perPieceMasks.Length > 0)
        {
            RebuildPerPieceMasks(_perPieceMasks, snapshot, _layout, _shape, _state);
        }
    }

    private static ulong[] InitializePerPieceMasks(BitboardSnapshot snapshot, BoardShape shape, BitboardLayout layout, GameState state)
    {
        var arr = new ulong[layout.PieceCount];
        RebuildPerPieceMasks(arr, snapshot, layout, shape, state);
        return arr;
    }

    private static void RebuildPerPieceMasks(ulong[] target, BitboardSnapshot snapshot, BitboardLayout layout, BoardShape shape, GameState state)
    {
        Array.Clear(target, 0, target.Length);
        // Iterate piece states once and assign by layout lookup (avoids nested loop)
        foreach (var ps in state.GetStates<PieceState>())
        {
            if (ps.CurrentTile is null)
            {
                continue;
            }

            if (!layout.TryGetPieceIndex(ps.Artifact, out var pIdx))
            {
                continue;
            }

            if (!shape.TryGetTileIndex(ps.CurrentTile, out var tileIdx))
            {
                continue;
            }

            target[pIdx] = 1UL << tileIdx;
        }
    }
}