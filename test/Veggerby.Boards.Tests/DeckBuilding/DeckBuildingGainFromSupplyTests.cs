using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Support; // TurnStateAssertions

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingGainFromSupplyTests
{
    [Fact]
    public void GivenSupply_WhenGainToDiscard_ThenCardAppendedAndSupplyDecremented()
    {
        // arrange

        // act

        // assert

        using var guard = FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var builder = new DeckBuildingGameBuilder();
        // register test card artifacts before compile
        var c1 = new Card("copper");
        builder.WithCard(c1.Id);
        var progress = builder.Compile();
        var game = progress.Game;
        var p1 = game.GetPlayer("P1");
        p1.Should().NotBeNull();
        var deck = game.GetArtifact<Deck>("p1-deck");
        deck.Should().NotBeNull();

        // initialize deck state with empty piles and supply

        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card>(),
        };
        var supply = new Dictionary<string, int> { [c1.Id] = 10 };

        progress = progress.HandleEvent(new CreateDeckEvent(deck!, piles, supply));
        progress.ShouldHaveSingleTurnState();
        progress.State.GetState<DeckState>(deck).Should().NotBeNull();

        // act
        // transition Start -> Main -> keep Main for buy phase (gain handled in buy phase which uses Main segment gating)
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start)); // Start -> Main
        progress = progress.HandleEvent(new GainFromSupplyEvent(p1!, deck!, c1.Id, DeckBuildingGameBuilder.Piles.Discard));

        // assert
        var ds = progress.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        ds!.Supply[c1.Id].Should().Be(9);
        ds.Piles[DeckBuildingGameBuilder.Piles.Discard].Should().ContainSingle().Which.Should().Be(c1);
    }

    [Fact]
    public void GivenInsufficientSupply_WhenGain_ThenRejected()
    {
        // arrange

        // act

        // assert

        using var guard = FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var builder = new DeckBuildingGameBuilder();
        var c1 = new Card("silver");
        builder.WithCard(c1.Id);
        var progress = builder.Compile();
        var game = progress.Game;
        var p1 = game.GetPlayer("P1");
        p1.Should().NotBeNull();
        var deck = game.GetArtifact<Deck>("p1-deck");
        deck.Should().NotBeNull();

        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card>(),
        };
        var supply = new Dictionary<string, int> { [c1.Id] = 0 };

        progress = progress.HandleEvent(new CreateDeckEvent(deck!, piles, supply));
        progress.ShouldHaveSingleTurnState();
        progress.State.GetState<DeckState>(deck).Should().NotBeNull();

        // act
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start)); // Start -> Main
        var act = () => progress.HandleEvent(new GainFromSupplyEvent(p1!, deck!, c1.Id, DeckBuildingGameBuilder.Piles.Discard));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }
}
