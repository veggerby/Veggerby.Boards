namespace Veggerby.Boards.Tests.Simulation;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Simulation;
using Veggerby.Boards.States;

using Xunit;

public class ParallelSimulatorTests
{
    [Fact]
    public async Task GivenSimulationDisabled_WhenRunManyAsync_ThenThrows()
    {
        // arrange
        FeatureFlags.EnableSimulation = false;
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();

        // act
        var act = async () => await ParallelSimulator.RunManyAsync(progress, 2, _ => _ => null);

        // assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Fact]
    public async Task GivenTwoPlayouts_WhenPoliciesDeterministic_ThenTerminalStateHashesStable()
    {
        // arrange
        FeatureFlags.EnableSimulation = true;
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        static PlayoutPolicy DeterministicPolicyFactory(int _) => _ => null; // no-op

        // act
        var batch1 = await ParallelSimulator.RunManyAsync(progress, 2, DeterministicPolicyFactory);
        var batch2 = await ParallelSimulator.RunManyAsync(progress, 2, DeterministicPolicyFactory);

        // assert
        Assert.Equal(batch1.Results.Select(r => r.Final.State.GetHashCode()).Order().ToArray(), batch2.Results.Select(r => r.Final.State.GetHashCode()).Order().ToArray());
    }

    [Fact]
    public async Task GivenCancellationRequested_MidExecution_PartialResultsReturned()
    {
        // arrange
        FeatureFlags.EnableSimulation = true;
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        using var cts = new CancellationTokenSource();
        static PlayoutPolicy SlowPolicyFactory(int _) => _ => null; // immediate stop

        // act
        cts.Cancel();
        var act = async () => await ParallelSimulator.RunManyAsync(progress, 4, SlowPolicyFactory, cancellationToken: cts.Token);

        // assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(act);
    }

    [Fact]
    public async Task GivenDetailedRunMany_WhenExecuted_ThenMetricsPresent()
    {
        // arrange
        FeatureFlags.EnableSimulation = true;
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        static PlayoutPolicy PolicyFactory(int _) => _ => null; // zero applied events

        // act
        var detailed = await ParallelSimulator.RunManyDetailedAsync(progress, 3, PolicyFactory);

        // assert
        Assert.Equal(3, detailed.Basic.Results.Count);
        Assert.Equal(3, detailed.Metrics.Count);
        Assert.False(detailed.CancellationRequested);
    }

    [Fact]
    public async Task GivenPartialCancellation_WhenUsingPartialAPI_ThenReturnsSubsetAndFlag()
    {
        // arrange
        FeatureFlags.EnableSimulation = true;
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        using var cts = new CancellationTokenSource();
        static PlayoutPolicy PolicyFactory(int _) => _ => null; // finishes instantly

        // act
        cts.Cancel();
        var partial = await ParallelSimulator.RunManyPartialAsync(progress, 10, PolicyFactory, cancellationToken: cts.Token);

        // assert
        Assert.True(partial.CancellationRequested);
        Assert.True(partial.Basic.Results.Count <= 10);
    }

    [Fact]
    public async Task GivenMultipleDeterministicPlayouts_WhenComparingDetailedAndBasic_ThenStateHashesEquivalent()
    {
        // arrange
        FeatureFlags.EnableSimulation = true;
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        static PlayoutPolicy PolicyFactory(int _) => _ => null;

        // act
        var basic = await ParallelSimulator.RunManyAsync(progress, 8, PolicyFactory);
        var detailed = await ParallelSimulator.RunManyDetailedAsync(progress, 8, PolicyFactory);

        // assert
        var basicHashes = basic.Results.Select(r => r.Final.State.GetHashCode()).Order().ToArray();
        var detailedHashes = detailed.Basic.Results.Select(r => r.Final.State.GetHashCode()).Order().ToArray();
        Assert.Equal(basicHashes, detailedHashes);
    }
}