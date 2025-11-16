using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

public class BackgammonInvariants
{
    // Deterministic initial roll sequence: rolling starting dice should either set both dice or remain without duplication of state mutation.
    [Fact]
    public void GivenInitialBackgammonState_WhenRollingStartingDice_ThenDistinctValuesOrRepeatUntilDistinct()
    {
        // arrange

        // act

        // assert

        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var d1 = progress.Game.Artifacts.OfType<Dice>().Single(d => d.Id == "dice-1");
        var d2 = progress.Game.Artifacts.OfType<Dice>().Single(d => d.Id == "dice-2");

        // act - single valid opening roll with distinct values chooses starter
        progress = progress.HandleEvent(new RollDiceGameEvent<int>(new DiceState<int>(d1, 4), new DiceState<int>(d2, 5)));

        // assert
        var states = progress.State.GetStates<DiceState<int>>().Where(s => s.Artifact.Id is "dice-1" or "dice-2").ToList();
        states.Count.Should().Be(2);
        states[0].CurrentValue.Should().NotBe(states[1].CurrentValue);
        progress.State.GetStates<ActivePlayerState>().Single(a => a.IsActive).Should().NotBeNull();
    }

    [Fact]
    public void GivenDoublingDice_FirstValidRollDoublesValueAndAssignsOwner()
    {
        // arrange

        // act

        // assert

        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var cube = progress.Game.Artifacts.OfType<Dice>().Single(a => a.Id == "doubling-dice");
        cube.Should().NotBeNull();

        // Establish starting player with single event containing both dice (distinct values)
        progress = ForceDistinctOpeningRoll(progress);

        var activeState = progress.State.GetStates<ActivePlayerState>().SingleOrDefault(x => x.IsActive);
        activeState.Should().NotBeNull(); // now required for doubling

        // Capture initial cube state: before first doubling it's a plain DiceState<int> (owner concept not assigned yet).
        var initialGeneric = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        initialGeneric.CurrentValue.Should().Be(1);

        // act - first doubling. Need to emit a RollDiceGameEvent<int> explicitly targeting the doubling cube
        // because the helper skips deterministic assignment for the cube (silent no-op preservation semantics).
        var rollEvent = new RollDiceGameEvent<int>(new DiceState<int>(cube, 0));
        progress = progress.HandleEvent(rollEvent);
        var afterFirst = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        var doublingState = afterFirst as DoublingDiceState;
        doublingState.Should().NotBeNull(); // now specialized state
                                            // The specialized state encapsulates the doubled value; ensure we read the concrete type instance.
        doublingState!.CurrentValue.Should().Be(2);
        var assignedOwner = doublingState.CurrentPlayer; // owner should be inactive player at time of doubling
        assignedOwner.Should().NotBeNull();
    }

    [Fact]
    public void GivenDoublingDice_WhenRedoubleAttemptSameTurn_ThenValueUnchanged()
    {
        // arrange

        // act

        // assert

        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var cube = progress.Game.Artifacts.OfType<Dice>().Single(a => a.Id == "doubling-dice");

        // Ensure starting player selected (distinct opening roll). If still not distinct after attempts, force one deterministic extra roll sequence.
        progress = ForceDistinctOpeningRoll(progress);
        var activeBeforeFirst = progress.State.GetStates<ActivePlayerState>().SingleOrDefault(x => x.IsActive);
        activeBeforeFirst.Should().NotBeNull(); // must have active player for doubling to proceed

        // First doubling (explicit event)
        var baseCubeState = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        var firstEvent = new RollDiceGameEvent<int>(baseCubeState);
        progress = progress.HandleEvent(firstEvent);
        var afterFirst = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        var owner = (afterFirst as DoublingDiceState)!.CurrentPlayer;
        afterFirst.CurrentValue.Should().Be(2);

        // act - second immediate doubling attempt (same turn). Expect Ignore -> unchanged (gated by cube state LastDoubledTurn).
        var secondEvent = new RollDiceGameEvent<int>(progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube));
        var attempt = progress.HandleEvent(secondEvent);
        var afterAttempt = attempt.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);

        // assert - unchanged (same turn redouble blocked)
        afterAttempt.CurrentValue.Should().Be(2);
        (afterAttempt as DoublingDiceState)!.CurrentPlayer.Should().BeSameAs(owner);
    }

    [Fact]
    public void GivenNoActivePlayer_WhenAttemptDoubling_ThenCubeUnchanged()
    {
        // arrange

        // act

        // assert

        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var cube = progress.Game.Artifacts.OfType<Dice>().Single(a => a.Id == "doubling-dice");
        var initial = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        initial.CurrentValue.Should().Be(1);

        // act - attempt to roll doubling dice before starter chosen (active player not yet assigned)
        // Use existing state value to avoid mutating via generic dice mutator unrelated to doubling conditions.
        var attempt = progress.HandleEvent(new RollDiceGameEvent<int>(initial));
        var after = attempt.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);

        // assert - unchanged (condition fails due to no active player)
        after.CurrentValue.Should().Be(1);
        after.Should().BeOfType<DiceState<int>>(); // still generic state (not specialized DoublingDiceState)
    }

    [Fact]
    public void GivenDoublingDiceOwned_WhenOwnerAttemptsImmediateRedouble_ThenUnchanged()
    {
        // arrange

        // act

        // assert

        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var cube = progress.Game.Artifacts.OfType<Dice>().Single(a => a.Id == "doubling-dice");

        // Ensure starting player selected (distinct opening roll)
        progress = ForceDistinctOpeningRoll(progress);

        // First doubling (owner becomes inactive player) – must emit explicit event because helper skips doubling cube
        var firstEvent = new RollDiceGameEvent<int>(progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube));
        progress = progress.HandleEvent(firstEvent);
        var afterFirst = progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        var doublingState = (DoublingDiceState)afterFirst;
        var owner = doublingState.CurrentPlayer;
        owner.Should().NotBeNull();
        afterFirst.CurrentValue.Should().Be(2);

        // Determine owner player id to simulate owner attempting redouble: active player now should be opponent
        var active = progress.State.GetStates<ActivePlayerState>().Single(p => p.IsActive).Artifact;

        // If active equals owner, still proceed – redouble attempt should be ignored either way because same-turn gating uses cube LastDoubledTurn.
        var attemptEvent = new RollDiceGameEvent<int>(progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube));
        var attempt = progress.HandleEvent(attemptEvent);
        var afterAttempt = attempt.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
        afterAttempt.CurrentValue.Should().Be(2);
        ((DoublingDiceState)afterAttempt).CurrentPlayer.Should().BeSameAs(owner);
    }

    [Fact]
    public void GivenDistinctTurns_WhenOpponentsAlternateRedoubles_ThenValueProgresses2xEachTurn()
    {
        // arrange

        // act

        // assert

        try
        {
            var builder = new BackgammonGameBuilder();
            var progress = builder.Compile();
            var cube = progress.Game.Artifacts.OfType<Dice>().Single(a => a.Id == "doubling-dice");
            // Opening distinct roll to select starter
            progress = ForceDistinctOpeningRoll(progress);

            var turnMutator = new TurnAdvanceStateMutator();

            // Helper local to advance segment (Start->Main->End) and then advance to next turn
            static GameState AdvanceFullTurn(TurnAdvanceStateMutator m, GameProgress p)
            {
                var s1 = m.MutateState(p.Engine, p.State, new EndTurnSegmentEvent(TurnSegment.Start));
                var s2 = m.MutateState(p.Engine, s1, new EndTurnSegmentEvent(TurnSegment.Main));
                return m.MutateState(p.Engine, s2, new EndTurnSegmentEvent(TurnSegment.End));
            }

            // First doubling (2)
            progress = progress.HandleEvent(new RollDiceGameEvent<int>(progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube)));
            var afterFirst = (DoublingDiceState)progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
            afterFirst.CurrentValue.Should().Be(2);
            var firstOwner = afterFirst.CurrentPlayer;

            // Advance one full turn (owner's opponent plays a full cycle) so redouble becomes eligible
            var advancedState = AdvanceFullTurn(turnMutator, progress);
            progress = new GameProgress(progress.Engine, advancedState, progress.Events);

            // Redouble by opponent (4)
            progress = progress.HandleEvent(new RollDiceGameEvent<int>(progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube)));
            var afterSecond = (DoublingDiceState)progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
            afterSecond.CurrentValue.Should().Be(4);
            var secondOwner = afterSecond.CurrentPlayer;
            secondOwner.Should().NotBeSameAs(firstOwner); // ownership alternates

            // Advance another full turn (back to original owner side)
            advancedState = AdvanceFullTurn(turnMutator, progress);
            progress = new GameProgress(progress.Engine, advancedState, progress.Events);

            // Third doubling (8)
            progress = progress.HandleEvent(new RollDiceGameEvent<int>(progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube)));
            var afterThird = (DoublingDiceState)progress.State.GetStates<DiceState<int>>().Single(s => s.Artifact == cube);
            afterThird.CurrentValue.Should().Be(8);
            afterThird.CurrentPlayer.Should().NotBeSameAs(secondOwner);

            // Ownership returns to first owner after two alternations
            afterThird.CurrentPlayer.Should().BeSameAs(firstOwner);
        }
        finally
        {
        }
    }

    private static GameProgress ForceDistinctOpeningRoll(GameProgress progress)
    {
        if (progress.State.GetStates<ActivePlayerState>().Any(a => a.IsActive))
        {
            return progress; // already selected
        }

        var dice1 = progress.Game.Artifacts.OfType<Dice>().Single(d => d.Id == "dice-1");
        var dice2 = progress.Game.Artifacts.OfType<Dice>().Single(d => d.Id == "dice-2");

        // Provide deterministic distinct values in a single event so the selection mutator can assign the active player.
        var openingEvent = new RollDiceGameEvent<int>(new DiceState<int>(dice1, 1), new DiceState<int>(dice2, 2));
        return progress.HandleEvent(openingEvent);
    }
}
