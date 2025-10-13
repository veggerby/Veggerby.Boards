using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Internal.Paths;

public class PathResolverRegistrationTests
{
    [Fact]
    public void GivenCompiledPatternsEnabled_WhenBuildingGame_ThenPathResolverRegistered()
    {
        // arrange
        using var scope = new FeatureFlagScope(compiledPatterns: true, bitboards: false, boardShape: true);
        var progress = new ChessGameBuilder().Compile();

        // act
        var capabilities = progress.Engine.Capabilities;

        // assert
        capabilities.Should().NotBeNull();
        capabilities!.PathResolver.Should().NotBeNull();
    }

    [Fact]
    public void GivenCompiledPatternsDisabled_WhenBuildingGame_ThenFallbackPathResolverRegistered()
    {
        // arrange
        using var scope = new FeatureFlagScope(compiledPatterns: false, bitboards: false, boardShape: true);
        var progress = new ChessGameBuilder().Compile();

        // act
        var capabilities = progress.Engine.Capabilities; // simple visitor-based resolver

        // assert
        capabilities.Should().NotBeNull();
        capabilities!.PathResolver.Should().NotBeNull();
    }
}