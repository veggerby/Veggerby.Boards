using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Support; // TurnStateAssertions
using AwesomeAssertions;

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingSupplyHelperTests
{
    [Fact]
    public void BuildSupply_WhenUsedWithCreateDeck_ThenGainFromSupplyDecrements()
    {
        using var guard = Veggerby.Boards.Tests.Support.FeatureFlagGuard.ForceTurnSequencing(true);
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
        progress.ShouldHaveSingleTurnState();
        progress.State.GetState<DeckState>(deck).Should().NotBeNull();

        // act
        var enumerator = progress.Game.Players.GetEnumerator();
        enumerator.MoveNext().Should().BeTrue();
        var player = enumerator.Current;
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        progress = progress.HandleEvent(new GainFromSupplyEvent(player, deck, "copper", DeckBuildingGameBuilder.Piles.Discard));

        // assert
        var ds = progress.State.GetState<DeckState>(deck)!;
        ds.Supply["copper"].Should().Be(4);
        ds.Piles[DeckBuildingGameBuilder.Piles.Discard].Count.Should().Be(1);
    }
}