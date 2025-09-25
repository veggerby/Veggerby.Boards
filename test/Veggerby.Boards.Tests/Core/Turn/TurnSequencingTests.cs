using System;
using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.Flows.Rules.Conditions.Turn;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Core.Turn;

public class TurnSequencingCoreTests
{
    private IDisposable EnableFlag()
    {
        var original = Veggerby.Boards.Internal.FeatureFlags.EnableTurnSequencing;
        Veggerby.Boards.Internal.FeatureFlags.EnableTurnSequencing = true;
        return new ResetFlag(() => Veggerby.Boards.Internal.FeatureFlags.EnableTurnSequencing = original);
    }

    private sealed class ResetFlag(Action reset) : IDisposable
    {
        private readonly Action _reset = reset;

        public void Dispose() { _reset(); }
    }

    [Fact]
    public void GivenFlagDisabled_WhenEndTurnSegmentEventApplied_ThenStateUnchanged()
    {
        // arrange
        Veggerby.Boards.Internal.FeatureFlags.EnableTurnSequencing = false;
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var initial = progress.State.GetStates<TurnState>().First();
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
    public void GivenMismatchedSegment_WhenEnded_ThenInvalid()
    {
        // arrange
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
}