using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingCleanupToDiscardTests
{
    [Fact]
    public void GivenCardsInHandAndInPlay_WhenCleanup_ThenAllMoveToDiscardAndSourcesCleared()
    {
        // arrange
        var builder = new DeckBuildingGameBuilder();
        builder.WithSeed(123UL);
        var c1 = new Card("a");
        var c2 = new Card("b");
        var c3 = new Card("c");
        var c4 = new Card("d");
        builder.WithCard(c1.Id);
        builder.WithCard(c2.Id);
        builder.WithCard(c3.Id);
        builder.WithCard(c4.Id);
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");

        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card> { },
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card> { c1, c2 },
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card> { c3, c4 },
        };
        progress = progress.HandleEvent(new CreateDeckEvent(deck, piles));

        // act
        progress = progress.HandleEvent(new CleanupToDiscardEvent(deck));

        // assert
        var ds = progress.State.GetState<DeckState>(deck);
        ds.Piles[DeckBuildingGameBuilder.Piles.Discard].Should().ContainInOrder(new[] { c1, c2, c3, c4 });
        ds.Piles[DeckBuildingGameBuilder.Piles.Hand].Count.Should().Be(0);
        ds.Piles[DeckBuildingGameBuilder.Piles.InPlay].Count.Should().Be(0);
    }

    [Fact]
    public void GivenNoCardsInHandOrInPlay_WhenCleanup_ThenNoOp()
    {
        // arrange
        var builder = new DeckBuildingGameBuilder();
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");
        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card> { },
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card> { },
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card> { },
        };
        progress = progress.HandleEvent(new CreateDeckEvent(deck, piles));

        // act
        var updated = progress.HandleEvent(new CleanupToDiscardEvent(deck));

        // assert
        updated.State.Should().NotBeSameAs(progress.State);
        var ds = updated.State.GetState<DeckState>(deck);
        ds.Piles[DeckBuildingGameBuilder.Piles.Discard].Count.Should().Be(0);
        ds.Piles[DeckBuildingGameBuilder.Piles.Hand].Count.Should().Be(0);
        ds.Piles[DeckBuildingGameBuilder.Piles.InPlay].Count.Should().Be(0);
    }
}