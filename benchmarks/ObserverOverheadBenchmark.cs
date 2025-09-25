using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Observers;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Measures overhead of a counting <see cref="IEvaluationObserver"/> vs no observer when handling a simple move.
/// Runs with and without DecisionPlan enabled (param) to contextualize relative observer cost.
/// </summary>
[MemoryDiagnoser]
public class ObserverOverheadBenchmark
{
    private sealed class CountingObserver : IEvaluationObserver
    {
        public int PhaseEnterCount;
        public int RuleEvaluatedCount;
        public int RuleAppliedCount;
        public int EventIgnoredCount;
        public int StateHashedCount;

        public void OnPhaseEnter(Flows.Phases.GamePhase phase, GameState state) => PhaseEnterCount++;
        public void OnRuleEvaluated(Flows.Phases.GamePhase phase, Flows.Rules.IGameEventRule rule, ConditionResponse response, GameState state, int ruleIndex) => RuleEvaluatedCount++;
        public void OnRuleApplied(Flows.Phases.GamePhase phase, Flows.Rules.IGameEventRule rule, IGameEvent @event, GameState beforeState, GameState afterState, int ruleIndex) => RuleAppliedCount++;
        public void OnEventIgnored(IGameEvent @event, GameState state) => EventIgnoredCount++;
        public void OnStateHashed(GameState state, ulong hash) => StateHashedCount++;
        public void OnRuleSkipped(Flows.Phases.GamePhase phase, Flows.Rules.IGameEventRule rule, RuleSkipReason reason, GameState state, int ruleIndex) { }
    }

    [Params(false, true)]
    public bool EnableDecisionPlan { get; set; }

    private GameProgress _baseline = null!;
    private GameProgress _observed = null!;
    private GameProgress _batched = null!;
    private MovePieceGameEvent _event = null!;
    private CountingObserver _observer = null!;
    private CountingObserver _batchedObserver = null!;

    [GlobalSetup]
    public void Setup()
    {
        FeatureFlags.EnableDecisionPlan = EnableDecisionPlan;
        var builder1 = new Chess.ChessGameBuilder();
        _baseline = builder1.Compile();

    FeatureFlags.EnableDecisionPlan = EnableDecisionPlan;
    FeatureFlags.EnableObserverBatching = false;
    _observer = new CountingObserver();
    var builder2 = new Chess.ChessGameBuilder().WithObserver(_observer);
    _observed = builder2.Compile();

    FeatureFlags.EnableDecisionPlan = EnableDecisionPlan;
    FeatureFlags.EnableObserverBatching = true;
    _batchedObserver = new CountingObserver();
    var builder3 = new Chess.ChessGameBuilder().WithObserver(_batchedObserver);
    _batched = builder3.Compile();

        var piece = _baseline.Game.GetPiece("white-pawn-2");
        var from = _baseline.Game.GetTile("e2");
        var to = _baseline.Game.GetTile("e4");
        var path = new ResolveTilePathPatternVisitor(_baseline.Game.Board, from, to).ResultPath!;
        _event = new MovePieceGameEvent(piece, path);
    }

    [Benchmark(Baseline = true)]
    public GameProgress HandleEvent_NoObserver()
    {
        return _baseline.HandleEvent(_event);
    }

    [Benchmark]
    public GameProgress HandleEvent_CountingObserver()
    {
        return _observed.HandleEvent(_event);
    }

    [Benchmark]
    public GameProgress HandleEvent_BatchedObserver()
    {
        return _batched.HandleEvent(_event);
    }
}