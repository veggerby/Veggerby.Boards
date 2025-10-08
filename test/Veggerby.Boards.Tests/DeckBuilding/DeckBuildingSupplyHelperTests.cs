using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingSupplyHelperTests
{
    [Fact]
    public void BuildSupply_WhenUsedWithCreateDeck_ThenGainFromSupplyDecrements()
    {
        // arrange
        var builder = new DeckBuildingGameBuilder();
        builder.WithCards("copper");
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");
        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card>(),
        };
        var supply = DeckBuildingTestHelpers.BuildSupply(("copper", 5));
        progress = progress.HandleEvent(new CreateDeckEvent(deck, piles, supply));

        // act
        var enumerator = progress.Game.Players.GetEnumerator();
        Assert.True(enumerator.MoveNext());
        var player = enumerator.Current;
        progress = progress.HandleEvent(new GainFromSupplyEvent(player, deck, "copper", DeckBuildingGameBuilder.Piles.Discard));

        // assert
        var ds = progress.State.GetState<DeckState>(deck);
        ds.Supply["copper"].Should().Be(4);
        ds.Piles[DeckBuildingGameBuilder.Piles.Discard].Count.Should().Be(1);
    }
}