using Veggerby.Boards.DeckBuilding;

using Veggerby.Boards.Cards.Artifacts;
namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingBuilderHelpersTests
{
    [Fact]
    public void WithCards_RegistersAllAndCompiles()
    {
        // arrange

        // act

        // assert

        var builder = new DeckBuildingGameBuilder();
        builder.WithCards("a", "b", "c");

        // act
        var progress = builder.Compile();

        // assert
        var deck = progress.Game.GetArtifact<Veggerby.Boards.Cards.Artifacts.Deck>("p1-deck");
        deck.Should().NotBeNull();
    }
}
