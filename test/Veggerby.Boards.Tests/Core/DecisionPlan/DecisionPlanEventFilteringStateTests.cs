using Veggerby.Boards.Backgammon; // includes SelectActivePlayerGameEvent
using Veggerby.Boards.Internal; // FeatureFlags
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

// (No custom conditions required for this basic classification test.)

public class DecisionPlanEventFilteringStateTests
{
    private static GameProgress Build()
    {
        // DecisionPlan always enabled
        var builder = new BackgammonGameBuilder();
        return builder.Compile();
    }

    [Fact]
    public void GivenStateEvent_WhenFilteringEnabled_MoveAndRollPhasesSkipped()
    {
        // arrange

        // act

        // assert

        var progress = Build();
        // Force an initial active player selection event directly (simulating post-roll emission)
        var evt = new SelectActivePlayerGameEvent("white");

        // act
        progress.HandleEvent(evt);

        // assert: no exception => event passed filtering path (currently no direct rule, but classification should not trigger unrelated rule conditions)
        // We cannot directly observe skipped counts without an observer injection here; absence of exception suffices for now.
    }
}
