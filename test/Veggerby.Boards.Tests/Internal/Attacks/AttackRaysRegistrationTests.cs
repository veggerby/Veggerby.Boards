using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal.Attacks;
using Veggerby.Boards.Tests.Utils;

using Xunit;

namespace Veggerby.Boards.Tests.Internal.Attacks;

public class AttackRaysRegistrationTests
{
    [Fact]
    public void GivenBitboardsEnabled_WhenBuildingGame_ThenAttackRaysRegistered()
    {
        using var scope = new FeatureFlagScope(bitboards: true, compiledPatterns: true, boardShape: true);
        var progress = new ChessGameBuilder().Compile();
        Assert.NotNull(progress.Engine.Capabilities?.Attacks?.Sliding);
    }

    [Fact]
    public void GivenBitboardsDisabled_WhenBuildingGame_ThenAttackRaysNotRegistered()
    {
        using var scope = new FeatureFlagScope(bitboards: false, compiledPatterns: true, boardShape: true);
        var progress = new ChessGameBuilder().Compile();
        Assert.True(progress.Engine.Capabilities?.Attacks == null);
    }
}