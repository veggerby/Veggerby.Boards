using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Veggerby.Boards.Tests.Utils;

namespace Veggerby.Boards.Tests.Core;

public class BugReportTests
{
    private GameProgress Build(bool hashing)
    {
        using var _ = FeatureFlagScope.StateHashing(hashing);
        var builder = new TestGameBuilder(useSimpleGamePhase: false);
        return builder.Compile();
    }

    [Fact]
    public void Capture_Should_Include_Final_Hash_When_Hashing_Enabled()
    {
        var progress = Build(true);
        var report = progress.CaptureBugReport();
        report.FinalHash64.Should().Be(progress.State.Hash);
        report.FinalHash128.Should().Be(progress.State.Hash128);
    }

    [Fact]
    public void Capture_Should_List_Event_Types_After_Events_Applied()
    {
        var progress = Build(true);
        var from = progress.Game.GetTile("tile-1");
        var to = progress.Game.GetTile("tile-2");
        var relation = progress.Game.Board.TileRelations.Single(r => r.From.Equals(from) && r.To.Equals(to));
        var path = new TilePath([relation]);
        var move = new MovePieceGameEvent(progress.Game.GetPiece("piece-1"), path);
        progress = progress.HandleEvent(move);
        var report = progress.CaptureBugReport();
        report.EventCount.Should().Be(1);
        report.EventTypeNames.Should().Contain(typeof(MovePieceGameEvent).FullName);
    }
}