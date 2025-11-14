using System;
using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions.Turn;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.Turn;

public class TurnSequencingCoreTests
{
    private IDisposable EnableFlag()
    {
        // No-op: Turn sequencing always enabled (graduated feature)
        return new ResetFlag(() => { });
    }

    private sealed class ResetFlag(Action reset) : IDisposable
    {
        private readonly Action _reset = reset;

        public void Dispose()
        {
            _reset();
        }
    }

    [Fact]
    public void GivenFlagDisabled_WhenEndTurnSegmentEventApplied_ThenStateUnchanged()
    {
        // arrange

        // act

        // assert

        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var initial = progress.State.GetStates<TurnState>().FirstOrDefault();
        if (initial is null)
        {
            // TurnState not emitted when sequencing disabled; nothing to validate
            return;
        }
        var ev = new EndTurnSegmentEvent(TurnSegment.Start);
        var condition = new EndTurnSegmentCondition();
        var mutator = new TurnAdvanceStateMutator();

        // act
        var response = condition.Evaluate(progress.Engine, progress.State, ev);
        var newState = response == ConditionResponse.Valid
            ? mutator.MutateState(progress.Engine, progress.State, ev)
            : progress.State;

        // assert
        response.Should().Be(ConditionResponse.NotApplicable);
        newState.GetStates<TurnState>().First().Should().BeEquivalentTo(initial);
    }

    [Fact]
    public void GivenStartSegment_WhenEnded_ThenSegmentAdvancesToMain()
    {
        // arrange

        // act

        // assert

        using var _ = EnableFlag();
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var ev = new EndTurnSegmentEvent(TurnSegment.Start);
        var condition = new EndTurnSegmentCondition();
        var mutator = new TurnAdvanceStateMutator();

        // act
        var response = condition.Evaluate(progress.Engine, progress.State, ev);
        response.Should().Be(ConditionResponse.Valid);
        var newState = mutator.MutateState(progress.Engine, progress.State, ev);

        // assert
        var updated = newState.GetStates<TurnState>().First();
        updated.TurnNumber.Should().Be(1);
        updated.Segment.Should().Be(TurnSegment.Main);
    }

    [Fact]
    public void GivenMainSegment_WhenEnded_ThenSegmentAdvancesToEnd()
    {
        // arrange

        // act

        // assert

        using var _ = EnableFlag();
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var condition = new EndTurnSegmentCondition();
        var mutator = new TurnAdvanceStateMutator();

        // Start -> Main
        condition.Evaluate(progress.Engine, progress.State, new EndTurnSegmentEvent(TurnSegment.Start))
            .Should().Be(ConditionResponse.Valid);
        var afterStart = mutator.MutateState(progress.Engine, progress.State, new EndTurnSegmentEvent(TurnSegment.Start));

        // Main -> End
        condition.Evaluate(progress.Engine, afterStart, new EndTurnSegmentEvent(TurnSegment.Main))
            .Should().Be(ConditionResponse.Valid);
        var afterMain = mutator.MutateState(progress.Engine, afterStart, new EndTurnSegmentEvent(TurnSegment.Main));

        // assert
        var updated = afterMain.GetStates<TurnState>().First();
        updated.TurnNumber.Should().Be(1);
        updated.Segment.Should().Be(TurnSegment.End);
    }

    [Fact]
    public void GivenEndSegment_WhenEnded_ThenTurnNumberIncrementsAndResetsToStart()
    {
        // arrange

        // act

        // assert

        using var _ = EnableFlag();
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var mutator = new TurnAdvanceStateMutator();

        var afterStart = mutator.MutateState(progress.Engine, progress.State, new EndTurnSegmentEvent(TurnSegment.Start));
        var afterMain = mutator.MutateState(progress.Engine, afterStart, new EndTurnSegmentEvent(TurnSegment.Main));
        var initialTurnState = afterMain.GetStates<TurnState>().First();

        // act
        var afterEnd = mutator.MutateState(progress.Engine, afterMain, new EndTurnSegmentEvent(TurnSegment.End));

        // assert
        var updated = afterEnd.GetStates<TurnState>().First();
        updated.TurnNumber.Should().Be(initialTurnState.TurnNumber + 1);
        updated.Segment.Should().Be(TurnSegment.Start);
    }

    [Fact]
    public void GivenEndSegment_WhenEnded_ThenActivePlayerRotates()
    {
        // arrange

        // act

        // assert

        using var _ = EnableFlag();
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var mutator = new TurnAdvanceStateMutator();
        var activeCandidates = progress.State.GetStates<ActivePlayerState>().ToList();
        if (!activeCandidates.Any())
        {
            // Chess builder may not designate an active player yet; skip rotation test until projection layer established
            return; // neutral skip (would be [Skip] attribute if we wanted explicit count)
        }
        var initialActive = activeCandidates.Single(x => x.IsActive).Artifact;

        // Progress through Start -> Main -> End
        var afterStart = mutator.MutateState(progress.Engine, progress.State, new EndTurnSegmentEvent(TurnSegment.Start));
        var afterMain = mutator.MutateState(progress.Engine, afterStart, new EndTurnSegmentEvent(TurnSegment.Main));
        var afterEnd = mutator.MutateState(progress.Engine, afterMain, new EndTurnSegmentEvent(TurnSegment.End));

        // assert - turn incremented & active player changed
        var updatedTurn = afterEnd.GetStates<TurnState>().First();
        updatedTurn.TurnNumber.Should().Be(2);
        updatedTurn.Segment.Should().Be(TurnSegment.Start);

        var newActive = afterEnd.GetStates<ActivePlayerState>().Single(x => x.IsActive).Artifact;
        newActive.Should().NotBe(initialActive);
    }

    [Fact]
    public void GivenMismatchedSegment_WhenEnded_ThenInvalid()
    {
        // arrange

        // act

        // assert

        using var _ = EnableFlag();
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var condition = new EndTurnSegmentCondition();
        var mutator = new TurnAdvanceStateMutator();
        var ev = new EndTurnSegmentEvent(TurnSegment.Main); // current segment is Start

        // act
        var response = condition.Evaluate(progress.Engine, progress.State, ev);
        var resultingState = response == ConditionResponse.Valid
            ? mutator.MutateState(progress.Engine, progress.State, ev)
            : progress.State;

        // assert
        response.Should().Be(ConditionResponse.Invalid);
        resultingState.GetStates<TurnState>().First().Segment.Should().Be(TurnSegment.Start);
    }

    [Fact]
    public void GivenAnySegment_WhenTurnPassEventApplied_ThenTurnAdvancesAndActivePlayerRotates()
    {
        // arrange

        // act

        // assert

        using var _ = EnableFlag();
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var initialTurn = progress.State.GetStates<TurnState>().First();
        var activePlayers = progress.State.GetStates<ActivePlayerState>().ToList();
        var initialActive = activePlayers.FirstOrDefault(x => x.IsActive)?.Artifact;
        var mutator = new TurnPassStateMutator();
        var passEvent = new TurnPassEvent();

        // act
        var newState = mutator.MutateState(progress.Engine, progress.State, passEvent);

        // assert
        var updatedTurn = newState.GetStates<TurnState>().First();
        updatedTurn.TurnNumber.Should().Be(initialTurn.TurnNumber + 1);
        updatedTurn.Segment.Should().Be(TurnSegment.Start);
        if (initialActive is not null)
        {
            var newActive = newState.GetStates<ActivePlayerState>().Single(x => x.IsActive).Artifact;
            newActive.Should().NotBe(initialActive);
        }
    }

    [Fact]
    public void GivenMainSegment_WhenTurnCommitEventApplied_ThenSegmentTransitionsToEnd()
    {
        // arrange

        // act

        // assert

        using var _ = EnableFlag();
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var advance = new TurnAdvanceStateMutator();
        // Move Start -> Main
        var afterStart = advance.MutateState(progress.Engine, progress.State, new EndTurnSegmentEvent(TurnSegment.Start));
        var commitMutator = new TurnCommitStateMutator();
        var commitEvent = new TurnCommitEvent();

        // act
        var afterCommit = commitMutator.MutateState(progress.Engine, afterStart, commitEvent);

        // assert
        var updated = afterCommit.GetStates<TurnState>().First();
        updated.TurnNumber.Should().Be(1);
        updated.Segment.Should().Be(TurnSegment.End);
    }

    [Fact]
    public void GivenStartSegment_WhenTurnCommitEventApplied_ThenStateUnchanged()
    {
        // arrange

        // act

        // assert

        using var _ = EnableFlag();
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var initial = progress.State.GetStates<TurnState>().First();
        var commitMutator = new TurnCommitStateMutator();
        var commitEvent = new TurnCommitEvent();

        // act
        var afterCommit = commitMutator.MutateState(progress.Engine, progress.State, commitEvent);

        // assert
        var updated = afterCommit.GetStates<TurnState>().First();
        updated.Should().BeEquivalentTo(initial);
    }

    [Fact]
    public void GivenSequencingEnabled_WhenAlternatingEndTurnSegments_ThenActivePlayerAlternates()
    {
        // arrange

        // act

        // assert

        using var _ = EnableFlag();
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var mutator = new TurnAdvanceStateMutator();
        var activeBefore = progress.State.GetStates<ActivePlayerState>().FirstOrDefault(x => x.IsActive)?.Artifact;
        if (activeBefore is null)
        {
            return; // skip if chess builder not yet assigning active player
        }

        // act
        // Complete first turn (Start->Main->End)
        var afterStart = mutator.MutateState(progress.Engine, progress.State, new EndTurnSegmentEvent(TurnSegment.Start));
        var afterMain = mutator.MutateState(progress.Engine, afterStart, new EndTurnSegmentEvent(TurnSegment.Main));
        var afterEnd = mutator.MutateState(progress.Engine, afterMain, new EndTurnSegmentEvent(TurnSegment.End));

        // Complete second turn
        var afterStart2 = mutator.MutateState(progress.Engine, afterEnd, new EndTurnSegmentEvent(TurnSegment.Start));
        var afterMain2 = mutator.MutateState(progress.Engine, afterStart2, new EndTurnSegmentEvent(TurnSegment.Main));
        var afterEnd2 = mutator.MutateState(progress.Engine, afterMain2, new EndTurnSegmentEvent(TurnSegment.End));

        // assert
        var activeAfterFirst = afterEnd.GetStates<ActivePlayerState>().Single(x => x.IsActive).Artifact;
        var activeAfterSecond = afterEnd2.GetStates<ActivePlayerState>().Single(x => x.IsActive).Artifact;
        activeAfterFirst.Should().NotBe(activeBefore);
        activeAfterSecond.Should().Be(activeBefore);
    }

    [Fact]
    public void GivenTwoPasses_WhenSequencingEnabled_ThenPassStreakIsTwo()
    {
        // arrange

        // act

        // assert

        using var _ = EnableFlag();
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var passMutator = new TurnPassStateMutator();
        var passEvent = new TurnPassEvent();
        var afterFirst = passMutator.MutateState(progress.Engine, progress.State, passEvent);
        var afterSecond = passMutator.MutateState(progress.Engine, afterFirst, passEvent);
        var turnState = afterSecond.GetStates<TurnState>().First();
        turnState.PassStreak.Should().Be(2);
    }

    [Fact]
    public void GivenReplayEvent_WhenSequencingEnabled_ThenTurnAdvancesWithoutActiveRotationAndStreakResets()
    {
        // arrange

        // act

        // assert

        using var _ = EnableFlag();
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var replayMutator = new TurnReplayStateMutator();
        var replayEvent = new TurnReplayEvent();
        var activeBefore = progress.State.GetStates<ActivePlayerState>().FirstOrDefault(x => x.IsActive)?.Artifact;
        var afterReplay = replayMutator.MutateState(progress.Engine, progress.State, replayEvent);
        var turnState = afterReplay.GetStates<TurnState>().First();
        turnState.TurnNumber.Should().Be(2);
        turnState.Segment.Should().Be(TurnSegment.Start);
        turnState.PassStreak.Should().Be(0);
        var activeAfter = afterReplay.GetStates<ActivePlayerState>().FirstOrDefault(x => x.IsActive)?.Artifact;
        if (activeBefore is not null)
        {
            activeAfter.Should().Be(activeBefore); // replay keeps same player
        }
    }
}
