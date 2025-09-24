using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal.Paths;
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
        Assert.True(progress.Engine.Services.TryGet<IPathResolver>(out var resolver));
        Assert.NotNull(resolver);
    }

    [Fact]
    public void GivenCompiledPatternsDisabled_WhenBuildingGame_ThenPathResolverNotRegistered()
    {
        using var scope = new FeatureFlagScope(compiledPatterns: false, bitboards: false, boardShape: true);
        var progress = new ChessGameBuilder().Compile();
        Assert.False(progress.Engine.Services.TryGet<IPathResolver>(out _));
    }
}