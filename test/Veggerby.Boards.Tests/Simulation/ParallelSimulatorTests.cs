namespace Veggerby.Boards.Tests.Simulation;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Simulation;

using AwesomeAssertions;
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
        Func<Task> act = async () => await ParallelSimulator.RunManyAsync(progress, 2, _ => _ => null);

        // assert
        await act.Should().ThrowAsync<InvalidOperationException>();
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
        var batch1Hashes = batch1.Results.Select(r => r.Final.State.GetHashCode()).Order().ToArray();
        var batch2Hashes = batch2.Results.Select(r => r.Final.State.GetHashCode()).Order().ToArray();
        batch1Hashes.Should().Equal(batch2Hashes);
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
        Func<Task> act = async () => await ParallelSimulator.RunManyAsync(progress, 4, SlowPolicyFactory, cancellationToken: cts.Token);

        // assert
        await act.Should().ThrowAsync<OperationCanceledException>();
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
        detailed.Basic.Results.Count.Should().Be(3);
        detailed.Metrics.Count.Should().Be(3);
        detailed.CancellationRequested.Should().BeFalse();
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
        partial.CancellationRequested.Should().BeTrue();
        partial.Basic.Results.Count.Should().BeLessThanOrEqualTo(10);
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
        basicHashes.Should().Equal(detailedHashes);
    }
}