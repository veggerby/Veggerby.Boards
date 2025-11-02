using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.TestHelpers;

namespace Veggerby.Boards.Tests.States;

public class TurnAdvanceStateMutatorTests
{
    private static (GameEngine engine, GameState state, TurnArtifact turnArtifact, Player p1, Player p2) CreateBaseline(TurnSegment segment = TurnSegment.Start)
    {
        var p1 = new Player("p1");
        var p2 = new Player("p2");
        var turnArtifact = new TurnArtifact("turn");
        var builder = new TestTurnGameBuilder(p1, p2);
        var progress = builder.Compile();
        var state = GameState.New(new IArtifactState[]
        {
            new TurnState(turnArtifact, 1, segment),
            new ActivePlayerState(p1, true),
            new ActivePlayerState(p2, false)
        });
        return (progress.Engine, state, turnArtifact, p1, p2);
    }

    [Fact]
    public void GivenNonFinalSegment_WhenEndTurnSegmentEvent_ThenAdvancesSegment()
    {
        // arrange

        // act

        // assert

        var (engine, state, turn, p1, p2) = CreateBaseline(TurnSegment.Start);
        var mutator = new TurnAdvanceStateMutator();
        var evt = new EndTurnSegmentEvent(TurnSegment.Start);
        var original = Boards.Internal.FeatureFlags.EnableTurnSequencing;
        Boards.Internal.FeatureFlags.EnableTurnSequencing = true;

        // act
        var updated = mutator.MutateState(engine, state, evt);
        Boards.Internal.FeatureFlags.EnableTurnSequencing = original;

        // assert
        var ts = updated.GetStates<TurnState>().Single();
        ts.TurnNumber.Should().Be(1);
        ts.Segment.Should().Be(TurnSegment.Main);
    }

    [Fact]
    public void GivenFinalSegment_WhenEndTurnSegmentEvent_ThenIncrementsTurnAndRotatesActivePlayer()
    {
        // arrange

        // act

        // assert

        var (engine, state, turn, p1, p2) = CreateBaseline(TurnSegment.End);
        var mutator = new TurnAdvanceStateMutator();
        var evt = new EndTurnSegmentEvent(TurnSegment.End);
        var original = Boards.Internal.FeatureFlags.EnableTurnSequencing;
        Boards.Internal.FeatureFlags.EnableTurnSequencing = true;

        // act
        var updated = mutator.MutateState(engine, state, evt);
        Boards.Internal.FeatureFlags.EnableTurnSequencing = original;

        // assert
        var ts = updated.GetStates<TurnState>().Single();
        ts.TurnNumber.Should().Be(2);
        ts.Segment.Should().Be(TurnSegment.Start);
        var activePlayers = updated.GetStates<ActivePlayerState>();
        activePlayers.Single(x => x.IsActive).Artifact.Should().Be(p2);
    }

    [Fact]
    public void GivenSegmentMismatch_WhenEndTurnSegmentEvent_ThenNoChange()
    {
        // arrange

        // act

        // assert

        var (engine, state, turn, p1, p2) = CreateBaseline(TurnSegment.Main);
        var mutator = new TurnAdvanceStateMutator();
        var evt = new EndTurnSegmentEvent(TurnSegment.Start); // mismatch
        var original = Boards.Internal.FeatureFlags.EnableTurnSequencing;
        Boards.Internal.FeatureFlags.EnableTurnSequencing = true;

        // act
        var updated = mutator.MutateState(engine, state, evt);
        Boards.Internal.FeatureFlags.EnableTurnSequencing = original;

        // assert
        updated.Should().BeSameAs(state);
    }

    [Fact]
    public void GivenDisabledFeatureFlag_WhenEndTurnSegmentEvent_ThenNoChange()
    {
        // arrange

        // act

        // assert

        var (engine, state, turn, p1, p2) = CreateBaseline(TurnSegment.End);
        var mutator = new TurnAdvanceStateMutator();
        var evt = new EndTurnSegmentEvent(TurnSegment.End);
        var original = Boards.Internal.FeatureFlags.EnableTurnSequencing;
        Boards.Internal.FeatureFlags.EnableTurnSequencing = false;
        try
        {
            // act
            var updated = mutator.MutateState(engine, state, evt);
            // assert
            updated.Should().BeSameAs(state);
        }
        finally
        {
            Boards.Internal.FeatureFlags.EnableTurnSequencing = original;
        }
    }

    [Fact]
    public void GivenNoActivePlayerProjection_WhenFinalSegment_ThenOnlyTurnStateAdvances()
    {
        // arrange

        // act

        // assert

        var p1 = new Player("p1");
        var turnArtifact = new TurnArtifact("turn");
        var builder = new TestTurnGameBuilder(p1);
        var progress = builder.Compile();
        var engine = progress.Engine;
        var state = GameState.New(new IArtifactState[] { new TurnState(turnArtifact, 1, TurnSegment.End) });
        var mutator = new TurnAdvanceStateMutator();
        var evt = new EndTurnSegmentEvent(TurnSegment.End);
        var original = Boards.Internal.FeatureFlags.EnableTurnSequencing;
        Boards.Internal.FeatureFlags.EnableTurnSequencing = true;

        // act
        var updated = mutator.MutateState(engine, state, evt);
        Boards.Internal.FeatureFlags.EnableTurnSequencing = original;

        // assert
        var ts = updated.GetStates<TurnState>().Single();
        ts.TurnNumber.Should().Be(2);
        ts.Segment.Should().Be(TurnSegment.Start);
        updated.GetStates<ActivePlayerState>().Should().BeEmpty();
    }
}
