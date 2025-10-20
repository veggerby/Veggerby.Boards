using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Observers;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Measures incremental overhead of enabling trace capture and observer batching in combinations:
///   1. Baseline (no observer, no trace)
///   2. Observer only
///   3. Observer + Trace
///   4. Observer + Batching
///   5. Observer + Batching + Trace
/// DecisionPlan toggled to measure effect across evaluator variants.
/// </summary>
[MemoryDiagnoser]
public class TraceObserverBatchOverheadBenchmark
{
    private sealed class CountingObserver : IEvaluationObserver
    {
        public void OnPhaseEnter(Flows.Phases.GamePhase phase, GameState state)
        {
        }
        public void OnRuleEvaluated(Flows.Phases.GamePhase phase, Flows.Rules.IGameEventRule rule, ConditionResponse response, GameState state, int ruleIndex)
        {
        }
        public void OnRuleApplied(Flows.Phases.GamePhase phase, Flows.Rules.IGameEventRule rule, IGameEvent @event, GameState beforeState, GameState afterState, int ruleIndex)
        {
        }
        public void OnEventIgnored(IGameEvent @event, GameState state)
        {
        }
        public void OnStateHashed(GameState state, ulong hash)
        {
        }
        public void OnRuleSkipped(Flows.Phases.GamePhase phase, Flows.Rules.IGameEventRule rule, RuleSkipReason reason, GameState state, int ruleIndex)
        {
        }
    }

    [Params(false, true)]
    public bool SimulatedPlanMode
    {
        get; set;
    }

    private GameProgress _baseline = null!;
    private GameProgress _observer = null!;
    private GameProgress _observerTrace = null!;
    private GameProgress _observerBatched = null!;
    private GameProgress _observerBatchedTrace = null!;
    private MovePieceGameEvent _event = null!;

    [GlobalSetup]
    public void Setup()
    {
        var originalTrace = FeatureFlags.EnableTraceCapture;
        var originalBatch = FeatureFlags.EnableObserverBatching;
        try
        {
            FeatureFlags.EnableDecisionPlanGrouping = SimulatedPlanMode;
            FeatureFlags.EnableTraceCapture = false;
            FeatureFlags.EnableObserverBatching = false;
            _baseline = new ChessGameBuilder().Compile();

            FeatureFlags.EnableDecisionPlanGrouping = SimulatedPlanMode;
            FeatureFlags.EnableTraceCapture = false;
            FeatureFlags.EnableObserverBatching = false;
            _observer = new ChessGameBuilder().WithObserver(new CountingObserver()).Compile();

            FeatureFlags.EnableDecisionPlanGrouping = SimulatedPlanMode;
            FeatureFlags.EnableTraceCapture = true;
            FeatureFlags.EnableObserverBatching = false;
            _observerTrace = new ChessGameBuilder().WithObserver(new CountingObserver()).Compile();

            FeatureFlags.EnableDecisionPlanGrouping = SimulatedPlanMode;
            FeatureFlags.EnableTraceCapture = false;
            FeatureFlags.EnableObserverBatching = true;
            _observerBatched = new ChessGameBuilder().WithObserver(new CountingObserver()).Compile();

            FeatureFlags.EnableDecisionPlanGrouping = SimulatedPlanMode;
            FeatureFlags.EnableTraceCapture = true;
            FeatureFlags.EnableObserverBatching = true;
            _observerBatchedTrace = new ChessGameBuilder().WithObserver(new CountingObserver()).Compile();

            var piece = _baseline.Game.GetPiece(ChessIds.Pieces.WhitePawn2) ?? throw new InvalidOperationException("TraceObserverBatch: pawn2 missing");
            var from = _baseline.Game.GetTile(ChessIds.Tiles.E2) ?? throw new InvalidOperationException("TraceObserverBatch: tile e2 missing");
            var to = _baseline.Game.GetTile(ChessIds.Tiles.E4) ?? throw new InvalidOperationException("TraceObserverBatch: tile e4 missing");
            var path = new ResolveTilePathPatternVisitor(_baseline.Game.Board, from, to).ResultPath!;
            _event = new MovePieceGameEvent(piece, path);
        }
        finally
        {
            FeatureFlags.EnableTraceCapture = originalTrace;
            FeatureFlags.EnableObserverBatching = originalBatch;
        }
    }

    [Benchmark(Baseline = true)]
    public GameProgress Handle_Baseline() => _baseline.HandleEvent(_event);

    [Benchmark]
    public GameProgress Handle_Observer() => _observer.HandleEvent(_event);

    [Benchmark]
    public GameProgress Handle_ObserverTrace() => _observerTrace.HandleEvent(_event);

    [Benchmark]
    public GameProgress Handle_ObserverBatched() => _observerBatched.HandleEvent(_event);

    [Benchmark]
    public GameProgress Handle_ObserverBatchedTrace() => _observerBatchedTrace.HandleEvent(_event);
}