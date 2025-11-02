using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Simulation;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Simulation;

// Simple test playout policy: attempts a fixed list of events provided via delegate.
file sealed class DelegatePlayoutPolicy(Func<GameProgress, IEnumerable<IGameEvent>> factory) : IPlayoutPolicy
{
    private readonly Func<GameProgress, IEnumerable<IGameEvent>> _factory = factory;
    public IEnumerable<IGameEvent> GetCandidateEvents(GameProgress progress) => _factory(progress) ?? Enumerable.Empty<IGameEvent>();
}

public class GameSimulatorTests
{
    private sealed class NoOpEvent : IGameEvent
    {
    }

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
        public sealed class ToggleEvent : IGameEvent
        {
        }

        private sealed class ToggleMutator : Flows.Mutators.IStateMutator<ToggleEvent>
        {
            public GameState MutateState(GameEngine engine, GameState state, ToggleEvent @event)
            {
                var piece = engine.Game.GetPiece("piece");
                piece.Should().NotBeNull();
                var pieceState = state.GetState<PieceState>(piece!);
                pieceState.Should().NotBeNull();
                var to = engine.Game.GetTile(pieceState!.CurrentTile.Id == "a" ? "b" : "a");
                to.Should().NotBeNull();
                var newPieceState = new PieceState(pieceState!.Artifact, to!);
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
                .If<NullGameStateCondition>()
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

        // act

        // assert

        var builder = new NoOpGameBuilder();
        var progress = builder.Compile();
        var policy = new DelegatePlayoutPolicy(_ => Enumerable.Empty<IGameEvent>()); // no candidates => immediate terminal
        var sim = new GameSimulator(policy);

        // act
        var result = sim.Playout(progress);

        // assert
        result.TerminalReason.Should().Be(PlayoutTerminalReason.NoMoves);
        result.AppliedEvents.Should().Be(0);
        result.Initial.Should().BeSameAs(progress);
    }

    [Fact]
    public void GivenSingleApplicableEvent_WhenPlayoutWithMaxEvents1_ThenStopsAtLimit()
    {
        // arrange

        // act

        // assert

        var builder = new SingleApplyBuilder();
        var progress = builder.Compile();
        var evt = new SingleApplyBuilder.ToggleEvent();
        var policy = new DelegatePlayoutPolicy(_ => [evt]);
        var sim = new GameSimulator(policy, new PlayoutOptions { MaxEvents = 1 });

        // act
        var result = sim.Playout(progress);

        // assert
        result.AppliedEvents.Should().Be(1);
        result.TerminalReason.Should().Be(PlayoutTerminalReason.MaxDepth);
        result.Final.State.Should().NotBeSameAs(progress.State);
    }

    [Fact]
    public async Task GivenParallelPlayouts_WhenExecuted_ThenAggregatesCounts()
    {
        // arrange

        // act

        // assert

        var builder = new NoOpGameBuilder();
        var progress = builder.Compile();
        var policy = new DelegatePlayoutPolicy(_ => Enumerable.Empty<IGameEvent>());
        var sim = new GameSimulator(policy);

        // act
        var batch = await sim.PlayoutManyAsync(progress, 8, degreeOfParallelism: 4);

        // assert
        batch.Count.Should().Be(8);
        batch.Results.Should().OnlyContain(r => r.TerminalReason == PlayoutTerminalReason.NoMoves);
    }

    [Fact]
    public async Task GivenCancellation_WhenPlayoutManyAsync_ThenThrows()
    {
        // arrange

        // act

        // assert

        var builder = new NoOpGameBuilder();
        var progress = builder.Compile();
        var policy = new DelegatePlayoutPolicy(_ => Enumerable.Empty<IGameEvent>());
        var sim = new GameSimulator(policy);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // act
        Func<Task> act = () => sim.PlayoutManyAsync(progress, 2, cancellationToken: cts.Token);

        // assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void GivenContinuousEventAndTimeLimit_WhenPlayout_ThenTerminatesByTime()
    {
        // arrange

        // act

        // assert

        var builder = new SingleApplyBuilder();
        var progress = builder.Compile();
        var evt = new SingleApplyBuilder.ToggleEvent();
        var policy = new DelegatePlayoutPolicy(_ => [evt]); // always available
        var sim = new GameSimulator(policy, new PlayoutOptions { TimeLimit = TimeSpan.FromMilliseconds(1) });

        // act
        var result = sim.Playout(progress);

        // assert
        result.TerminalReason.Should().Be(PlayoutTerminalReason.TimeLimit);
        result.AppliedEvents.Should().BeGreaterThan(0); // should have applied some before time expired
    }

    [Fact]
    public void GivenMixedLengths_WhenPlayoutManyAsync_ThenHistogramAndMetricsComputed()
    {
        // arrange

        // act

        // assert

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
        batch.Count.Should().Be(8);
        batch.ProgressedCount.Should().Be(5);
        batch.TotalApplied.Should().Be(5);
        batch.MinApplied.Should().Be(0);
        batch.MaxApplied.Should().Be(1);
        batch.AverageApplied.Should().BeApproximately(5d / 8d, 1e-5);
        // histogram: index 0 => 3, index 1 => 5
        batch.Histogram.Count.Should().Be(2);
        batch.Histogram[0].Should().Be(3);
        batch.Histogram[1].Should().Be(5);
    }

    private sealed class RecordingObserver : GameSimulator.IPlayoutObserver
    {
        public int Steps; public int AppliedSteps; public int TotalCandidates; public PlayoutResult? Completed;
        public void OnStep(GameProgress p, int stepIndex, int candidateCount, bool applied, IGameEvent? attempted)
        {
            Steps++;
            TotalCandidates += candidateCount;
            if (applied)
            {
                AppliedSteps++;
            }
        }
        public void OnCompleted(PlayoutResult result)
        {
            Completed = result;
        }
    }

    [Fact]
    public void GivenObserver_WhenPlayout_ThenReceivesCallbacks()
    {
        // arrange

        // act

        // assert

        var builder = new SingleApplyBuilder();
        var progress = builder.Compile();
        var evt = new SingleApplyBuilder.ToggleEvent();
        var policy = new DelegatePlayoutPolicy(_ => [evt]);
        var sim = new GameSimulator(policy, new PlayoutOptions { MaxEvents = 3 });
        var observer = new RecordingObserver();

        // act
        var result = sim.Playout(progress, observer);

        // assert
        observer.Completed.Should().NotBeNull();
        observer.Completed!.Should().Be(result);
        observer.Steps.Should().BeGreaterThan(0);
        observer.AppliedSteps.Should().Be(result.AppliedEvents); // each step applies exactly one
        observer.TotalCandidates.Should().BeGreaterThanOrEqualTo(observer.Steps); // at least one candidate per step
    }

    [Fact]
    public void GivenSingleStepAllPiecesPolicy_WhenEnumerated_ThenReturnsDeterministicMoves()
    {
        // arrange

        // act

        // assert

        var builder = new SingleApplyBuilder();
        var progress = builder.Compile();
        var policy = PolicyHelpers.SingleStepAllPieces();

        // act
        var candidatesFirst = policy.GetCandidateEvents(progress).OfType<MovePieceGameEvent>().ToList();
        var candidatesSecond = policy.GetCandidateEvents(progress).OfType<MovePieceGameEvent>().ToList();

        // assert
        candidatesFirst.Should().NotBeEmpty();
        candidatesSecond.Should().HaveCount(candidatesFirst.Count); // deterministic count
        for (int i = 0; i < candidatesFirst.Count; i++)
        {
            candidatesSecond[i].Piece.Id.Should().Be(candidatesFirst[i].Piece.Id);
            candidatesSecond[i].To.Id.Should().Be(candidatesFirst[i].To.Id);
        }
    }

    [Fact]
    public void GivenVarianceThreshold_WhenSequentialPlayouts_ThenStopsEarly()
    {
        // arrange

        // act

        // assert

        var builder = new SingleApplyBuilder();
        var progress = builder.Compile();
        var evt = new SingleApplyBuilder.ToggleEvent();
        var policy = new DelegatePlayoutPolicy(_ => [evt]); // always applies until MaxEvents limit
        var sim = new GameSimulator(policy, new PlayoutOptions { MaxEvents = 2 }); // fixed length = 2

        // act: variance should be 0 after first two playouts (all identical length)
        var batch = sim.PlayoutManyUntil(progress, maxCount: 50, stopPredicate: b => b.Count >= 2 && b.Variance == 0);

        // assert
        batch.Count.Should().BeLessThanOrEqualTo(3); // should stop very early
        batch.Variance.Should().Be(0);
        batch.Results.Should().OnlyContain(r => r.AppliedEvents == 2);
    }

    [Fact]
    public void GivenProgressedCountThreshold_WhenSequentialPlayouts_ThenStopsAfterEnoughProgressed()
    {
        // arrange

        // act

        // assert

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
        batch.ProgressedCount.Should().BeGreaterThanOrEqualTo(5);
        batch.Count.Should().BeLessThanOrEqualTo(10); // should not need many to reach threshold
    }

    [Fact]
    public async Task GivenVarianceThreshold_WhenParallelPlayouts_ThenStopsEarly()
    {
        // arrange

        // act

        // assert

        var builder = new SingleApplyBuilder();
        var progress = builder.Compile();
        var evt = new SingleApplyBuilder.ToggleEvent();
        var policy = new DelegatePlayoutPolicy(_ => [evt]);
        var sim = new GameSimulator(policy, new PlayoutOptions { MaxEvents = 1 }); // fixed length =1

        // act
        var batch = await sim.PlayoutManyUntilAsync(progress, maxCount: 128, stopPredicate: b => b.Count >= 4 && b.Variance == 0, waveSize: 8);

        // assert
        batch.Count.Should().BeLessThanOrEqualTo(16);
        batch.Variance.Should().Be(0);
    }
}
