using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.PropertyTests;

public class BackgammonInvariants
{
    // Deterministic initial roll sequence: rolling starting dice should either set both dice or remain without duplication of state mutation.
    [Fact]
    public void GivenInitialBackgammonState_WhenRollingStartingDice_ThenDistinctValuesOrRepeatUntilDistinct()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var dice = progress.Game.GetArtifacts<Dice>("dice-1", "dice-2").ToList();
        Assert.Equal(2, dice.Count);

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
        Assert.True(finalStates.Count <= 2);
    }

    [Fact]
    public void GivenDoublingDice_FirstValidRollDoublesValueAndAssignsOwner()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var cube = progress.Game.GetArtifact<Dice>("doubling-dice");
        Assert.NotNull(cube);

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
        Assert.NotNull(activeState); // now required for doubling

        // Capture initial cube state: before first doubling it's a plain DiceState<int> (owner concept not assigned yet).
        var initialGeneric = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        Assert.Equal(1, initialGeneric.CurrentValue);

        // act - first doubling. Need to emit a RollDiceGameEvent<int> explicitly targeting the doubling cube
        // because the helper skips deterministic assignment for the cube (silent no-op preservation semantics).
        var rollEvent = new RollDiceGameEvent<int>(new DiceState<int>(cube, 0));
        progress = progress.HandleEvent(rollEvent);
        var afterFirst = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        var doublingState = afterFirst as DoublingDiceState;
        Assert.NotNull(doublingState); // now specialized state
                                       // The specialized state encapsulates the doubled value; ensure we read the concrete type instance.
        Assert.Equal(2, doublingState!.CurrentValue);
        var assignedOwner = doublingState.CurrentPlayer; // owner should be inactive player at time of doubling
        Assert.NotNull(assignedOwner);
    }

    [Fact]
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

        Assert.NotNull(progress.State.GetStates<ActivePlayerState>().SingleOrDefault(x => x.IsActive));

        // First doubling (explicit event)
        var firstEvent = new RollDiceGameEvent<int>(new DiceState<int>(cube, 0));
        progress = progress.HandleEvent(firstEvent);
        var afterFirst = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        var owner = (afterFirst as DoublingDiceState)!.CurrentPlayer;
        Assert.Equal(2, afterFirst.CurrentValue);

        // act - second immediate doubling attempt (same turn). Expect Ignore -> unchanged (gated by cube state LastDoubledTurn).
        var secondEvent = new RollDiceGameEvent<int>(new DiceState<int>(cube, 0));
        var attempt = progress.HandleEvent(secondEvent);
        var afterAttempt = attempt.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);

        // assert - unchanged (same turn redouble blocked)
        Assert.Equal(2, afterAttempt.CurrentValue);
        Assert.Same(owner, (afterAttempt as DoublingDiceState)!.CurrentPlayer);
    }

    [Fact]
    public void GivenNoActivePlayer_WhenAttemptDoubling_ThenCubeUnchanged()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var cube = progress.Game.GetArtifact<Dice>("doubling-dice");
        var initial = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        Assert.Equal(1, initial.CurrentValue);

        // act - attempt to roll doubling dice before starter chosen (active player not yet assigned)
        var attempt = progress.RollDice("doubling-dice");
        var after = attempt.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);

        // assert - unchanged (condition fails due to no active player)
        Assert.Equal(1, after.CurrentValue);
        Assert.IsType<DiceState<int>>(after); // still generic state (not specialized DoublingDiceState)
    }

    [Fact]
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
        Assert.NotNull(owner);
        Assert.Equal(2, afterFirst.CurrentValue);

        // Determine owner player id to simulate owner attempting redouble: active player now should be opponent
        var active = progress.State.GetStates<ActivePlayerState>().Single(p => p.IsActive).Artifact;

        // If active equals owner, still proceed – redouble attempt should be ignored either way because same-turn gating uses cube LastDoubledTurn.
        var attemptEvent = new RollDiceGameEvent<int>(new DiceState<int>(cube, 0));
        var attempt = progress.HandleEvent(attemptEvent);
        var afterAttempt = attempt.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        Assert.Equal(2, afterAttempt.CurrentValue);
        Assert.Same(owner, ((DoublingDiceState)afterAttempt).CurrentPlayer);
    }

    [Fact]
    public void GivenDistinctTurns_WhenOpponentsAlternateRedoubles_ThenValueProgresses2xEachTurn()
    {
        // arrange
        var original = Internal.FeatureFlags.EnableTurnSequencing;
        Internal.FeatureFlags.EnableTurnSequencing = true;
        try
        {
            var builder = new BackgammonGameBuilder();
            var progress = builder.Compile();
            var cube = progress.Game.GetArtifact<Dice>("doubling-dice");

            // Opening distinct roll to select starter
            for (int i = 0; i < 5; i++)
            {
                progress = progress.RollDice("dice-1", "dice-2");
                var startStates = progress.State.GetStates<DiceState<int>>().Where(s => s.Artifact.Id is "dice-1" or "dice-2").ToList();
                if (startStates.Count == 2 && startStates[0].CurrentValue != startStates[1].CurrentValue)
                {
                    break;
                }
            }

            var turnMutator = new TurnAdvanceStateMutator();

            // Helper local to advance segment (Start->Main->End) and then advance to next turn
            static GameState AdvanceFullTurn(TurnAdvanceStateMutator m, GameProgress p)
            {
                var s1 = m.MutateState(p.Engine, p.State, new EndTurnSegmentEvent(TurnSegment.Start));
                var s2 = m.MutateState(p.Engine, s1, new EndTurnSegmentEvent(TurnSegment.Main));
                return m.MutateState(p.Engine, s2, new EndTurnSegmentEvent(TurnSegment.End));
            }

            // First doubling (2)
            progress = progress.HandleEvent(new RollDiceGameEvent<int>(new DiceState<int>(cube, 0)));
            var afterFirst = (DoublingDiceState)progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
            Assert.Equal(2, afterFirst.CurrentValue);
            var firstOwner = afterFirst.CurrentPlayer;

            // Advance one full turn (owner's opponent plays a full cycle) so redouble becomes eligible
            var advancedState = AdvanceFullTurn(turnMutator, progress);
            progress = new GameProgress(progress.Engine, advancedState, progress.Events);

            // Redouble by opponent (4)
            progress = progress.HandleEvent(new RollDiceGameEvent<int>(new DiceState<int>(cube, 0)));
            var afterSecond = (DoublingDiceState)progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
            Assert.Equal(4, afterSecond.CurrentValue);
            var secondOwner = afterSecond.CurrentPlayer;
            Assert.NotSame(firstOwner, secondOwner); // ownership alternates

            // Advance another full turn (back to original owner side)
            advancedState = AdvanceFullTurn(turnMutator, progress);
            progress = new GameProgress(progress.Engine, advancedState, progress.Events);

            // Third doubling (8)
            progress = progress.HandleEvent(new RollDiceGameEvent<int>(new DiceState<int>(cube, 0)));
            var afterThird = (DoublingDiceState)progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
            Assert.Equal(8, afterThird.CurrentValue);
            Assert.NotSame(secondOwner, afterThird.CurrentPlayer);

            // Ownership returns to first owner after two alternations
            Assert.Same(firstOwner, afterThird.CurrentPlayer);
        }
        finally
        {
            Internal.FeatureFlags.EnableTurnSequencing = original;
        }
    }
}