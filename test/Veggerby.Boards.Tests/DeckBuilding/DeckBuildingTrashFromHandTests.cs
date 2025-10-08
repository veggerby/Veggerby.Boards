using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingTrashFromHandTests
{
    [Fact]
    public void GivenCardsInHand_WhenTrash_ThenRemoved()
    {
        // arrange
        var builder = new DeckBuildingGameBuilder();
        var c1 = new Card("a");
        var c2 = new Card("b");
        builder.WithCard(c1.Id);
        builder.WithCard(c2.Id);
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");
        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card> { c1, c2 },
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card>(),
        };
        progress = progress.HandleEvent(new CreateDeckEvent(deck, piles));

        // act
        progress = progress.HandleEvent(new TrashFromHandEvent(deck, new List<Card> { c1 }));

        // assert
        var ds = progress.State.GetState<DeckState>(deck);
        ds.Piles[DeckBuildingGameBuilder.Piles.Hand].Should().ContainSingle().Which.Should().Be(c2);
    }

    [Fact]
    public void GivenCardNotInHand_WhenTrash_ThenRejected()
    {
        // arrange
        var builder = new DeckBuildingGameBuilder();
        var c1 = new Card("a");
        var c2 = new Card("b");
        builder.WithCard(c1.Id);
        builder.WithCard(c2.Id);
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
        var act = () => progress.HandleEvent(new TrashFromHandEvent(deck, new List<Card> { c2 }));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }
}