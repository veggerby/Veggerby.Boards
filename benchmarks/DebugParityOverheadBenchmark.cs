using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Chess.Constants;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Measures overhead of enabling debug parity dual-run (DecisionPlan + legacy) during event handling.
/// This benchmark is intended for diagnostic builds (DEBUG configuration) – in RELEASE the legacy path is compiled out
/// so parity flag exerts minimal overhead beyond the conditional check.
/// </summary>
[MemoryDiagnoser]
public class DebugParityOverheadBenchmark
{
    private GameProgress _baseline = null!; // parity disabled
    private GameProgress _parity = null!;   // parity enabled
    private MovePieceGameEvent _event = null!;

    [GlobalSetup]
    public void Setup()
    {
        // DecisionPlan core flag removed; simulate representative config via grouping + filtering + masks

        // Build baseline (parity disabled)
        // parity flag removed – treat baseline identical
        _baseline = new ChessGameBuilder().Compile();

        // Build parity-enabled
        _parity = new ChessGameBuilder().Compile();

        var piece = _baseline.Game.GetPiece(ChessIds.Pieces.WhitePawn2) ?? throw new InvalidOperationException("Benchmark setup: white pawn not found");
        var from = _baseline.Game.GetTile(ChessIds.Tiles.E2) ?? throw new InvalidOperationException("Benchmark setup: from tile e2 not found");
        var to = _baseline.Game.GetTile(ChessIds.Tiles.E4) ?? throw new InvalidOperationException("Benchmark setup: to tile e4 not found");
        var path = new ResolveTilePathPatternVisitor(_baseline.Game.Board, from, to).ResultPath!;
        _event = new MovePieceGameEvent(piece, path);
    }

    [Benchmark(Baseline = true)]
    public GameProgress HandleEvent_ParityDisabled()
    {
        return _baseline.HandleEvent(_event);
    }

    [Benchmark]
    public GameProgress HandleEvent_ParityEnabled()
    {
        // simulate parity path (identical currently)
        return _parity.HandleEvent(_event);
    }
}