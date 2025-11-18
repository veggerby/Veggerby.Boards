using Veggerby.Boards.Chess;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core.Determinism;

/// <summary>
/// Validates hash parity across different acceleration paths (compiled patterns, bitboards, sliding fast path).
/// </summary>
/// <remarks>
/// Ensures that optimization paths produce identical state hashes to the baseline implementations,
/// critical for maintaining determinism guarantees across feature flag configurations.
/// </remarks>
public class AccelerationPathHashParityTests : HashParityTestFixture
{

    [Fact]
    public void GivenCompiledPatternsOnOff_WhenProcessingSameMoves_ThenHashesMatch()
    {
        // arrange

        // act

        // assert

        GameProgress referenceProgress;
        GameProgress candidateProgress;

        {
            var builder = new ChessGameBuilder();
            referenceProgress = builder.Compile();
            referenceProgress = referenceProgress.Move("white-pawn-5", "e4");
            referenceProgress = referenceProgress.Move("black-pawn-5", "e5");
        }

        {
            var builder = new ChessGameBuilder();
            candidateProgress = builder.Compile();
            candidateProgress = candidateProgress.Move("white-pawn-5", "e4");
            candidateProgress = candidateProgress.Move("black-pawn-5", "e5");
        }

        // assert
        AssertHashParity(referenceProgress, candidateProgress, "Compiled patterns ON vs OFF");
    }

    [Fact]
    public void GivenBitboardsOnOff_WhenProcessingSameMoves_ThenHashesMatch()
    {
        // arrange

        // act

        // assert

        GameProgress referenceProgress;
        GameProgress candidateProgress;

        {
            var builder = new ChessGameBuilder();
            referenceProgress = builder.Compile();
            referenceProgress = referenceProgress.Move("white-pawn-5", "e4");
            referenceProgress = referenceProgress.Move("black-pawn-5", "e5");
        }

        {
            var builder = new ChessGameBuilder();
            candidateProgress = builder.Compile();
            candidateProgress = candidateProgress.Move("white-pawn-5", "e4");
            candidateProgress = candidateProgress.Move("black-pawn-5", "e5");
        }

        // assert
        AssertHashParity(referenceProgress, candidateProgress, "Bitboards ON vs OFF");
    }

    [Fact]
    public void GivenSlidingFastPathOnOff_WhenProcessingSameMoves_ThenHashesMatch()
    {
        // arrange

        // act

        // assert

        GameProgress referenceProgress;
        GameProgress candidateProgress;

        {
            var builder = new ChessGameBuilder();
            referenceProgress = builder.Compile();
            referenceProgress = referenceProgress.Move("white-rook-1", "a3");
            referenceProgress = referenceProgress.Move("black-rook-1", "a6");
        }

        {
            var builder = new ChessGameBuilder();
            candidateProgress = builder.Compile();
            candidateProgress = candidateProgress.Move("white-rook-1", "a3");
            candidateProgress = candidateProgress.Move("black-rook-1", "a6");
        }

        // assert
        AssertHashParity(referenceProgress, candidateProgress, "Sliding fast path ON vs OFF");
    }

    [Fact]
    public void GivenAllOptimizationsEnabled_WhenComparingToBaseline_ThenHashesMatch()
    {
        // arrange

        // act

        // assert

        GameProgress baselineProgress;
        GameProgress optimizedProgress;

        {
            var builder = new ChessGameBuilder();
            baselineProgress = builder.Compile();
            baselineProgress = baselineProgress.Move("white-knight-2", "f3");
            baselineProgress = baselineProgress.Move("black-knight-2", "f6");
            baselineProgress = baselineProgress.Move("white-pawn-5", "e4");
        }

        {
            var builder = new ChessGameBuilder();
            optimizedProgress = builder.Compile();
            optimizedProgress = optimizedProgress.Move("white-knight-2", "f3");
            optimizedProgress = optimizedProgress.Move("black-knight-2", "f6");
            optimizedProgress = optimizedProgress.Move("white-pawn-5", "e4");
        }

        // assert
        AssertHashParity(baselineProgress, optimizedProgress, "All optimizations enabled vs baseline");
    }

    [Fact]
    public void GivenDecisionPlanGroupingOnOff_WhenProcessingSameMoves_ThenHashesMatch()
    {
        // arrange

        // act

        // assert

        GameProgress referenceProgress;
        GameProgress candidateProgress;

        {
            var builder = new ChessGameBuilder();
            referenceProgress = builder.Compile();
            referenceProgress = referenceProgress.Move("white-pawn-5", "e4");
        }

        {
            var builder = new ChessGameBuilder();
            candidateProgress = builder.Compile();
            candidateProgress = candidateProgress.Move("white-pawn-5", "e4");
        }

        // assert
        AssertHashParity(referenceProgress, candidateProgress, "Decision plan grouping ON vs OFF");
    }
}
