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
}