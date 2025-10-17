using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core;

public class SeedDeterminismTests
{
    [Fact]
    public void GivenSameSeed_WhenBuildingGame_ThenRandomSourceSeedMatches()
    {
        // arrange
        const ulong seed = 424242UL;
        var builder1 = new TestGameBuilder(useSimpleGamePhase: false).WithSeed(seed);
        var builder2 = new TestGameBuilder(useSimpleGamePhase: false).WithSeed(seed);

        // act
        var progress1 = builder1.Compile();
        var progress2 = builder2.Compile();

        // assert
        progress1.State.Random.Should().NotBeNull();
        progress2.State.Random.Should().NotBeNull();
        progress1.State.Random!.Seed.Should().Be(seed);
        progress2.State.Random!.Seed.Should().Be(seed);
    }
}