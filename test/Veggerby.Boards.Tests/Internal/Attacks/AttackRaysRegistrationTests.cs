using Veggerby.Boards.Chess;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Internal.Attacks;

public class AttackRaysRegistrationTests
{
    [Fact]
    public void GivenBitboardsEnabled_WhenBuildingGame_ThenAttackRaysRegistered()
    {
        // arrange
        using var scope = new FeatureFlagScope(bitboards: true, compiledPatterns: true, boardShape: true);
        var progress = new ChessGameBuilder().Compile();

        // act
        var rays = progress.Engine.Capabilities?.AccelerationContext.AttackRays;

        // assert
        rays.Should().NotBeNull();
    }

    [Fact]
    public void GivenBitboardsDisabled_WhenBuildingGame_ThenAttackRaysStillRegistered()
    {
        // arrange
        using var scope = new FeatureFlagScope(bitboards: false, compiledPatterns: true, boardShape: true);
        var progress = new ChessGameBuilder().Compile();

        // act
        var rays = progress.Engine.Capabilities?.AccelerationContext.AttackRays; // naive context still provides sliding attacks

        // assert
        rays.Should().NotBeNull();
    }
}