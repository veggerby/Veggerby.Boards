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
internal sealed class BitboardAccelerationContext(
    BitboardLayout bitboardLayout,
    BitboardSnapshot bitboardSnapshot,
    PieceMapLayout pieceMapLayout,
    PieceMapSnapshot pieceMapSnapshot,
    BoardShape shape,
    IBoardTopology topology,
    IOccupancyIndex occupancy,
    IAttackRays attackRays) : IAccelerationContext
{
    private readonly BitboardLayout _bitboardLayout = bitboardLayout;
    private BitboardSnapshot _bitboardSnapshot = bitboardSnapshot;
    private readonly PieceMapLayout _pieceMapLayout = pieceMapLayout;
    private PieceMapSnapshot _pieceMapSnapshot = pieceMapSnapshot;
    private readonly BoardShape _shape = shape; // needed for rebuilds
    private readonly IBoardTopology _topology = topology; // optional for future index resolution helpers

    public IOccupancyIndex Occupancy { get; } = occupancy;
    public IAttackRays AttackRays { get; } = attackRays;

    public void OnStateTransition(GameState oldState, GameState newState, IGameEvent evt)
    {
        // Incremental path temporarily disabled pending correctness investigation (occupancy desync).
        // if (evt is Flows.Events.MovePieceGameEvent mpe)
        // {
        //     if (_shape.TryGetTileIndex(mpe.From, out var fromIdx) && _shape.TryGetTileIndex(mpe.To, out var toIdx))
        //     {
        //         var previousPieceMap = _pieceMapSnapshot;
        //         _bitboardSnapshot = _bitboardSnapshot.UpdateForMove(mpe.Piece, (short)fromIdx, (short)toIdx, previousPieceMap, _shape);
        //         _pieceMapSnapshot = _pieceMapSnapshot.UpdateForMove(mpe.Piece, (short)fromIdx, (short)toIdx);
        //         (Occupancy as IBitboardBackedOccupancy)?.BindSnapshot(_bitboardSnapshot);
        //         return;
        //     }
        // }

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