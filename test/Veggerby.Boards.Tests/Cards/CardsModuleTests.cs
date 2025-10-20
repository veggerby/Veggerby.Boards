using System;
using System.Linq;

using Veggerby.Boards.Cards;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Cards;

public class CardsModuleTests
{
    [Fact]
    public void CreateDeck_ThenDraw_MovesCardsToHand()
    {
        // arrange
        var builder = new CardsGameBuilder();
        builder.WithSeed(123UL);
        var progress = builder.Compile();
        var create = builder.CreateInitialDeckEvent();

        // act
        var afterCreate = progress.HandleEvent(create).State;
        var deck = create.Deck;
        deck.Should().NotBeNull();
        var drawEvt = new DrawCardsEvent(deck!, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 2);
        var afterDraw = new GameProgress(progress.Engine, afterCreate, progress.Events).HandleEvent(drawEvt).State;

        // assert
        var ds = afterDraw.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        ds!.Piles[CardsGameBuilder.Piles.Hand].Count.Should().Be(2);
        ds!.Piles[CardsGameBuilder.Piles.Draw].Count.Should().Be(3);
    }

    [Fact]
    public void Shuffle_WithSeed_IsDeterministic()
    {
        // arrange
        var builder1 = new CardsGameBuilder();
        builder1.WithSeed(42UL);
        var p1 = builder1.Compile();
        var create1 = builder1.CreateInitialDeckEvent();

        var builder2 = new CardsGameBuilder();
        builder2.WithSeed(42UL);
        var p2 = builder2.Compile();
        var create2 = builder2.CreateInitialDeckEvent();

        // act
        var s1 = p1.HandleEvent(create1).State;
        var s2 = p2.HandleEvent(create2).State;
        var deck1 = create1.Deck;
        var deck2 = create2.Deck;
        deck1.Should().NotBeNull();
        deck2.Should().NotBeNull();
        var shuffle = new ShuffleDeckEvent(deck1!, CardsGameBuilder.Piles.Draw);
        s1 = new GameProgress(p1.Engine, s1, p1.Events).HandleEvent(shuffle).State;
        s2 = new GameProgress(p2.Engine, s2, p2.Events).HandleEvent(new ShuffleDeckEvent(deck2!, CardsGameBuilder.Piles.Draw)).State;

        // assert
        var d1 = s1.GetState<DeckState>(deck1!);
        var d2 = s2.GetState<DeckState>(deck2!);
        d1.Should().NotBeNull();
        d2.Should().NotBeNull();
        d1!.Piles[CardsGameBuilder.Piles.Draw].Select(c => c.Id)
            .SequenceEqual(d2!.Piles[CardsGameBuilder.Piles.Draw].Select(c => c.Id))
            .Should().BeTrue();
    }

    [Fact]
    public void Draw_TooMany_ThrowsInvalidEvent()
    {
        // arrange
        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        var state = progress.HandleEvent(builder.CreateInitialDeckEvent()).State;
        var deck = builder.CreateInitialDeckEvent().Deck;
        deck.Should().NotBeNull();

        // act
        var act = () => new GameProgress(progress.Engine, state, progress.Events).HandleEvent(new DrawCardsEvent(deck!, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 10));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }
}