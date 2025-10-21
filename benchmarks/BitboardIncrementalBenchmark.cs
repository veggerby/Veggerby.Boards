using System.Diagnostics;

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

        var fromFull = _rebuild.Game.GetTile(ChessIds.Tiles.E2);
        Debug.Assert(fromFull is not null, "Benchmark setup artifact missing: e2 tile (full)");
        var toFull = _rebuild.Game.GetTile(ChessIds.Tiles.E4);
        Debug.Assert(toFull is not null, "Benchmark setup artifact missing: e4 tile (full)");
        _pawnFull = _rebuild.Game.GetPiece(ChessIds.Pieces.WhitePawn5)!;
        Debug.Assert(_pawnFull is not null, "Benchmark setup artifact missing: white pawn5 (full)");
        var visitorFull = new ResolveTilePathPatternVisitor(_rebuild.Game.Board, fromFull, toFull);
        _pathFull = visitorFull.ResultPath!;
        if (_pathFull is null)
        {
            _pathFull = TwoStepPathOrNull(_rebuild.Game, fromFull!, toFull!, ChessIds.Tiles.E3)!;
        }

        var fromInc = _incremental.Game.GetTile(ChessIds.Tiles.E2);
        Debug.Assert(fromInc is not null, "Benchmark setup artifact missing: e2 tile (incremental)");
        var toInc = _incremental.Game.GetTile(ChessIds.Tiles.E4);
        Debug.Assert(toInc is not null, "Benchmark setup artifact missing: e4 tile (incremental)");
        _pawnInc = _incremental.Game.GetPiece(ChessIds.Pieces.WhitePawn5)!;
        Debug.Assert(_pawnInc is not null, "Benchmark setup artifact missing: white pawn5 (incremental)");
        var visitorInc = new ResolveTilePathPatternVisitor(_incremental.Game.Board, fromInc, toInc);
        _pathInc = visitorInc.ResultPath!;
        if (_pathInc is null)
        {
            _pathInc = TwoStepPathOrNull(_incremental.Game, fromInc!, toInc!, ChessIds.Tiles.E3)!;
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

    /// <summary>
    /// Attempts to build a two-step path using an intermediate tile id. Returns null when relations are not connected.
    /// Benchmark-only helper (internal construction simplification).
    /// </summary>
    private static TilePath? TwoStepPathOrNull(Game game, Tile from, Tile to, string midTileId)
    {
        var mid = game.GetTile(midTileId);
        if (mid is null)
        {
            return null;
        }

        var r1 = game.Board.GetTileRelation(from, mid);
        var r2 = game.Board.GetTileRelation(mid, to);
        if (r1 is null || r2 is null)
        {
            return null;
        }

        var first = TilePath.Create(null, r1);
        return TilePath.Create(first, r2);
    }
}