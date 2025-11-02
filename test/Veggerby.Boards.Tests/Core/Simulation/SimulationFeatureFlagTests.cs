using System;
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
/// Guard tests validating that simulation APIs respect FeatureFlags.EnableSimulation toggle and remain deterministic.
/// </summary>
public class SimulationFeatureFlagTests
{
    private static GameProgress BuildProgress()
    {
        var builder = new MinimalSimGameBuilder();
        return builder.Compile();
    }

    [Fact]
    public void GivenSequentialSimulator_WhenFlagDisabled_ThenThrows()
    {
        // arrange

        // act

        // assert

        FeatureFlags.EnableSimulation = false;
        var progress = BuildProgress();

        // act
        var act = () => SequentialSimulator.Run(progress, _ => new TurnPassEvent());

        // assert
        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("Simulation feature disabled");
    }

    [Fact]
    public void GivenSequentialSimulator_WhenFlagEnabled_ThenRuns()
    {
        // arrange

        // act

        // assert

        FeatureFlags.EnableSimulation = true;
        var progress = BuildProgress();

        // act
        var result = SequentialSimulator.Run(progress, state => new TurnPassEvent());

        // assert
        result.Should().NotBeNull();
        result.State.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenParallelSimulator_WhenFlagDisabled_ThenThrows()
    {
        // arrange

        // act

        // assert

        FeatureFlags.EnableSimulation = false;
        var progress = BuildProgress();

        // act
        var act = async () => await ParallelSimulator.RunManyAsync(progress, 2, _ => (_ => new TurnPassEvent()));

        // assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("Simulation feature disabled");
    }

    [Fact]
    public async Task GivenParallelSimulator_WhenFlagEnabled_ThenRuns()
    {
        // arrange

        // act

        // assert

        FeatureFlags.EnableSimulation = true;
        var progress = BuildProgress();

        // act
        var batch = await ParallelSimulator.RunManyAsync(progress, 3, _ => (_ => new TurnPassEvent()));

        // assert
        batch.Results.Should().HaveCount(3);
    }

    private sealed class MinimalSimGameBuilder : GameBuilder
    {
        protected override void Build()
        {
            BoardId = "sim-board";
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
