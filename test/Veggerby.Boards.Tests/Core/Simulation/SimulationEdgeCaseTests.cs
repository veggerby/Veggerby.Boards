using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Simulation;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Core.Simulation;

/// <summary>
/// Edge case coverage for simulator APIs ensuring validation, determinism, cancellation handling, and terminal reasons.
/// </summary>
public class SimulationEdgeCaseTests
{
    private static GameProgress BuildProgress()
    {
        var builder = new MinimalSimGameBuilder();
        return builder.Compile();
    }

    [Fact]
    public async Task GivenParallelSimulator_WhenPlayoutCountZero_ThenThrows()
    {
        FeatureFlags.EnableSimulation = true;
        var progress = BuildProgress();
        var act = async () => await ParallelSimulator.RunManyAsync(progress, 0, _ => (_ => new TurnPassEvent()));
        var ex = await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        ex.Which.ParamName.Should().Be("playoutCount");
    }

    [Fact]
    public async Task GivenParallelPartialSimulator_WhenCancelledEarly_ThenReturnsPartial()
    {
        FeatureFlags.EnableSimulation = true;
        var progress = BuildProgress();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // cancel immediately to force partial (zero or minimal results)
        var detailed = await ParallelSimulator.RunManyPartialAsync(progress, 5, _ => (_ => new TurnPassEvent()), cancellationToken: cts.Token);
        detailed.Should().NotBeNull();
        detailed.CancellationRequested.Should().BeTrue();
        detailed.Basic.Results.Count.Should().BeLessThan(5);
    }

    [Fact]
    public async Task GivenParallelSimulator_WhenRunTwiceWithSamePolicy_ThenDeterministicMultiset()
    {
        FeatureFlags.EnableSimulation = true;
        var progress = BuildProgress();
        // Simple policy: always pass turn until first null (maxDepth will terminate)
        PlayoutPolicy policy = _ => new TurnPassEvent();
        var batch1 = await ParallelSimulator.RunManyAsync(progress, 4, _ => policy, maxDepth: 3);
        var batch2 = await ParallelSimulator.RunManyAsync(progress, 4, _ => policy, maxDepth: 3);
        var hashes1 = batch1.Results.Select(r => r.Final.State.GetHashCode()).OrderBy(h => h).ToArray();
        var hashes2 = batch2.Results.Select(r => r.Final.State.GetHashCode()).OrderBy(h => h).ToArray();
        hashes1.Should().Equal(hashes2);
    }

    [Fact]
    public void GivenSequentialSimulator_WhenPolicyProducesNoProgress_ThenTerminalReasonPolicyReturnedNull()
    {
        FeatureFlags.EnableSimulation = true;
        var progress = BuildProgress();
        // Pass event in current minimal phase yields no state change; simulator treats non-progress as terminal PolicyReturnedNull.
        var detailed = SequentialSimulator.RunDetailed(progress, _ => new TurnPassEvent(), maxDepth: 2);
        detailed.Result.TerminalReason.Should().Be(PlayoutTerminalReason.PolicyReturnedNull);
    }

    private sealed class MinimalSimGameBuilder : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "sim-edge-board";
            AddTile("t1");
            AddTile("t2").WithRelationTo("t1").InDirection("d").WithDistance(1).Done();
            AddDirection("d");
            AddPlayer("p1");
            AddPiece("piece-1").WithOwner("p1").OnTile("t1");
            AddGamePhase("pass-phase")
                .If<NullGameStateCondition>()
                .Then()
                .ForEvent<TurnPassEvent>()
                    .If(_ => new AlwaysValid<TurnPassEvent>())
                    .Then();
        }

        private sealed class AlwaysValid<TEvent> : IGameEventCondition<TEvent> where TEvent : IGameEvent
        {
            public ConditionResponse Evaluate(GameEngine engine, GameState state, TEvent @event) => ConditionResponse.Valid;
        }
    }
}