using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Monopoly;
using Veggerby.Boards.Monopoly.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.Mutators;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Monopoly;

public class JailMechanicsTests
{
    private static (GameProgress Progress, Player Player) SetupGame()
    {
        var builder = new MonopolyGameBuilder(playerCount: 2, playerNames: ["Alice", "Bob"]);
        var progress = builder.Compile();

        // Initialize player states
        var initialPlayerStates = builder.CreateInitialPlayerStates(progress).ToList();
        progress = progress.NewState(initialPlayerStates);

        var player = progress.Engine.Game.GetPlayer("Alice")!;

        return (progress, player);
    }

    [Fact]
    public void TripleDoublesCondition_WithNoDoubles_ReturnsIgnore()
    {
        // arrange
        var (progress, player) = SetupGame();
        var condition = new TripleDoublesCondition();
        var gameEvent = new MovePlayerGameEvent(player, 3, 4); // Not doubles

        // act
        var result = condition.Evaluate(progress.Engine, progress.State, gameEvent);

        // assert
        result.Result.Should().Be(ConditionResult.Ignore);
    }

    [Fact]
    public void TripleDoublesCondition_WithFirstDoubles_ReturnsIgnore()
    {
        // arrange
        var (progress, player) = SetupGame();
        var condition = new TripleDoublesCondition();
        var gameEvent = new MovePlayerGameEvent(player, 3, 3); // Doubles

        // act
        var result = condition.Evaluate(progress.Engine, progress.State, gameEvent);

        // assert
        result.Result.Should().Be(ConditionResult.Ignore);
    }

    [Fact]
    public void TripleDoublesCondition_WithTwoConsecutiveDoublesAndThirdDoubles_ReturnsValid()
    {
        // arrange
        var (progress, player) = SetupGame();

        // Set up player with 2 consecutive doubles already
        var playerState = progress.State.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player));
        var updatedPlayerState = playerState.WithConsecutiveDoubles(2);
        progress = progress.NewState([updatedPlayerState]);

        var condition = new TripleDoublesCondition();
        var gameEvent = new MovePlayerGameEvent(player, 4, 4); // Third doubles

        // act
        var result = condition.Evaluate(progress.Engine, progress.State, gameEvent);

        // assert
        result.Result.Should().Be(ConditionResult.Valid);
    }

    [Fact]
    public void PlayerInJailCondition_WhenPlayerNotInJail_ReturnsNotApplicable()
    {
        // arrange
        var (progress, player) = SetupGame();
        var condition = new PlayerInJailCondition();
        var gameEvent = new StayInJailGameEvent(player);

        // act
        var result = condition.Evaluate(progress.Engine, progress.State, gameEvent);

        // assert
        // ConditionResponse.NotApplicable has Result = ConditionResult.Ignore
        result.Result.Should().Be(ConditionResult.Ignore);
    }

    [Fact]
    public void PlayerInJailCondition_WhenPlayerInJail_ReturnsValid()
    {
        // arrange
        var (progress, player) = SetupGame();

        // Put player in jail
        var playerState = progress.State.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player));
        var jailedState = playerState.GoToJail();
        progress = progress.NewState([jailedState]);

        var condition = new PlayerInJailCondition();
        var gameEvent = new StayInJailGameEvent(player);

        // act
        var result = condition.Evaluate(progress.Engine, progress.State, gameEvent);

        // assert
        result.Result.Should().Be(ConditionResult.Valid);
    }

    [Fact]
    public void StayInJailStateMutator_IncrementsJailTurns()
    {
        // arrange
        var (progress, player) = SetupGame();

        // Put player in jail
        var playerState = progress.State.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player));
        var jailedState = playerState.GoToJail();
        progress = progress.NewState([jailedState]);

        var mutator = new StayInJailStateMutator();
        var gameEvent = new StayInJailGameEvent(player);

        // act
        var newState = mutator.MutateState(progress.Engine, progress.State, gameEvent);

        // assert
        var updatedPlayerState = newState.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player));
        updatedPlayerState.JailTurns.Should().Be(1);
        updatedPlayerState.InJail.Should().BeTrue();
    }

    [Fact]
    public void StayInJailStateMutator_WhenPlayerNotInJail_ThrowsException()
    {
        // arrange
        var (progress, player) = SetupGame();
        var mutator = new StayInJailStateMutator();
        var gameEvent = new StayInJailGameEvent(player);

        // act
        var act = () => mutator.MutateState(progress.Engine, progress.State, gameEvent);

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not in jail*");
    }

    [Fact]
    public void EndTurnStateMutator_SwitchesToNextPlayer()
    {
        // arrange
        var (progress, _) = SetupGame();
        var mutator = new EndTurnStateMutator();
        var gameEvent = new EndTurnGameEvent();

        // Alice should be active initially
        var initialActive = progress.State.GetStates<ActivePlayerState>()
            .First(aps => aps.IsActive);
        initialActive.Artifact.Id.Should().Be("Alice");

        // act
        var newState = mutator.MutateState(progress.Engine, progress.State, gameEvent);

        // assert
        var newActive = newState.GetStates<ActivePlayerState>()
            .First(aps => aps.IsActive);
        newActive.Artifact.Id.Should().Be("Bob");
    }

    [Fact]
    public void EndTurnStateMutator_WrapsAroundToFirstPlayer()
    {
        // arrange
        var (progress, _) = SetupGame();
        var mutator = new EndTurnStateMutator();
        var gameEvent = new EndTurnGameEvent();

        // Make Bob active
        var alice = progress.Engine.Game.GetPlayer("Alice")!;
        var bob = progress.Engine.Game.GetPlayer("Bob")!;
        progress = progress.NewState([
            new ActivePlayerState(alice, false),
            new ActivePlayerState(bob, true)
        ]);

        // act
        var newState = mutator.MutateState(progress.Engine, progress.State, gameEvent);

        // assert
        var newActive = newState.GetStates<ActivePlayerState>()
            .First(aps => aps.IsActive);
        newActive.Artifact.Id.Should().Be("Alice"); // Wrapped around
    }

    [Fact]
    public void EndTurnStateMutator_WhenNoActivePlayer_ReturnsUnchangedState()
    {
        // arrange
        var (progress, _) = SetupGame();
        var mutator = new EndTurnStateMutator();
        var gameEvent = new EndTurnGameEvent();

        // Make no one active
        var alice = progress.Engine.Game.GetPlayer("Alice")!;
        var bob = progress.Engine.Game.GetPlayer("Bob")!;
        progress = progress.NewState([
            new ActivePlayerState(alice, false),
            new ActivePlayerState(bob, false)
        ]);

        // act
        var newState = mutator.MutateState(progress.Engine, progress.State, gameEvent);

        // assert
        // State should be unchanged since no active player was found
        newState.Should().BeSameAs(progress.State);
    }
}
