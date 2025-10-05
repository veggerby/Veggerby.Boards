using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Observers;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Benchmarks EventKind filtering vs no filtering for:
/// 1. Homogeneous move burst.
/// 2. Mixed move + state event stream (lower filtering hit ratio).
/// Captures rule evaluation counts via lightweight observer to quantify skip effectiveness.
/// </summary>
[MemoryDiagnoser]
public class EventKindFilteringBenchmark
{
    private GameProgress _legacy = null!;      // filtering disabled
    private GameProgress _filtering = null!;   // filtering enabled
    private MovePieceGameEvent[] _moves = null!;
    private IGameEvent[] _mixed50 = null!; // 50/50 moves vs state
    private IGameEvent[] _mixed80Move = null!; // ~80% moves
    private IGameEvent[] _mixed20Move = null!; // ~20% moves

    private sealed class CountingObserver : IEvaluationObserver
    {
        public int RuleEvaluations;
        public void OnPhaseEnter(Flows.Phases.GamePhase phase, GameState state) { }
        public void OnRuleEvaluated(Flows.Phases.GamePhase phase, Flows.Rules.IGameEventRule rule, ConditionResponse response, GameState state, int ruleIndex) { RuleEvaluations++; }
        public void OnRuleApplied(Flows.Phases.GamePhase phase, Flows.Rules.IGameEventRule rule, IGameEvent @event, GameState beforeState, GameState afterState, int ruleIndex) { }
        public void OnEventIgnored(IGameEvent @event, GameState state) { }
        public void OnStateHashed(GameState state, ulong hash) { }
        public void OnRuleSkipped(Flows.Phases.GamePhase phase, Flows.Rules.IGameEventRule rule, RuleSkipReason reason, GameState state, int ruleIndex) { }
    }

    private CountingObserver _obsLegacy = null!;
    private CountingObserver _obsFiltering = null!;

    [GlobalSetup]
    public void Setup()
    {
        // simulate plan grouping only (core plan flag removed)
        FeatureFlags.EnableDecisionPlanGrouping = true;
        FeatureFlags.EnableDecisionPlanEventFiltering = false; // baseline build
        _obsLegacy = new CountingObserver();
        _legacy = new ChessGameBuilder().WithObserver(_obsLegacy).Compile();

        FeatureFlags.EnableDecisionPlanEventFiltering = true; // filtering build
        _obsFiltering = new CountingObserver();
        _filtering = new ChessGameBuilder().WithObserver(_obsFiltering).Compile();
        FeatureFlags.EnableDecisionPlanEventFiltering = false; // reset

        var rook = _legacy.Game.GetPiece(ChessIds.Pieces.WhiteRook1);
        var from = _legacy.Game.GetTile("a1");
        var to = _legacy.Game.GetTile("b1");
        var forwardPath = new ResolveTilePathPatternVisitor(_legacy.Game.Board, from, to).ResultPath;
        if (forwardPath is null)
        {
            var forwardRel = _legacy.Game.Board.TileRelations.First(r => r.From.Equals(from) && r.To.Equals(to));
            forwardPath = new TilePath(new[] { forwardRel });
        }
        var backwardPath = new ResolveTilePathPatternVisitor(_legacy.Game.Board, to, from).ResultPath;
        if (backwardPath is null)
        {
            var backRel = _legacy.Game.Board.TileRelations.First(r => r.From.Equals(to) && r.To.Equals(from));
            backwardPath = new TilePath(new[] { backRel });
        }
        _moves = new MovePieceGameEvent[64];
        for (int i = 0; i < _moves.Length; i++)
        {
            var path = (i % 2 == 0) ? forwardPath : backwardPath; // alternate e2->e3 then e3->e2
            _moves[i] = new MovePieceGameEvent(rook, path);
        }

        _mixed50 = BuildMixed(32, 32);        // 32 moves + 32 state
        _mixed80Move = BuildMixed(51, 13);    // 51 moves + 13 state (~80/20)
        _mixed20Move = BuildMixed(13, 51);    // 13 moves + 51 state (~20/80)
    }

    private IGameEvent[] BuildMixed(int moveCount, int stateCount)
    {
        var list = new List<IGameEvent>(moveCount + stateCount);
        // Simple block ordering sufficient for measuring filtering evaluation reductions.
        for (int i = 0; i < moveCount && i < _moves.Length; i++)
        {
            list.Add(_moves[i]);
        }
        for (int s = 0; s < stateCount; s++)
        {
            list.Add(new BenchmarkStateNoOpEvent());
        }
        return list.ToArray();
    }

    [Benchmark(Baseline = true)]
    public GameProgress Legacy_NoFiltering_MoveBurst()
    {
#if !DEBUG && !TESTS
        // In release builds legacy traversal is compiled out; fall back to filtering-disabled DecisionPlan path (feature flag off)
#endif
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
        FeatureFlags.EnableDecisionPlanEventFiltering = true;
        var gp = _filtering;
        foreach (var m in _moves)
        {
            gp = gp.HandleEvent(m);
        }
        FeatureFlags.EnableDecisionPlanEventFiltering = false;
        return gp;
    }

    [Benchmark]
    public (int evalLegacy, int evalFiltering) Mixed50_50_EvaluationCounts()
    {
        return RunMixed(_mixed50);
    }

    [Benchmark]
    public (int evalLegacy, int evalFiltering) Mixed80_20_EvaluationCounts() => RunMixed(_mixed80Move);

    [Benchmark]
    public (int evalLegacy, int evalFiltering) Mixed20_80_EvaluationCounts() => RunMixed(_mixed20Move);

    // Performance-focused variants (same streams) returning final GameProgress to measure time/allocations.
    [Benchmark]
    public GameProgress Legacy_NoFiltering_Mixed50_50()
    {
        FeatureFlags.EnableDecisionPlanEventFiltering = false;
        var gp = _legacy;
        foreach (var e in _mixed50)
        {
            gp = gp.HandleEvent(e);
        }
        return gp;
    }

    [Benchmark]
    public GameProgress Filtering_Mixed50_50()
    {
        FeatureFlags.EnableDecisionPlanEventFiltering = true;
        var gp = _filtering;
        foreach (var e in _mixed50)
        {
            gp = gp.HandleEvent(e);
        }
        FeatureFlags.EnableDecisionPlanEventFiltering = false;
        return gp;
    }

    [Benchmark]
    public GameProgress Legacy_NoFiltering_Mixed80_20()
    {
        FeatureFlags.EnableDecisionPlanEventFiltering = false;
        var gp = _legacy;
        foreach (var e in _mixed80Move)
        {
            gp = gp.HandleEvent(e);
        }
        return gp;
    }

    [Benchmark]
    public GameProgress Filtering_Mixed80_20()
    {
        FeatureFlags.EnableDecisionPlanEventFiltering = true;
        var gp = _filtering;
        foreach (var e in _mixed80Move)
        {
            gp = gp.HandleEvent(e);
        }
        FeatureFlags.EnableDecisionPlanEventFiltering = false;
        return gp;
    }

    [Benchmark]
    public GameProgress Legacy_NoFiltering_Mixed20_80()
    {
        FeatureFlags.EnableDecisionPlanEventFiltering = false;
        var gp = _legacy;
        foreach (var e in _mixed20Move)
        {
            gp = gp.HandleEvent(e);
        }
        return gp;
    }

    [Benchmark]
    public GameProgress Filtering_Mixed20_80()
    {
        FeatureFlags.EnableDecisionPlanEventFiltering = true;
        var gp = _filtering;
        foreach (var e in _mixed20Move)
        {
            gp = gp.HandleEvent(e);
        }
        FeatureFlags.EnableDecisionPlanEventFiltering = false;
        return gp;
    }

    private (int evalLegacy, int evalFiltering) RunMixed(IGameEvent[] events)
    {
        FeatureFlags.EnableDecisionPlanEventFiltering = false;
        _obsLegacy.RuleEvaluations = 0;
        var gpLegacy = _legacy;
        foreach (var e in events)
        {
            gpLegacy = gpLegacy.HandleEvent(e);
        }

        FeatureFlags.EnableDecisionPlanEventFiltering = true;
        _obsFiltering.RuleEvaluations = 0;
        var gpFiltering = _filtering;
        foreach (var e in events)
        {
            gpFiltering = gpFiltering.HandleEvent(e);
        }
        FeatureFlags.EnableDecisionPlanEventFiltering = false;
        return (_obsLegacy.RuleEvaluations, _obsFiltering.RuleEvaluations);
    }
}