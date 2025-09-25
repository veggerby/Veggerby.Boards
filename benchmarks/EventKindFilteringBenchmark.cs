using System;

using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Measures impact of EventKind filtering feature flag on rule evaluation counts when processing
/// a mixed sequence of events (Move + synthetic State events). Baseline: filtering disabled; Variant: enabled.
/// Focus metric: relative throughput + (manual) evaluation count captured via IEvaluationObserver hook (future enhancement).
/// </summary>
[MemoryDiagnoser]
public class EventKindFilteringBenchmark
{
    private GameProgress _legacy = null!;
    private GameProgress _filtering = null!;
    private MovePieceGameEvent[] _moves = null!;

    [GlobalSetup]
    public void Setup()
    {
        // build baseline game
        // baseline (filtering off)
        FeatureFlags.EnableDecisionPlan = true;
        FeatureFlags.EnableDecisionPlanGrouping = true;
        FeatureFlags.EnableDecisionPlanEventFiltering = false;
        _legacy = new ChessGameBuilder().Compile();

        // variant (filtering on)
        FeatureFlags.EnableDecisionPlanEventFiltering = true;
        _filtering = new ChessGameBuilder().Compile();

        // reset flag post-build (benchmarks toggle via instances only)
        FeatureFlags.EnableDecisionPlanEventFiltering = false;

        // pre-generate deterministic move events (white pawn e2->e3 single push repeated)
        var pawn = _legacy.Game.GetPiece("white-pawn-2");
        var from = _legacy.Game.GetTile("e2");
        var to = _legacy.Game.GetTile("e3");
        var path = new ResolveTilePathPatternVisitor(_legacy.Game.Board, from, to).ResultPath!;
        _moves = new MovePieceGameEvent[64];
        for (var i = 0; i < _moves.Length; i++)
        {
            _moves[i] = new MovePieceGameEvent(pawn, path);
        }
    }

    [Benchmark(Baseline = true)]
    public GameProgress Legacy_NoFiltering_MoveBurst()
    {
        var gp = _legacy;
        foreach (var m in _moves)
        {
            gp = gp.HandleEvent(m);
        }
        return gp;
    }

    [Benchmark]
    public GameProgress DecisionPlan_Filtering_MoveBurst()
    {
        // ensure filtering flag enabled during run (no rebuild needed; evaluator branches consult flag)
        FeatureFlags.EnableDecisionPlanEventFiltering = true;
        var gp = _filtering;
        foreach (var m in _moves)
        {
            gp = gp.HandleEvent(m);
        }
        FeatureFlags.EnableDecisionPlanEventFiltering = false;
        return gp;
    }
}