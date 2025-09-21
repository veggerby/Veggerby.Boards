using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using Veggerby.Boards;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Observers;
using Veggerby.Boards.Flows.Phases;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Measures overhead introduced by enabling an evaluation observer during HandleEvent.
/// </summary>
/// <remarks>
/// Uses a simple counting observer that increments counters for callbacks; intended to approximate
/// the minimal structural cost of dispatch. Real observers (serialization, tracing) will incur more.
/// </remarks>
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

        public void OnPhaseEnter(GamePhase phase, GameState state) => PhaseEnterCount++;
        public void OnRuleEvaluated(GamePhase phase, IGameEventRule rule, ConditionResponse response, GameState state) => RuleEvaluatedCount++;
        public void OnRuleApplied(GamePhase phase, IGameEventRule rule, IGameEvent @event, GameState beforeState, GameState afterState) => RuleAppliedCount++;
        public void OnEventIgnored(IGameEvent @event, GameState state) => EventIgnoredCount++;
        public void OnStateHashed(GameState state, ulong hash) => StateHashedCount++;
    }

    [Params(false, true)]
    /// <summary>
    /// Toggles the DecisionPlan feature flag for the run. Executes each benchmark with and without the DecisionPlan path.
    /// </summary>
    public bool EnableDecisionPlan { get; set; }

    private GameProgress _baseline = null!;
    private GameProgress _observed = null!;
    private MovePieceGameEvent _event = null!;
    private CountingObserver _observer = null!;

    [GlobalSetup]
    /// <summary>
    /// Prepares baseline and observed game progress instances plus a representative move event.
    /// </summary>
    public void Setup()
    {
        // baseline without observer
        FeatureFlags.EnableDecisionPlan = EnableDecisionPlan;
        var builder1 = new ChessGameBuilder();
        _baseline = builder1.Compile();

        // observed with counting observer
        FeatureFlags.EnableDecisionPlan = EnableDecisionPlan; // ensure consistent flag
        _observer = new CountingObserver();
        var builder2 = new ChessGameBuilder().WithObserver(_observer);
        _observed = builder2.Compile();

        var piece = _baseline.Game.GetPiece("white-pawn-2");
        var from = _baseline.Game.GetTile("e2");
        var to = _baseline.Game.GetTile("e4");
        var path = new ResolveTilePathPatternVisitor(_baseline.Game.Board, from, to).ResultPath!;
        _event = new MovePieceGameEvent(piece, path);
    }

    [Benchmark(Baseline = true)]
    /// <summary>
    /// Handles the prepared event without an observer (baseline).
    /// </summary>
    public GameProgress HandleEvent_NoObserver()
    {
        return _baseline.HandleEvent(_event);
    }

    [Benchmark]
    /// <summary>
    /// Handles the prepared event with a counting observer attached.
    /// </summary>
    public GameProgress HandleEvent_CountingObserver()
    {
        return _observed.HandleEvent(_event);
    }
}

public static class ObserverOverheadProgram
{
    /// <summary>
    /// Entry point for running the observer overhead benchmarks.
    /// </summary>
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<ObserverOverheadBenchmark>();
    }
}