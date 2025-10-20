using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Measures relative overhead of incremental bitboard + piece map updates vs full rebuild during a representative
/// piece move (white pawn e2→e4). The incremental path is guarded by feature flag <c>EnableBitboardIncremental</c>.
/// Target: incremental path should be measurably faster (allocation &lt; full rebuild) while remaining parity safe.
/// </summary>
/// <remarks>
/// Benchmark constructs two game instances: one with incremental updates enabled, one with default full rebuild.
/// Only a single representative move is applied per iteration – broader randomized suites are expected externally.
/// Style charter adhered to: explicit braces, no LINQ in hot loop, immutable state transitions.
/// </remarks>
[MemoryDiagnoser]
public class BitboardIncrementalBenchmark
{
    private GameProgress _incremental = null!;
    private GameProgress _rebuild = null!;
    private Piece _pawnInc = null!;
    private Piece _pawnFull = null!;
    private TilePath _pathInc = null!;
    private TilePath _pathFull = null!;

    [GlobalSetup]
    public void Setup()
    {
        var origInc = Internal.FeatureFlags.EnableBitboardIncremental;
        try
        {
            Internal.FeatureFlags.EnableBitboardIncremental = false;
            _rebuild = new ChessGameBuilder().Compile();

            Internal.FeatureFlags.EnableBitboardIncremental = true;
            _incremental = new ChessGameBuilder().Compile();
        }
        finally
        {
            Internal.FeatureFlags.EnableBitboardIncremental = origInc;
        }

        var fromFull = _rebuild.Game.GetTile(ChessIds.Tiles.E2) ?? throw new InvalidOperationException("BitboardIncremental: from tile e2 missing (full)");
        var toFull = _rebuild.Game.GetTile(ChessIds.Tiles.E4) ?? throw new InvalidOperationException("BitboardIncremental: to tile e4 missing (full)");
        _pawnFull = _rebuild.Game.GetPiece(ChessIds.Pieces.WhitePawn5) ?? throw new InvalidOperationException("BitboardIncremental: pawn5 missing (full)");
        var visitorFull = new ResolveTilePathPatternVisitor(_rebuild.Game.Board, fromFull, toFull);
        _pathFull = visitorFull.ResultPath!;
        if (_pathFull is null)
        {
            var mid = _rebuild.Game.GetTile(ChessIds.Tiles.E3) ?? throw new InvalidOperationException("BitboardIncremental benchmark: mid tile e3 missing (full)");
            var rel1 = _rebuild.Game.Board.GetTileRelation(fromFull, mid);
            var rel2 = _rebuild.Game.Board.GetTileRelation(mid, toFull);
            if (rel1 is not null && rel2 is not null)
            {
                var partial = TilePath.Create(null, rel1);
                _pathFull = TilePath.Create(partial, rel2);
            }
        }

        var fromInc = _incremental.Game.GetTile(ChessIds.Tiles.E2) ?? throw new InvalidOperationException("BitboardIncremental: from tile e2 missing (incremental)");
        var toInc = _incremental.Game.GetTile(ChessIds.Tiles.E4) ?? throw new InvalidOperationException("BitboardIncremental: to tile e4 missing (incremental)");
        _pawnInc = _incremental.Game.GetPiece(ChessIds.Pieces.WhitePawn5) ?? throw new InvalidOperationException("BitboardIncremental: pawn5 missing (incremental)");
        var visitorInc = new ResolveTilePathPatternVisitor(_incremental.Game.Board, fromInc, toInc);
        _pathInc = visitorInc.ResultPath!;
        if (_pathInc is null)
        {
            var mid2 = _incremental.Game.GetTile(ChessIds.Tiles.E3) ?? throw new InvalidOperationException("BitboardIncremental benchmark: mid tile e3 missing (incremental)");
            var rel1b = _incremental.Game.Board.GetTileRelation(fromInc, mid2);
            var rel2b = _incremental.Game.Board.GetTileRelation(mid2, toInc);
            if (rel1b is not null && rel2b is not null)
            {
                var partial2 = TilePath.Create(null, rel1b);
                _pathInc = TilePath.Create(partial2, rel2b);
            }
        }
    }

    [Benchmark(Baseline = true)]
    public GameProgress MovePawn_FullRebuild()
    {
        return _pathFull is null ? _rebuild : _rebuild.HandleEvent(new MovePieceGameEvent(_pawnFull, _pathFull));
    }

    [Benchmark]
    public GameProgress MovePawn_Incremental()
    {
        return _pathInc is null ? _incremental : _incremental.HandleEvent(new MovePieceGameEvent(_pawnInc, _pathInc));
    }
}