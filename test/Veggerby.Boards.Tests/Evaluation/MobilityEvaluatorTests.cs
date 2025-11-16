using Veggerby.Boards.Internal.Evaluation.Mobility;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Evaluation;

public class MobilityEvaluatorTests
{
    [Fact]
    public void GivenStandardChessStart_WhenComputingMobility_ThenCountsNonZeroAndDeterministic()
    {
        // arrange

        // act

        // assert

        var builder = new Boards.Chess.ChessGameBuilder();
        var progress = builder.Compile();
        var eval = MobilityEvaluator.TryCreate(progress.Engine.Capabilities);
        eval.Should().NotBeNull(); // bitboards active

        // act
        var counts1 = eval!.Compute(progress.State);
        var counts2 = eval.Compute(progress.State); // repeat to ensure determinism / no mutation

        // assert
        counts2.Should().BeEquivalentTo(counts1);
        counts1.Length.Should().BeGreaterThanOrEqualTo(2);
        // At initial chess position each side has some slider mobility (bishops blocked, knights have 2 each)
        counts1.Should().Contain(c => c > 0);
    }
}
