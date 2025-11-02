using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.Flows.Mutators;

public class NextPlayerStateMutatorTests
{
    [Fact]
    public void GivenSingleActivePlayer_WhenMutated_ThenAdvancesToNextPlayer()
    {
        // arrange

        // act

        // assert

        var progress = new TestGameBuilder().Compile();
        var p1 = progress.Game.Players.First();
        var p2 = progress.Game.Players.Skip(1).First();

        var initial = progress.State.Next([
            new ActivePlayerState(p1, true),
            new ActivePlayerState(p2, false)
        ]);

        var mutator = new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition());
        var dice = progress.Game.GetArtifacts<Dice>().First();
        var evt = new RollDiceGameEvent<int>(new DiceState<int>(dice, 0));

        // act
        var updated = mutator.MutateState(progress.Engine, initial, evt);

        // assert
        updated.Should().NotBeSameAs(initial);
        var active = updated.GetStates<ActivePlayerState>().Single(x => x.IsActive);
        active.Artifact.Should().Be(p2);
        var inactive = updated.GetStates<ActivePlayerState>().Single(x => !x.IsActive);
        inactive.Artifact.Should().Be(p1);
    }

    [Fact]
    public void GivenNoActivePlayer_WhenConditionInvalid_ThenStateUnchanged()
    {
        // arrange

        // act

        // assert

        var progress = new TestGameBuilder().Compile();
        var p1 = progress.Game.Players.First();
        var p2 = progress.Game.Players.Skip(1).First();

        // No active player projection present -> condition invalid
        var initial = progress.State.Next([
            new ActivePlayerState(p1, false),
            new ActivePlayerState(p2, false)
        ]);

        var mutator = new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition());
        var dice = progress.Game.GetArtifacts<Dice>().First();
        var evt = new RollDiceGameEvent<int>(new DiceState<int>(dice, 0));

        // act
        var updated = mutator.MutateState(progress.Engine, initial, evt);

        // assert
        updated.Should().BeSameAs(initial);
    }
}
