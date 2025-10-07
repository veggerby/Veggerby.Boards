using Veggerby.Boards.DeckBuilding;

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingScaffoldTests
{
    [Fact]
    public void GivenScaffold_WhenCompiled_ThenGameIsConstructed()
    {
        // arrange
        var builder = new DeckBuildingGameBuilder();
        builder.WithSeed(123UL);

        // act
        var compiled = builder.Compile();

        // assert
        compiled.Game.Should().NotBeNull();
        compiled.State.Should().NotBeNull();
    }
}