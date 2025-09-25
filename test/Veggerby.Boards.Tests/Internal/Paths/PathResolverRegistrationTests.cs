using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal.Paths;
using Veggerby.Boards.Tests.Infrastructure;
using Veggerby.Boards.Tests.Utils;

using Xunit;

namespace Veggerby.Boards.Tests.Internal.Paths;

public class PathResolverRegistrationTests
{
    [Fact]
    public void GivenCompiledPatternsEnabled_WhenBuildingGame_ThenPathResolverRegistered()
    {
        using var scope = new FeatureFlagScope(compiledPatterns: true, bitboards: false, boardShape: true);
        var progress = new ChessGameBuilder().Compile();
        Assert.NotNull(progress.Engine.Capabilities);
        Assert.NotNull(progress.Engine.Capabilities.PathResolver);
    }

    [Fact]
    public void GivenCompiledPatternsDisabled_WhenBuildingGame_ThenFallbackPathResolverRegistered()
    {
        using var scope = new FeatureFlagScope(compiledPatterns: false, bitboards: false, boardShape: true);
        var progress = new ChessGameBuilder().Compile();
        Assert.NotNull(progress.Engine.Capabilities);
        Assert.NotNull(progress.Engine.Capabilities.PathResolver); // simple visitor-based resolver
    }
}