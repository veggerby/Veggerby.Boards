using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Occupancy;

/// <summary>
/// Simple occupancy index that queries the underlying <see cref="GameState"/> piece states each call.
/// Used when bitboard acceleration is disabled or unsupported (>64 tiles).
/// </summary>
internal sealed class NaiveOccupancyIndex(Game game, GameState state) : IOccupancyIndex, INaiveMutableOccupancy
{
    private readonly Game _game = game ?? throw new ArgumentNullException(nameof(game));
    private GameState _state = state ?? throw new ArgumentNullException(nameof(state));

    public bool IsEmpty(Tile tile)
    {
        return !_state.GetStates<PieceState>().Any(ps => ps.CurrentTile == tile);
    }

    public bool IsOwnedBy(Tile tile, Player player)
    {
        return _state.GetStates<PieceState>().Any(ps => ps.CurrentTile == tile && ps.Artifact.Owner == player);
    }

    public ulong GlobalMask => 0UL; // Not supported (would require tile indexing mapping; callers treat 0 as absent)

    public ulong PlayerMask(Player player) => 0UL;

    public void BindState(GameState state)
    {
        if (state is not null)
        {
            _state = state;
        }
    }
}

internal interface INaiveMutableOccupancy : IOccupancyIndex
{
    void BindState(GameState state);
}