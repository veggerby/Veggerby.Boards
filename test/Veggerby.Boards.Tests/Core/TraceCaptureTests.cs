using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core;

public class TraceCaptureTests
{
    private static (Infrastructure.FeatureFlagScope scope, GameProgress progress) Build(bool trace, bool hashing)
    {
        var scope = new Infrastructure.FeatureFlagScope(trace: trace, hashing: hashing);
        var builder = new TestGameBuilder(useSimpleGamePhase: false);
        return (scope, builder.Compile());
    }



    [Fact]
    public void When_Trace_Enabled_LastTrace_Populated_After_Move()
    {
        // arrange

        // act

        // assert

        var (scope2, progress) = Build(trace: true, hashing: true);
        using var __ = scope2;
        var from = progress.Game.GetTile("tile-1");
        from.Should().NotBeNull();
        var to = progress.Game.GetTile("tile-2");
        to.Should().NotBeNull();
        var relation = progress.Game.Board.TileRelations.Single(r => r.From.Equals(from) && r.To.Equals(to));
        relation.Should().NotBeNull();
        var path = new TilePath([relation]);
        var piece = progress.Game.GetPiece("piece-1");
        piece.Should().NotBeNull();
        var move = new MovePieceGameEvent(piece!, path);

        // act
        progress = progress.HandleEvent(move);

        // assert
        var trace = progress.Engine.LastTrace;
        trace.Should().NotBeNull();
        trace!.Entries.Should().NotBeEmpty();
        trace!.Entries.Any(e => e.Kind == "RuleApplied").Should().BeTrue();
        trace.Entries.Last().StateHash.Should().Be(progress.State.Hash);
    }

    [Fact]
    public void Trace_Contains_PhaseEnter_Then_RuleEvaluated_Before_RuleApplied()
    {
        // arrange

        // act

        // assert

        var (scope3, progress) = Build(trace: true, hashing: false);
        using var ___ = scope3;
        var from = progress.Game.GetTile("tile-1");
        from.Should().NotBeNull();
        var to = progress.Game.GetTile("tile-2");
        to.Should().NotBeNull();
        var relation = progress.Game.Board.TileRelations.Single(r => r.From.Equals(from) && r.To.Equals(to));
        relation.Should().NotBeNull();
        var path = new TilePath([relation]);
        var piece = progress.Game.GetPiece("piece-1");
        piece.Should().NotBeNull();
        var move = new MovePieceGameEvent(piece!, path);

        // act
        progress = progress.HandleEvent(move);
        var kinds = progress.Engine.LastTrace!.Entries.Select(e => e.Kind).ToList();
        var phaseEnterIndex = kinds.IndexOf("PhaseEnter");
        var ruleEvaluatedIndex = kinds.IndexOf("RuleEvaluated");
        var ruleAppliedIndex = kinds.IndexOf("RuleApplied");

        // assert
        (phaseEnterIndex >= 0).Should().BeTrue();
        (ruleEvaluatedIndex > phaseEnterIndex).Should().BeTrue();
        (ruleAppliedIndex > ruleEvaluatedIndex).Should().BeTrue();

        // and rule evaluation entries have a non-null RuleIndex (>=0)
        var evaluated = progress.Engine.LastTrace!.Entries.First(e => e.Kind == "RuleEvaluated");
        evaluated.RuleIndex.Should().NotBeNull();
        evaluated.RuleIndex.Should().BeGreaterThanOrEqualTo(0);
    }
}
