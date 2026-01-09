using System.Collections.Generic;

using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Examples.RockPaperScissors;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Benchmark comparing commit/reveal overhead vs sequential event processing.
/// </summary>
/// <remarks>
/// This benchmark measures the performance impact of simultaneous turns (commit/reveal pattern)
/// compared to sequential event application. It validates that the commitment system adds
/// minimal overhead (target: &lt; 5% vs sequential).
/// </remarks>
[MemoryDiagnoser]
public class SimultaneousTurnsBenchmark
{
    private GameProgress _progressSimultaneous = null!;
    private GameProgress _progressSequential = null!;
    private Player _player1 = null!;
    private Player _player2 = null!;
    private SelectChoiceEvent _choice1 = null!;
    private SelectChoiceEvent _choice2 = null!;

    /// <summary>
    /// Global benchmark setup creating two identical game states for comparison.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var builder = new RockPaperScissorsGameBuilder();

        // Setup simultaneous version
        _progressSimultaneous = builder.Compile();
        _player1 = _progressSimultaneous.Game.GetPlayer("player-1") ?? throw new InvalidOperationException("player-1 missing");
        _player2 = _progressSimultaneous.Game.GetPlayer("player-2") ?? throw new InvalidOperationException("player-2 missing");

        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { _player1, _player2 };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = _progressSimultaneous.State.Next([stagedState]);
        _progressSimultaneous = new GameProgress(_progressSimultaneous.Engine, stateWithStaged, null);

        // Setup sequential version (no staged state)
        _progressSequential = builder.Compile();

        // Pre-allocate choice events to avoid allocation overhead in benchmark
        _choice1 = new SelectChoiceEvent(_player1, Choice.Rock);
        _choice2 = new SelectChoiceEvent(_player2, Choice.Scissors);
    }

    /// <summary>
    /// Benchmarks simultaneous turns using commit/reveal pattern.
    /// </summary>
    /// <returns>The resulting <see cref="GameProgress"/> after reveal.</returns>
    [Benchmark]
    public GameProgress SimultaneousTurns()
    {
        var progress = _progressSimultaneous;

        // Commit both actions
        progress = progress.HandleEvent(new CommitActionEvent(_player1, _choice1));
        progress = progress.HandleEvent(new CommitActionEvent(_player2, _choice2));

        // Reveal
        progress = progress.HandleEvent(new RevealCommitmentsEvent());

        return progress;
    }

    /// <summary>
    /// Benchmarks sequential event processing (baseline for comparison).
    /// </summary>
    /// <returns>The resulting <see cref="GameProgress"/> after both events.</returns>
    [Benchmark(Baseline = true)]
    public GameProgress SequentialTurns()
    {
        var progress = _progressSequential;

        // Apply events sequentially
        progress = progress.HandleEvent(_choice1);
        progress = progress.HandleEvent(_choice2);

        return progress;
    }

    /// <summary>
    /// Benchmarks only the commit phase (staging without reveal).
    /// </summary>
    /// <returns>The resulting <see cref="GameProgress"/> after commits.</returns>
    [Benchmark]
    public GameProgress CommitPhaseOnly()
    {
        var progress = _progressSimultaneous;

        // Commit both actions (no reveal)
        progress = progress.HandleEvent(new CommitActionEvent(_player1, _choice1));
        progress = progress.HandleEvent(new CommitActionEvent(_player2, _choice2));

        return progress;
    }

    /// <summary>
    /// Benchmarks reveal phase with pre-committed actions.
    /// </summary>
    /// <returns>The resulting <see cref="GameProgress"/> after reveal.</returns>
    [Benchmark]
    public GameProgress RevealPhaseOnly()
    {
        // Pre-commit actions during setup for this benchmark
        var progress = _progressSimultaneous;
        progress = progress.HandleEvent(new CommitActionEvent(_player1, _choice1));
        progress = progress.HandleEvent(new CommitActionEvent(_player2, _choice2));

        // Benchmark only the reveal
        progress = progress.HandleEvent(new RevealCommitmentsEvent());

        return progress;
    }
}
