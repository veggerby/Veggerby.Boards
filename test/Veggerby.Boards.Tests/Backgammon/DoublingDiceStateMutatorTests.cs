using System;
using System.Linq;

using AwesomeAssertions;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.TestHelpers;

using Xunit;

namespace Veggerby.Boards.Tests.Backgammon;

public class DoublingDiceStateMutatorTests
{
    private static (GameEngine engine, GameState state, Player p1, Player p2, Dice doublingDice) CreateBaseline(int turnNumber = 0, bool withTurnState = true)
    {
        var p1 = new Player("p1");
        var p2 = new Player("p2");
        var doublingDice = new Dice("cube");
        var turnArtifact = new TurnArtifact("turn");
        var builder = new TestTurnGameBuilder(p1, p2);
        var progress = builder.Compile();
        var states = new System.Collections.Generic.List<IArtifactState>();
        if (withTurnState)
        {
            // TurnState is 1-based; shift provided turnNumber (0 meaning first) to 1-based storage
            states.Add(new TurnState(turnArtifact, turnNumber + 1, TurnSegment.Start));
        }
        // active players: p1 active, p2 inactive
        states.Add(new ActivePlayerState(p1, true));
        states.Add(new ActivePlayerState(p2, false));
        // base dice state value 1
        states.Add(new DiceState<int>(doublingDice, 1));
        var state = GameState.New(states);
        return (progress.Engine, state, p1, p2, doublingDice);
    }

    [Fact]
    public void GivenFirstDouble_WhenMutated_ThenSpecializedStateCreatedAndOwnershipAssignedToInactive()
    {
        // arrange
        var (engine, state, p1, p2, cube) = CreateBaseline();
        var mutator = new DoublingDiceStateMutator(cube);
        var evt = new RollDiceGameEvent<int>(new DiceState<int>(cube, 1));

        // act
        var updated = mutator.MutateState(engine, state, evt);

        // assert
        var specialized = updated.GetState<DoublingDiceState>(cube);
        specialized.Should().NotBeNull();
        specialized!.CurrentPlayer.Should().Be(p2); // inactive gets ownership
        specialized.LastDoubledTurn.Should().Be(1); // initial turn now 1
        specialized.CurrentValue.Should().Be(2); // first doubling from 1 -> 2
    }

    [Fact]
    public void GivenAlreadyDoubledSameTurn_WhenMutated_ThenNoChange()
    {
        // arrange
        var (engine, state, p1, p2, cube) = CreateBaseline();
        var firstMutator = new DoublingDiceStateMutator(cube);
        var evt = new RollDiceGameEvent<int>(new DiceState<int>(cube, 1));
        var afterFirst = firstMutator.MutateState(engine, state, evt);
        var specialized = afterFirst.GetState<DoublingDiceState>(cube)!;
        var mutator = new DoublingDiceStateMutator(cube);

        // act
        var afterSecond = mutator.MutateState(engine, afterFirst, evt);

        // assert
        afterSecond.Should().BeSameAs(afterFirst); // blocked same turn
    }

    [Fact]
    public void GivenRedoubleNextTurnByOwner_WhenMutated_ThenValueIncreasesAndOwnershipTransfers()
    {
        // arrange
        var (engine, state, p1, p2, cube) = CreateBaseline();
        var evt = new RollDiceGameEvent<int>(new DiceState<int>(cube, 1));
        var mutator = new DoublingDiceStateMutator(cube);
        var afterFirst = mutator.MutateState(engine, state, evt);
        var specialized = afterFirst.GetState<DoublingDiceState>(cube)!;
        // advance turn: owner is p2 (inactive originally); make p2 active & increment turn state to 1 so redouble allowed
        var turn = afterFirst.GetStates<TurnState>().Single();
        var advancedTurn = new TurnState(turn.Artifact, 2, TurnSegment.Start); // advance from 1 -> 2
        var newStates = afterFirst.ChildStates.Where(s => s is not TurnState && s is not ActivePlayerState).ToList();
        newStates.Add(advancedTurn);
        newStates.Add(new ActivePlayerState(p2, true));
        newStates.Add(new ActivePlayerState(p1, false));
        var progressed = afterFirst.Next(newStates);

        // act
        var afterRedouble = mutator.MutateState(engine, progressed, evt);

        // assert
        afterRedouble.Should().NotBeSameAs(progressed);
        var redoubled = afterRedouble.GetState<DoublingDiceState>(cube)!;
        redoubled.CurrentValue.Should().Be(specialized.CurrentValue * 2);
        redoubled.CurrentPlayer.Should().Be(p1); // ownership flips back
        redoubled.LastDoubledTurn.Should().Be(2);
    }

    [Fact]
    public void GivenRedoubleAttemptByNonOwner_WhenMutated_ThenNoChange()
    {
        // arrange
        var (engine, state, p1, p2, cube) = CreateBaseline();
        var evt = new RollDiceGameEvent<int>(new DiceState<int>(cube, 1));
        var mutator = new DoublingDiceStateMutator(cube);
        var afterFirst = mutator.MutateState(engine, state, evt);
        // switch active player to owner (p2) to simulate proper ownership, then attempt redouble with p1 active incorrectly
        var turn = afterFirst.GetStates<TurnState>().Single();
        var advancedTurn = new TurnState(turn.Artifact, 2, TurnSegment.Start);
        var newStates = afterFirst.ChildStates.Where(s => s is not TurnState && s is not ActivePlayerState).ToList();
        newStates.Add(advancedTurn);
        newStates.Add(new ActivePlayerState(p1, true)); // incorrect owner active
        newStates.Add(new ActivePlayerState(p2, false));
        var tampered = afterFirst.Next(newStates);

        // act
        var after = mutator.MutateState(engine, tampered, evt);

        // assert
        after.Should().BeSameAs(tampered); // blocked
    }

    [Fact]
    public void GivenMissingBaseDiceState_WhenFirstDouble_ThenThrows()
    {
        // arrange
        var p1 = new Player("p1");
        var p2 = new Player("p2");
        var cube = new Dice("cube");
        var turnArtifact = new TurnArtifact("turn");
        var builder = new TestTurnGameBuilder(p1, p2);
        var progress = builder.Compile();
        var state = GameState.New(new IArtifactState[] { new TurnState(turnArtifact, 1, TurnSegment.Start), new ActivePlayerState(p1, true), new ActivePlayerState(p2, false) });
        var mutator = new DoublingDiceStateMutator(cube);
        var evt = new RollDiceGameEvent<int>(new DiceState<int>(cube, 1));

        // act
        Action act = () => mutator.MutateState(progress.Engine, state, evt);

        // assert
        act.Should().Throw<InvalidOperationException>();
    }
}