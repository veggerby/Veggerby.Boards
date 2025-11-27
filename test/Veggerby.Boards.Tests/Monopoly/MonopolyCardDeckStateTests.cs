using System.Linq;

using Veggerby.Boards.Monopoly.Cards;
using Veggerby.Boards.Monopoly.States;

namespace Veggerby.Boards.Tests.Monopoly;

public class MonopolyCardDeckStateTests
{
    [Fact]
    public void Constructor_WithValidCards_CreatesState()
    {
        // arrange
        var cards = MonopolyCardDecks.ChanceCards;

        // act
        var state = new MonopolyCardDeckState(MonopolyCardDecks.ChanceDeckId, cards);

        // assert
        state.DeckId.Should().Be(MonopolyCardDecks.ChanceDeckId);
        state.DrawPile.Count.Should().Be(16);
        state.DiscardPile.Count.Should().Be(0);
    }

    [Fact]
    public void DrawCard_FromFullDeck_ReturnsTopCard()
    {
        // arrange
        var cards = MonopolyCardDecks.ChanceCards;
        var state = new MonopolyCardDeckState(MonopolyCardDecks.ChanceDeckId, cards);
        var topCard = cards[0];

        // act
        var (newState, drawnCard) = state.DrawCard();

        // assert
        drawnCard.Should().NotBeNull();
        drawnCard!.Id.Should().Be(topCard.Id);
        newState.DrawPile.Count.Should().Be(15);
        newState.DiscardPile.Count.Should().Be(1);
        newState.DiscardPile[0].Id.Should().Be(topCard.Id);
    }

    [Fact]
    public void DrawCard_FromEmptyDeck_ReturnsNull()
    {
        // arrange
        var state = new MonopolyCardDeckState(MonopolyCardDecks.ChanceDeckId, []);

        // act
        var (newState, drawnCard) = state.DrawCard();

        // assert
        drawnCard.Should().BeNull();
        newState.Should().Be(state);
    }

    [Fact]
    public void Reshuffle_CombinesAndShufflesCards()
    {
        // arrange
        var cards = MonopolyCardDecks.ChanceCards.Take(5).ToList();
        var state = new MonopolyCardDeckState(MonopolyCardDecks.ChanceDeckId, cards.Take(2), cards.Skip(2));

        // act
        var reshuffled = state.Reshuffle(42);

        // assert
        reshuffled.DrawPile.Count.Should().Be(5);
        reshuffled.DiscardPile.Count.Should().Be(0);
    }

    [Fact]
    public void Reshuffle_IsDeterministic()
    {
        // arrange
        var cards = MonopolyCardDecks.ChanceCards;
        var state1 = new MonopolyCardDeckState(MonopolyCardDecks.ChanceDeckId, cards);
        var state2 = new MonopolyCardDeckState(MonopolyCardDecks.ChanceDeckId, cards);

        // act
        var reshuffled1 = state1.Reshuffle(42);
        var reshuffled2 = state2.Reshuffle(42);

        // assert
        for (int i = 0; i < reshuffled1.DrawPile.Count; i++)
        {
            reshuffled1.DrawPile[i].Id.Should().Be(reshuffled2.DrawPile[i].Id);
        }
    }

    [Fact]
    public void ChanceCards_Contains16Cards()
    {
        // arrange & act
        var cards = MonopolyCardDecks.ChanceCards;

        // assert
        cards.Count.Should().Be(16);
    }

    [Fact]
    public void CommunityChestCards_Contains16Cards()
    {
        // arrange & act
        var cards = MonopolyCardDecks.CommunityChestCards;

        // assert
        cards.Count.Should().Be(16);
    }

    [Fact]
    public void ChanceCards_ContainsGetOutOfJailCard()
    {
        // arrange & act
        var cards = MonopolyCardDecks.ChanceCards;

        // assert
        var getOutOfJailCard = cards.FirstOrDefault(c => c.Effect == MonopolyCardEffect.GetOutOfJailFree);
        getOutOfJailCard.Should().NotBeNull();
    }

    [Fact]
    public void CommunityChestCards_ContainsGetOutOfJailCard()
    {
        // arrange & act
        var cards = MonopolyCardDecks.CommunityChestCards;

        // assert
        var getOutOfJailCard = cards.FirstOrDefault(c => c.Effect == MonopolyCardEffect.GetOutOfJailFree);
        getOutOfJailCard.Should().NotBeNull();
    }

    [Fact]
    public void RemoveGetOutOfJailCard_RemovesCardFromDiscardPile()
    {
        // arrange
        var getOutOfJailCard = MonopolyCardDecks.ChanceCards.First(c => c.Effect == MonopolyCardEffect.GetOutOfJailFree);
        var otherCard = MonopolyCardDecks.ChanceCards.First(c => c.Effect != MonopolyCardEffect.GetOutOfJailFree);
        var state = new MonopolyCardDeckState(MonopolyCardDecks.ChanceDeckId, [], [getOutOfJailCard, otherCard]);

        // act
        var newState = state.RemoveGetOutOfJailCard();

        // assert
        newState.DiscardPile.Count.Should().Be(1);
        newState.DiscardPile.Should().NotContain(c => c.Effect == MonopolyCardEffect.GetOutOfJailFree);
    }
}
