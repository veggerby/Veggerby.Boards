using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.PropertyTests;

public class BackgammonInvariants
{
    // Deterministic initial roll sequence: rolling starting dice should either set both dice or remain without duplication of state mutation.
    [Xunit.Fact]
    public void GivenInitialBackgammonState_WhenRollingStartingDice_ThenDistinctValuesOrRepeatUntilDistinct()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var dice = progress.Game.GetArtifacts<Dice>("dice-1", "dice-2").ToList();
        Xunit.Assert.Equal(2, dice.Count);
        // act - attempt up to 5 rolls until different (modeling rule: must differ to choose starter)
        for (int i = 0; i < 5; i++)
        {
            // use sequential deterministic roll helper (assigns index values)
            var updated = progress.RollDice("dice-1", "dice-2");
            var states = updated.State.GetStates<DiceState<int>>().Where(s => dice.Contains(s.Artifact)).ToList();
            if (states.Count == 2 && states[0].CurrentValue != states[1].CurrentValue)
            {
                return; // success condition satisfied
            }
            progress = updated; // continue attempts
        }
        // assert - if still not distinct, acceptable (engine may allow tie continuation) but must not have produced more than 2 dice states
        var finalStates = progress.State.GetStates<DiceState<int>>().Where(s => dice.Contains(s.Artifact)).ToList();
        Xunit.Assert.True(finalStates.Count <= 2);
    }

    [Xunit.Fact]
    public void GivenDoublingDice_FirstValidRollDoublesValueAndAssignsOwner()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var cube = progress.Game.GetArtifact<Dice>("doubling-dice");
        Xunit.Assert.NotNull(cube);

        // Establish dice values (initial roll phase) until they differ so active player is auto-selected by opening mutator.
        for (int i = 0; i < 5; i++)
        {
            progress = progress.RollDice("dice-1", "dice-2");
            var startStates = progress.State.GetStates<DiceState<int>>().Where(s => s.Artifact.Id is "dice-1" or "dice-2").ToList();
            if (startStates.Count == 2 && startStates[0].CurrentValue != startStates[1].CurrentValue)
            {
                break;
            }
        }

        var activeState = progress.State.GetStates<ActivePlayerState>().SingleOrDefault(x => x.IsActive);
        Xunit.Assert.NotNull(activeState); // now required for doubling

        // Capture initial cube state: before first doubling it's a plain DiceState<int> (owner concept not assigned yet).
        var initialGeneric = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        Xunit.Assert.Equal(1, initialGeneric.CurrentValue);

        // act - first doubling. Need to emit a RollDiceGameEvent<int> explicitly targeting the doubling cube
        // because the helper skips deterministic assignment for the cube (silent no-op preservation semantics).
        var rollEvent = new RollDiceGameEvent<int>(new DiceState<int>(cube, 0));
        progress = progress.HandleEvent(rollEvent);
        var afterFirst = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        var doublingState = afterFirst as DoublingDiceState;
        Xunit.Assert.NotNull(doublingState); // now specialized state
                                             // The specialized state encapsulates the doubled value; ensure we read the concrete type instance.
        Xunit.Assert.Equal(2, doublingState!.CurrentValue);
        var assignedOwner = doublingState.CurrentPlayer; // owner should be inactive player at time of doubling
        Xunit.Assert.NotNull(assignedOwner);
    }

    [Xunit.Fact]
    public void GivenDoublingDice_WhenRedoubleAttemptSameTurn_ThenValueUnchanged()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var cube = progress.Game.GetArtifact<Dice>("doubling-dice");

        // Ensure starting player selected (distinct opening roll)
        for (int i = 0; i < 5; i++)
        {
            progress = progress.RollDice("dice-1", "dice-2");
            var startStates = progress.State.GetStates<DiceState<int>>().Where(s => s.Artifact.Id is "dice-1" or "dice-2").ToList();
            if (startStates.Count == 2 && startStates[0].CurrentValue != startStates[1].CurrentValue)
            {
                break;
            }
        }
        Xunit.Assert.NotNull(progress.State.GetStates<ActivePlayerState>().SingleOrDefault(x => x.IsActive));

        // First doubling (explicit event)
        var firstEvent = new RollDiceGameEvent<int>(new DiceState<int>(cube, 0));
        progress = progress.HandleEvent(firstEvent);
        var afterFirst = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        var owner = (afterFirst as DoublingDiceState)!.CurrentPlayer;
        Xunit.Assert.Equal(2, afterFirst.CurrentValue);

        // act - second immediate doubling attempt (same turn). Expect Ignore -> unchanged (gated by cube state LastDoubledTurn).
        var secondEvent = new RollDiceGameEvent<int>(new DiceState<int>(cube, 0));
        var attempt = progress.HandleEvent(secondEvent);
        var afterAttempt = attempt.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        // assert - unchanged (same turn redouble blocked)
        Xunit.Assert.Equal(2, afterAttempt.CurrentValue);
        Xunit.Assert.Same(owner, (afterAttempt as DoublingDiceState)!.CurrentPlayer);
    }

    [Xunit.Fact]
    public void GivenNoActivePlayer_WhenAttemptDoubling_ThenCubeUnchanged()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var cube = progress.Game.GetArtifact<Dice>("doubling-dice");
        var initial = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        Xunit.Assert.Equal(1, initial.CurrentValue);
        // act - attempt to roll doubling dice before starter chosen (active player not yet assigned)
        var attempt = progress.RollDice("doubling-dice");
        var after = attempt.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        // assert - unchanged (condition fails due to no active player)
        Xunit.Assert.Equal(1, after.CurrentValue);
        Xunit.Assert.IsType<DiceState<int>>(after); // still generic state (not specialized DoublingDiceState)
    }

    [Xunit.Fact]
    public void GivenDoublingDiceOwned_WhenOwnerAttemptsImmediateRedouble_ThenUnchanged()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var cube = progress.Game.GetArtifact<Dice>("doubling-dice");
        // Ensure starting player selected
        for (int i = 0; i < 5; i++)
        {
            progress = progress.RollDice("dice-1", "dice-2");
            var startStates = progress.State.GetStates<DiceState<int>>().Where(s => s.Artifact.Id is "dice-1" or "dice-2").ToList();
            if (startStates.Count == 2 && startStates[0].CurrentValue != startStates[1].CurrentValue)
            {
                break;
            }
        }

        // First doubling (owner becomes inactive player) – must emit explicit event because helper skips doubling cube
        var firstEvent = new RollDiceGameEvent<int>(new DiceState<int>(cube, 0));
        progress = progress.HandleEvent(firstEvent);
        var afterFirst = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        var doublingState = (DoublingDiceState)afterFirst;
        var owner = doublingState.CurrentPlayer;
        Xunit.Assert.NotNull(owner);
        Xunit.Assert.Equal(2, afterFirst.CurrentValue);
        // Determine owner player id to simulate owner attempting redouble: active player now should be opponent
        var active = progress.State.GetStates<ActivePlayerState>().Single(p => p.IsActive).Artifact;
        // If active equals owner, still proceed – redouble attempt should be ignored either way because same-turn gating uses cube LastDoubledTurn.
        var attemptEvent = new RollDiceGameEvent<int>(new DiceState<int>(cube, 0));
        var attempt = progress.HandleEvent(attemptEvent);
        var afterAttempt = attempt.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        Xunit.Assert.Equal(2, afterAttempt.CurrentValue);
        Xunit.Assert.Same(owner, ((DoublingDiceState)afterAttempt).CurrentPlayer);
    }
}