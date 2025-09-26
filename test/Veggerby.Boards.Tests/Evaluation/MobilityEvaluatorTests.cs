using System.Linq;

using Veggerby.Boards.Internal.Evaluation.Mobility;
using Veggerby.Boards.Tests.Infrastructure;

using Xunit;

namespace Veggerby.Boards.Tests.Evaluation;

public class MobilityEvaluatorTests
{
    [Fact]
    public void GivenStandardChessStart_WhenComputingMobility_ThenCountsNonZeroAndDeterministic()
    {
        // arrange
        using var scope = new FeatureFlagScope(bitboards: true, compiledPatterns: true, slidingFastPath: true);
        var builder = new Boards.Chess.ChessGameBuilder();
        var progress = builder.Compile();
        var eval = MobilityEvaluator.TryCreate(progress.Engine.Capabilities);
        Assert.NotNull(eval); // bitboards active

        // act
        var counts1 = eval.Compute(progress.State);
        var counts2 = eval.Compute(progress.State); // repeat to ensure determinism / no mutation

        // assert
        Assert.Equal(counts1, counts2);
        Assert.True(counts1.Length >= 2);
        // At initial chess position each side has some slider mobility (bishops blocked, knights have 2 each)
        Assert.Contains(counts1, c => c > 0);
    }
}