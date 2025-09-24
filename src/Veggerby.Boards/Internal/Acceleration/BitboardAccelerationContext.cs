using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Internal.Attacks;
using Veggerby.Boards.Internal.Layout;
using Veggerby.Boards.Internal.Occupancy;
using Veggerby.Boards.Internal.Topology;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Acceleration;

/// <summary>
/// Acceleration context maintaining internal bitboard + piece map snapshots, updating them incrementally where possible.
/// </summary>
internal sealed class BitboardAccelerationContext : IAccelerationContext
{
    private readonly BitboardLayout _bitboardLayout;
    private BitboardSnapshot _bitboardSnapshot;
    private readonly PieceMapLayout _pieceMapLayout;
    private PieceMapSnapshot _pieceMapSnapshot;
    private readonly BoardShape _shape; // needed for rebuilds
    private readonly IBoardTopology _topology; // optional for future index resolution helpers

    public IOccupancyIndex Occupancy { get; }
    public IAttackRays AttackRays { get; }

    public BitboardAccelerationContext(
        BitboardLayout bitboardLayout,
        BitboardSnapshot bitboardSnapshot,
        PieceMapLayout pieceMapLayout,
        PieceMapSnapshot pieceMapSnapshot,
        BoardShape shape,
        IBoardTopology topology,
        IOccupancyIndex occupancy,
        IAttackRays attackRays)
    {
        _bitboardLayout = bitboardLayout;
        _bitboardSnapshot = bitboardSnapshot;
        _pieceMapLayout = pieceMapLayout;
        _pieceMapSnapshot = pieceMapSnapshot;
        _shape = shape;
        _topology = topology;
        Occupancy = occupancy;
        AttackRays = attackRays;
    }

    public void OnStateTransition(GameState oldState, GameState newState, IGameEvent evt)
    {
        // Optimize common MovePiece path for incremental update; fallback to rebuild for others.
        if (evt is Flows.Events.MovePieceGameEvent mpe)
        {
            if (_shape.TryGetTileIndex(mpe.From, out var fromIdx) && _shape.TryGetTileIndex(mpe.To, out var toIdx))
            {
                // Update piece map first (authoritative validation of expected from index if needed)
                var expectedFrom = _pieceMapSnapshot.GetTileIndex(mpe.Piece);
                _pieceMapSnapshot = _pieceMapSnapshot.UpdateForMove(mpe.Piece, (short)fromIdx, (short)toIdx);
                _bitboardSnapshot = _bitboardSnapshot.UpdateForMove(mpe.Piece, (short)fromIdx, (short)toIdx, _pieceMapSnapshot, _shape);
                (Occupancy as IBitboardBackedOccupancy)?.BindSnapshot(_bitboardSnapshot);
                return;
            }
        }

        // Fallback full rebuild
        _pieceMapSnapshot = PieceMapSnapshot.Build(_pieceMapLayout, newState, _shape);
        _bitboardSnapshot = BitboardSnapshot.Build(_bitboardLayout, newState, _shape);
        (Occupancy as IBitboardBackedOccupancy)?.BindSnapshot(_bitboardSnapshot);
    }
}

/// <summary>
/// Optional extension interface for bitboard-backed occupancy indexes to accept updated snapshots from the acceleration context.
/// </summary>
internal interface IBitboardBackedOccupancy : IOccupancyIndex
{
    void BindSnapshot(BitboardSnapshot snapshot);
}