using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.Turn;

public class TurnSequencingDeterminismTests
{
    [Fact]
    public void GivenScriptedSequence_WhenRunTwice_ThenFinalTurnStateDeterministic()
    {
        // arrange

        // act

        // assert

        Boards.Internal.FeatureFlags.EnableTurnSequencing = true; // explicit
        var builder1 = new ChessGameBuilder();
        var progress1 = builder1.Compile();
        var builder2 = new ChessGameBuilder();
        var progress2 = builder2.Compile();
        var adv = new TurnAdvanceStateMutator();
        var pass = new TurnPassStateMutator();
        var replay = new TurnReplayStateMutator();

        static GameState ApplyScript(GameProgress p, TurnAdvanceStateMutator adv, TurnPassStateMutator pass, TurnReplayStateMutator replay)
        {
            var s = p.State;
            // Turn 1: Start->Main->End
            s = adv.MutateState(p.Engine, s, new EndTurnSegmentEvent(TurnSegment.Start));
            s = adv.MutateState(p.Engine, s, new EndTurnSegmentEvent(TurnSegment.Main));
            s = adv.MutateState(p.Engine, s, new EndTurnSegmentEvent(TurnSegment.End));
            // Turn 2: pass
            s = pass.MutateState(p.Engine, s, new TurnPassEvent());
            // Turn 3: Start->Main then replay (extra turn without rotation)
            s = adv.MutateState(p.Engine, s, new EndTurnSegmentEvent(TurnSegment.Start));
            s = adv.MutateState(p.Engine, s, new EndTurnSegmentEvent(TurnSegment.Main));
            s = replay.MutateState(p.Engine, s, new TurnReplayEvent());
            return s;
        }

        // act
        var final1 = ApplyScript(progress1, adv, pass, replay).GetStates<TurnState>().First();
        var final2 = ApplyScript(progress2, adv, pass, replay).GetStates<TurnState>().First();

        // assert
        final1.TurnNumber.Should().Be(final2.TurnNumber);
        final1.Segment.Should().Be(final2.Segment);
        final1.PassStreak.Should().Be(final2.PassStreak);
    }

    [Fact]
    public void GivenReplayAfterPass_WhenApplied_ThenPassStreakResets()
    {
        // arrange

        // act

        // assert

        Boards.Internal.FeatureFlags.EnableTurnSequencing = true;
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var pass = new TurnPassStateMutator();
        var replay = new TurnReplayStateMutator();

        // act
        var afterPass = pass.MutateState(progress.Engine, progress.State, new TurnPassEvent());
        var mid = afterPass.GetStates<TurnState>().First();
        mid.PassStreak.Should().Be(1);
        var afterReplay = replay.MutateState(progress.Engine, afterPass, new TurnReplayEvent());
        var final = afterReplay.GetStates<TurnState>().First();

        // assert
        final.PassStreak.Should().Be(0);
    }
}
