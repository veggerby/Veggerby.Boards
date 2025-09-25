using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts.Relations;
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
        FeatureFlags.EnableDecisionPlan = true;
        FeatureFlags.EnableDecisionPlanGrouping = true;
        FeatureFlags.EnableDecisionPlanEventFiltering = true; // representative config
        FeatureFlags.EnableDecisionPlanMasks = true;

        // Build baseline (parity disabled)
        FeatureFlags.EnableDecisionPlanDebugParity = false;
        _baseline = new Chess.ChessGameBuilder().Compile();

        // Build parity-enabled
        FeatureFlags.EnableDecisionPlanDebugParity = true;
        _parity = new Chess.ChessGameBuilder().Compile();
        FeatureFlags.EnableDecisionPlanDebugParity = false; // reset default

        var piece = _baseline.Game.GetPiece("white-pawn-2");
        var from = _baseline.Game.GetTile("e2");
        var to = _baseline.Game.GetTile("e4");
        var path = new ResolveTilePathPatternVisitor(_baseline.Game.Board, from, to).ResultPath!;
        _event = new MovePieceGameEvent(piece, path);
    }

    [Benchmark(Baseline = true)]
    public GameProgress HandleEvent_ParityDisabled()
    {
        FeatureFlags.EnableDecisionPlanDebugParity = false;
        return _baseline.HandleEvent(_event);
    }

    [Benchmark]
    public GameProgress HandleEvent_ParityEnabled()
    {
        FeatureFlags.EnableDecisionPlanDebugParity = true;
        var gp = _parity.HandleEvent(_event);
        FeatureFlags.EnableDecisionPlanDebugParity = false; // reset
        return gp;
    }
}