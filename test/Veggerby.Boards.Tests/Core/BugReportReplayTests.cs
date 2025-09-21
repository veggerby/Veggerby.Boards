using System.Linq;

using Veggerby.Boards.Internal;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.Utils;

namespace Veggerby.Boards.Tests.Core;

public class BugReportReplayTests
{
    private static GameBuilder NewBuilder() => new TestGameBuilder(useSimpleGamePhase: false);

    [Fact]
    public void Replay_Should_Apply_Captured_Seed()
    {
        // Arrange
        using var _ = FeatureFlagScope.StateHashing(true);
        const ulong seed = 123456789UL;
        var progress = NewBuilder().WithSeed(seed).Compile();
        var report = progress.CaptureBugReport();
        report.Seed.Should().Be(seed);

        // Act
        var result = BugReportReplayer.Replay(report, NewBuilder);

        // Assert
        result.Success.Should().BeTrue();
        result.Reason.Should().BeEmpty();
    }

    [Fact]
    public void Replay_Should_Succeed_For_Empty_Event_Report_With_Matching_Hash()
    {
        // Arrange
        using var _ = FeatureFlagScope.StateHashing(true);
        var builder = NewBuilder();
        var progress = builder.Compile();
        var report = progress.CaptureBugReport();
        report.EventCount.Should().Be(0); // defensive

        // Act
        var result = BugReportReplayer.Replay(report, NewBuilder);

        // Assert
        result.Success.Should().BeTrue();
        result.Reason.Should().BeEmpty();
        result.AppliedEvents.Should().Be(0);
    }

    [Fact]
    public void Replay_Should_Fail_For_Report_With_Events_Pending_Payload_Synthesis()
    {
        // Arrange
        using var _ = FeatureFlagScope.StateHashing(true);
        var progress = NewBuilder().Compile();
        var piece = progress.Game.GetPiece("piece-1");
        var from = progress.Game.GetTile("tile-1");
        var to = progress.Game.GetTile("tile-2");
        var relation = progress.Game.Board.TileRelations.Single(r => r.From.Equals(from) && r.To.Equals(to));
        var path = new Veggerby.Boards.Artifacts.Relations.TilePath([relation]);
        progress = progress.HandleEvent(new Veggerby.Boards.Flows.Events.MovePieceGameEvent(piece, path));
        var reportWithEvent = progress.CaptureBugReport();
        reportWithEvent.EventCount.Should().Be(1); // defensive

        // Act
        var result = BugReportReplayer.Replay(reportWithEvent, NewBuilder);

        // Assert
        result.Success.Should().BeFalse();
        result.Reason.Should().Contain("payload");
        result.ExpectedEvents.Should().Be(1);
        result.AppliedEvents.Should().Be(0);
    }
}