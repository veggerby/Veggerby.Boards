using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Cards.Mutators;
using Veggerby.Boards.DeckBuilding;

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingGainFromSupplyTests
{
    [Fact]
    public void GivenSupply_WhenGainToDiscard_ThenCardAppendedAndSupplyDecremented()
    {
        // arrange
        var builder = new DeckBuildingGameBuilder();
        // register test card artifacts before compile
        var c1 = new Card("copper");
        builder.WithCard(c1.Id);
        var progress = builder.Compile();
        var game = progress.Game;
        var p1 = game.GetPlayer("P1");
        var deck = game.GetArtifact<Deck>("p1-deck");

        // initialize deck state with empty piles and supply

        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card>(),
        };
        var supply = new Dictionary<string, int> { [c1.Id] = 10 };

        progress = progress.HandleEvent(new CreateDeckEvent(deck, piles, supply));

        // act
        progress = progress.HandleEvent(new GainFromSupplyEvent(p1, deck, c1.Id, DeckBuildingGameBuilder.Piles.Discard));

        // assert
        var ds = progress.State.GetState<DeckState>(deck);
        ds.Supply[c1.Id].Should().Be(9);
        ds.Piles[DeckBuildingGameBuilder.Piles.Discard].Should().ContainSingle().Which.Should().Be(c1);
    }

    [Fact]
    public void GivenInsufficientSupply_WhenGain_ThenRejected()
    {
        // arrange
        var builder = new DeckBuildingGameBuilder();
        var c1 = new Card("silver");
        builder.WithCard(c1.Id);
        var progress = builder.Compile();
        var game = progress.Game;
        var p1 = game.GetPlayer("P1");
        var deck = game.GetArtifact<Deck>("p1-deck");

        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card>(),
        };
        var supply = new Dictionary<string, int> { [c1.Id] = 0 };

        progress = progress.HandleEvent(new CreateDeckEvent(deck, piles, supply));

        // act
        var act = () => progress.HandleEvent(new GainFromSupplyEvent(p1, deck, c1.Id, DeckBuildingGameBuilder.Piles.Discard));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }
}