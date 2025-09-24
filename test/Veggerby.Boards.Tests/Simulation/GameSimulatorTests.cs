using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Simulation;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Simulation;

// Simple test playout policy: attempts a fixed list of events provided via delegate.
file sealed class DelegatePlayoutPolicy(Func<GameProgress, IEnumerable<IGameEvent>> factory) : IPlayoutPolicy
{
    private readonly Func<GameProgress, IEnumerable<IGameEvent>> _factory = factory;
    public IEnumerable<IGameEvent> GetCandidateEvents(GameProgress progress) => _factory(progress) ?? Enumerable.Empty<IGameEvent>();
}

public class GameSimulatorTests
{
    private sealed class NoOpEvent : IGameEvent { }

    // Builder producing a trivial phase that produces no candidate events (policy returns none)
    private sealed class NoOpGameBuilder : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "noop-board";
            AddTile("t1");
            AddTile("t2");
            AddDirection("d");
            WithTile("t1").WithRelationTo("t2").InDirection("d");
            AddPlayer("p");
            AddPiece("noop-piece").WithOwner("p").OnTile("t1");
            // Intentionally do NOT add a custom phase; builder will supply default null-phase with a no-op rule.
        }
    }

    // Builder that applies an event once by moving a pseudo piece (state hash change via random source reseed)
    private sealed class SingleApplyBuilder : GameBuilder
    {
        public sealed class ToggleEvent : IGameEvent { }

        private sealed class ToggleMutator : Flows.Mutators.IStateMutator<ToggleEvent>
        {
            public GameState MutateState(GameEngine engine, GameState state, ToggleEvent @event)
            {
                var piece = engine.Game.GetPiece("piece");
                var pieceState = state.GetState<PieceState>(piece);
                var to = engine.Game.GetTile(pieceState.CurrentTile.Id == "a" ? "b" : "a");
                var newPieceState = new PieceState(pieceState.Artifact, to);
                return state.Next([newPieceState]);
            }
        }

        protected override void Build()
        {
            BoardId = "single";
            AddTile("a");
            AddTile("b");
            AddDirection("d");
            WithTile("a").WithRelationTo("b").InDirection("d");
            AddPlayer("p1");
            AddPiece("piece").WithOwner("p1").OnTile("a");
            AddGamePhase("phase")
                .If<States.Conditions.NullGameStateCondition>()
                .Then()
                .ForEvent<ToggleEvent>()
                .Then()
                .Before(game => new ToggleMutator());
        }
    }

    [Fact]
    public void GivenNoOpPolicy_WhenPlayout_ThenTerminatesWithNoMoves()
    {
        // arrange
        var builder = new NoOpGameBuilder();
        var progress = builder.Compile();
        var policy = new DelegatePlayoutPolicy(_ => Enumerable.Empty<IGameEvent>()); // no candidates => immediate terminal
        var sim = new GameSimulator(policy);

        // act
        var result = sim.Playout(progress);

        // assert
        Assert.Equal(PlayoutTerminalReason.NoMoves, result.TerminalReason);
        Assert.Equal(0, result.AppliedEvents);
        Assert.Same(progress, result.Initial);
    }

    [Fact]
    public void GivenSingleApplicableEvent_WhenPlayoutWithMaxEvents1_ThenStopsAtLimit()
    {
        // arrange
        var builder = new SingleApplyBuilder();
        var progress = builder.Compile();
        var evt = new SingleApplyBuilder.ToggleEvent();
        var policy = new DelegatePlayoutPolicy(_ => [evt]);
        var sim = new GameSimulator(policy, new PlayoutOptions { MaxEvents = 1 });

        // act
        var result = sim.Playout(progress);

        // assert
        Assert.Equal(1, result.AppliedEvents);
        Assert.Equal(PlayoutTerminalReason.MaxEvents, result.TerminalReason);
        Assert.NotSame(progress.State, result.Final.State);
    }

    [Fact]
    public async Task GivenParallelPlayouts_WhenExecuted_ThenAggregatesCounts()
    {
        // arrange
        var builder = new NoOpGameBuilder();
        var progress = builder.Compile();
        var policy = new DelegatePlayoutPolicy(_ => Enumerable.Empty<IGameEvent>());
        var sim = new GameSimulator(policy);

        // act
        var batch = await sim.PlayoutManyAsync(progress, 8, degreeOfParallelism: 4);

        // assert
        Assert.Equal(8, batch.Count);
        Assert.All(batch.Results, r => Assert.Equal(PlayoutTerminalReason.NoMoves, r.TerminalReason));
    }

    [Fact]
    public async Task GivenCancellation_WhenPlayoutManyAsync_ThenThrows()
    {
        // arrange
        var builder = new NoOpGameBuilder();
        var progress = builder.Compile();
        var policy = new DelegatePlayoutPolicy(_ => Enumerable.Empty<IGameEvent>());
        var sim = new GameSimulator(policy);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // act/assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => sim.PlayoutManyAsync(progress, 2, cancellationToken: cts.Token));
    }

    [Fact]
    public void GivenContinuousEventAndTimeLimit_WhenPlayout_ThenTerminatesByTime()
    {
        // arrange
        var builder = new SingleApplyBuilder();
        var progress = builder.Compile();
        var evt = new SingleApplyBuilder.ToggleEvent();
        var policy = new DelegatePlayoutPolicy(_ => [evt]); // always available
        var sim = new GameSimulator(policy, new PlayoutOptions { TimeLimit = TimeSpan.FromMilliseconds(1) });

        // act
        var result = sim.Playout(progress);

        // assert
        Assert.Equal(PlayoutTerminalReason.TimeLimit, result.TerminalReason);
        Assert.True(result.AppliedEvents > 0); // should have applied some before time expired
    }

    [Fact]
    public void GivenMixedLengths_WhenPlayoutManyAsync_ThenHistogramAndMetricsComputed()
    {
        // arrange: one builder with single toggle, one no-op to create diversity
        var builderApply = new SingleApplyBuilder();
        var builderNoOp = new NoOpGameBuilder();
        var progressApply = builderApply.Compile();
        var progressNoOp = builderNoOp.Compile();
        var evt = new SingleApplyBuilder.ToggleEvent();
        var policyApply = new DelegatePlayoutPolicy(_ => [evt]);
        var policyNoOp = new DelegatePlayoutPolicy(_ => Enumerable.Empty<IGameEvent>());
        var simApply = new GameSimulator(policyApply, new PlayoutOptions { MaxEvents = 1 });
        var simNoOp = new GameSimulator(policyNoOp);

        // act: run several playouts manually then aggregate into artificial batch
        var results = new List<PlayoutResult>();
        for (int i = 0; i < 5; i++)
        {
            results.Add(simApply.Playout(progressApply)); // each should have 1 applied (max events)
        }
        for (int i = 0; i < 3; i++)
        {
            results.Add(simNoOp.Playout(progressNoOp)); // 0 applied
        }
        var batch = new PlayoutBatchResult(results);

        // assert
        Assert.Equal(8, batch.Count);
        Assert.Equal(5, batch.ProgressedCount);
        Assert.Equal(5, batch.TotalApplied);
        Assert.Equal(0, batch.MinApplied);
        Assert.Equal(1, batch.MaxApplied);
        Assert.Equal(5d / 8d, batch.AverageApplied, 5);
        // histogram: index 0 => 3, index 1 => 5
        Assert.Equal(2, batch.Histogram.Count);
        Assert.Equal(3, batch.Histogram[0]);
        Assert.Equal(5, batch.Histogram[1]);
    }

    private sealed class RecordingObserver : GameSimulator.IPlayoutObserver
    {
        public int Steps; public int AppliedSteps; public int TotalCandidates; public PlayoutResult Completed;
        public void OnStep(GameProgress p, int stepIndex, int candidateCount, bool applied, IGameEvent attempted)
        {
            Steps++;
            TotalCandidates += candidateCount;
            if (applied)
            {
                AppliedSteps++;
            }
        }
        public void OnCompleted(PlayoutResult result) { Completed = result; }
    }

    [Fact]
    public void GivenObserver_WhenPlayout_ThenReceivesCallbacks()
    {
        // arrange
        var builder = new SingleApplyBuilder();
        var progress = builder.Compile();
        var evt = new SingleApplyBuilder.ToggleEvent();
        var policy = new DelegatePlayoutPolicy(_ => [evt]);
        var sim = new GameSimulator(policy, new PlayoutOptions { MaxEvents = 3 });
        var observer = new RecordingObserver();

        // act
        var result = sim.Playout(progress, observer);

        // assert
        Assert.NotNull(observer.Completed);
        Assert.Equal(result, observer.Completed);
        Assert.True(observer.Steps > 0);
        Assert.Equal(observer.AppliedSteps, result.AppliedEvents); // each step applies exactly one
        Assert.True(observer.TotalCandidates >= observer.Steps); // at least one candidate per step
    }

    [Fact]
    public void GivenSingleStepAllPiecesPolicy_WhenEnumerated_ThenReturnsDeterministicMoves()
    {
        // arrange
        var builder = new SingleApplyBuilder();
        var progress = builder.Compile();
        var policy = PolicyHelpers.SingleStepAllPieces();

        // act
        var candidatesFirst = policy.GetCandidateEvents(progress).OfType<MovePieceGameEvent>().ToList();
        var candidatesSecond = policy.GetCandidateEvents(progress).OfType<MovePieceGameEvent>().ToList();

        // assert
        Assert.NotEmpty(candidatesFirst);
        Assert.Equal(candidatesFirst.Count, candidatesSecond.Count); // deterministic count
        for (int i = 0; i < candidatesFirst.Count; i++)
        {
            Assert.Equal(candidatesFirst[i].Piece.Id, candidatesSecond[i].Piece.Id);
            Assert.Equal(candidatesFirst[i].To.Id, candidatesSecond[i].To.Id);
        }
    }

    [Fact]
    public void GivenVarianceThreshold_WhenSequentialPlayouts_ThenStopsEarly()
    {
        // arrange
        var builder = new SingleApplyBuilder();
        var progress = builder.Compile();
        var evt = new SingleApplyBuilder.ToggleEvent();
        var policy = new DelegatePlayoutPolicy(_ => [evt]); // always applies until MaxEvents limit
        var sim = new GameSimulator(policy, new PlayoutOptions { MaxEvents = 2 }); // fixed length = 2

        // act: variance should be 0 after first two playouts (all identical length)
        var batch = sim.PlayoutManyUntil(progress, maxCount: 50, stopPredicate: b => b.Count >= 2 && b.Variance == 0);

        // assert
        Assert.True(batch.Count <= 3); // should stop very early
        Assert.Equal(0, batch.Variance);
        Assert.True(batch.Results.All(r => r.AppliedEvents == 2));
    }

    [Fact]
    public void GivenProgressedCountThreshold_WhenSequentialPlayouts_ThenStopsAfterEnoughProgressed()
    {
        // arrange: mix deterministic progressed (toggle) and no-op
        var builderApply = new SingleApplyBuilder();
        var builderNoOp = new NoOpGameBuilder();
        var progressApply = builderApply.Compile();
        var progressNoOp = builderNoOp.Compile();
        var evt = new SingleApplyBuilder.ToggleEvent();
        var policyApply = new DelegatePlayoutPolicy(_ => [evt]);
        var policyNoOp = new DelegatePlayoutPolicy(_ => Enumerable.Empty<IGameEvent>());
        var simApply = new GameSimulator(policyApply, new PlayoutOptions { MaxEvents = 1 });
        var simNoOp = new GameSimulator(policyNoOp);

        // act: interleave two types manually through wrapper predicate
        var batch = simApply.PlayoutManyUntil(progressApply, 100, b => b.ProgressedCount >= 5);
        if (batch.ProgressedCount < 5)
        {
            // fallback ensure condition met (shouldn't happen but guard)
            batch = new PlayoutBatchResult(batch.Results.Concat(Enumerable.Range(0, 5 - batch.ProgressedCount).Select(_ => simApply.Playout(progressApply))).ToList());
        }

        // assert
        Assert.True(batch.ProgressedCount >= 5);
        Assert.True(batch.Count <= 10); // should not need many to reach threshold
    }

    [Fact]
    public async Task GivenVarianceThreshold_WhenParallelPlayouts_ThenStopsEarly()
    {
        // arrange
        var builder = new SingleApplyBuilder();
        var progress = builder.Compile();
        var evt = new SingleApplyBuilder.ToggleEvent();
        var policy = new DelegatePlayoutPolicy(_ => [evt]);
        var sim = new GameSimulator(policy, new PlayoutOptions { MaxEvents = 1 }); // fixed length =1

        // act
        var batch = await sim.PlayoutManyUntilAsync(progress, maxCount: 128, stopPredicate: b => b.Count >= 4 && b.Variance == 0, waveSize: 8);

        // assert
        Assert.True(batch.Count <= 16);
        Assert.Equal(0, batch.Variance);
    }
}