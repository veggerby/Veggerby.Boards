using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingDrawWithReshuffleTests
{
    [Fact]
    public void GivenEmptyDraw_WithDiscard_WhenDraw2_ReshufflesAndDraws()
    {
        // arrange
        var builder = new DeckBuildingGameBuilder();
        builder.WithSeed(42UL);
        var c1 = new Card("a");
        var c2 = new Card("b");
        var c3 = new Card("c");
        builder.WithCard(c1.Id);
        builder.WithCard(c2.Id);
        builder.WithCard(c3.Id);
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");

        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card> { c1, c2, c3 },
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card>(),
        };
        progress = progress.HandleEvent(new CreateDeckEvent(deck, piles));

        // act
        progress = progress.HandleEvent(new DrawWithReshuffleEvent(deck, 2));

        // assert
        var ds = progress.State.GetState<DeckState>(deck);
        ds.Piles[DeckBuildingGameBuilder.Piles.Hand].Count.Should().Be(2);
        ds.Piles[DeckBuildingGameBuilder.Piles.Draw].Count.Should().Be(1);
        ds.Piles[DeckBuildingGameBuilder.Piles.Discard].Count.Should().Be(0);
    }

    [Fact]
    public void GivenInsufficientTotal_WhenDraw_ThenRejected()
    {
        // arrange
        var builder = new DeckBuildingGameBuilder();
        var c1 = new Card("a");
        builder.WithCard(c1.Id);
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");
        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card> { c1 },
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card>(),
        };
        progress = progress.HandleEvent(new CreateDeckEvent(deck, piles));

        // act
        var act = () => progress.HandleEvent(new DrawWithReshuffleEvent(deck, 2));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }
}