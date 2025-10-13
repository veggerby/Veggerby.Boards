using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Support;

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingSupplyStatsTests
{
    [Fact]
    public void GivenSupplyAboveZero_WhenGain_NotCrossingZero_ThenEmptyPilesUnchanged()
    {
        using var guard = FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var builder = new DeckBuildingGameBuilder();
        var c1 = new Card("iron");
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
        var supply = new Dictionary<string, int> { [c1.Id] = 3 }; // >1 so first decrement does not cross zero

        progress = progress.HandleEvent(new CreateDeckEvent(deck, piles, supply));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        var beforeStats = progress.State.GetExtras<DeckSupplyStats>();
        beforeStats.EmptyPiles.Should().Be(0);
        beforeStats.TotalPiles.Should().Be(1);

        // act
        progress = progress.HandleEvent(new GainFromSupplyEvent(p1, deck, c1.Id, DeckBuildingGameBuilder.Piles.Discard));

        // assert
        var afterStats = progress.State.GetExtras<DeckSupplyStats>();
        afterStats.EmptyPiles.Should().Be(0); // not crossed zero yet
        afterStats.TotalPiles.Should().Be(1);
    }

    [Fact]
    public void GivenSupplyAtOne_WhenGain_CrossesZero_ThenEmptyPilesIncrements()
    {
        using var guard = FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var builder = new DeckBuildingGameBuilder();
        var c1 = new Card("silverling");
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
        var supply = new Dictionary<string, int> { [c1.Id] = 1 }; // decrement will cross to zero

        progress = progress.HandleEvent(new CreateDeckEvent(deck, piles, supply));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        var beforeStats = progress.State.GetExtras<DeckSupplyStats>();
        beforeStats.EmptyPiles.Should().Be(0);

        // act
        progress = progress.HandleEvent(new GainFromSupplyEvent(p1, deck, c1.Id, DeckBuildingGameBuilder.Piles.Discard));

        // assert
        var afterStats = progress.State.GetExtras<DeckSupplyStats>();
        afterStats.EmptyPiles.Should().Be(1); // crossed to zero
        afterStats.TotalPiles.Should().Be(1);
    }
}