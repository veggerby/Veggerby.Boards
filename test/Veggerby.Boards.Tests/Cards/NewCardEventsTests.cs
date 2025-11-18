using System;
using System.Linq;

using Veggerby.Boards.Cards;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Cards;

public class NewCardEventsTests
{
    [Fact]
    public void PeekCards_DoesNotChangeState()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        builder.WithSeed(123UL);
        var progress = builder.Compile();
        var create = builder.CreateInitialDeckEvent();
        var afterCreate = progress.HandleEvent(create).State;
        var deck = create.Deck;

        // act
        var peekEvt = new PeekCardsEvent(deck!, CardsGameBuilder.Piles.Draw, 3);
        var afterPeek = new GameProgress(progress.Engine, afterCreate, progress.Events).HandleEvent(peekEvt).State;

        // assert
        var dsBefore = afterCreate.GetState<DeckState>(deck!);
        var dsAfter = afterPeek.GetState<DeckState>(deck!);
        dsBefore.Should().NotBeNull();
        dsAfter.Should().NotBeNull();
        dsBefore!.Piles[CardsGameBuilder.Piles.Draw].Count.Should().Be(5);
        dsAfter!.Piles[CardsGameBuilder.Piles.Draw].Count.Should().Be(5);
        dsBefore.Equals(dsAfter).Should().BeTrue();
    }

    [Fact]
    public void PeekCards_TooMany_ThrowsInvalidEvent()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        var state = progress.HandleEvent(builder.CreateInitialDeckEvent()).State;
        var deck = builder.CreateInitialDeckEvent().Deck;

        // act
        var act = () => new GameProgress(progress.Engine, state, progress.Events).HandleEvent(new PeekCardsEvent(deck!, CardsGameBuilder.Piles.Draw, 10));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }

    [Fact]
    public void RevealCards_DoesNotChangeState()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        var create = builder.CreateInitialDeckEvent();
        var afterCreate = progress.HandleEvent(create).State;
        var deck = create.Deck;

        // Draw some cards to hand first
        var drawEvt = new DrawCardsEvent(deck!, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 2);
        var afterDraw = new GameProgress(progress.Engine, afterCreate, progress.Events).HandleEvent(drawEvt).State;
        var ds = afterDraw.GetState<DeckState>(deck!);
        var handCards = ds!.Piles[CardsGameBuilder.Piles.Hand].ToList();

        // act
        var revealEvt = new RevealCardsEvent(deck!, CardsGameBuilder.Piles.Hand, handCards);
        var afterReveal = new GameProgress(progress.Engine, afterDraw, progress.Events).HandleEvent(revealEvt).State;

        // assert
        var dsBefore = afterDraw.GetState<DeckState>(deck!);
        var dsAfter = afterReveal.GetState<DeckState>(deck!);
        dsBefore.Should().NotBeNull();
        dsAfter.Should().NotBeNull();
        dsBefore.Equals(dsAfter).Should().BeTrue();
    }

    [Fact]
    public void RevealCards_NotInPile_ThrowsInvalidEvent()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        var create = builder.CreateInitialDeckEvent();
        var state = progress.HandleEvent(create).State;
        var deck = create.Deck;
        var ds = state.GetState<DeckState>(deck!);
        var drawCards = ds!.Piles[CardsGameBuilder.Piles.Draw].Take(2).ToList();

        // act - try to reveal cards from draw pile as if they're in hand
        var act = () => new GameProgress(progress.Engine, state, progress.Events).HandleEvent(new RevealCardsEvent(deck!, CardsGameBuilder.Piles.Hand, drawCards));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }

    [Fact]
    public void Reshuffle_MovesDiscardToDrawAndShuffles()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        builder.WithSeed(42UL);
        var progress = builder.Compile();
        var create = builder.CreateInitialDeckEvent();
        var state = progress.HandleEvent(create).State;
        var deck = create.Deck;

        // Move all cards to discard
        var ds = state.GetState<DeckState>(deck!);
        var allCards = ds!.Piles[CardsGameBuilder.Piles.Draw].ToList();
        var discardEvt = new DiscardCardsEvent(deck!, CardsGameBuilder.Piles.Discard, allCards);
        state = new GameProgress(progress.Engine, state, progress.Events).HandleEvent(discardEvt).State;

        // act
        var reshuffleEvt = new ReshuffleEvent(deck!, CardsGameBuilder.Piles.Discard, CardsGameBuilder.Piles.Draw);
        var afterReshuffle = new GameProgress(progress.Engine, state, progress.Events).HandleEvent(reshuffleEvt).State;

        // assert
        var dsAfter = afterReshuffle.GetState<DeckState>(deck!);
        dsAfter.Should().NotBeNull();
        dsAfter!.Piles[CardsGameBuilder.Piles.Draw].Count.Should().Be(5);
        dsAfter!.Piles[CardsGameBuilder.Piles.Discard].Count.Should().Be(0);
        // Verify shuffled (not in original order)
        var afterOrder = dsAfter.Piles[CardsGameBuilder.Piles.Draw].Select(c => c.Id).ToList();
        var originalOrder = allCards.Select(c => c.Id).ToList();
        afterOrder.SequenceEqual(originalOrder).Should().BeFalse();
    }

    [Fact]
    public void Reshuffle_WithSeed_IsDeterministic()
    {
        // arrange

        // act

        // assert

        var builder1 = new CardsGameBuilder();
        builder1.WithSeed(42UL);
        var p1 = builder1.Compile();
        var create1 = builder1.CreateInitialDeckEvent();
        var s1 = p1.HandleEvent(create1).State;
        var deck1 = create1.Deck;

        var builder2 = new CardsGameBuilder();
        builder2.WithSeed(42UL);
        var p2 = builder2.Compile();
        var create2 = builder2.CreateInitialDeckEvent();
        var s2 = p2.HandleEvent(create2).State;
        var deck2 = create2.Deck;

        // Move all cards to discard for both
        var ds1 = s1.GetState<DeckState>(deck1!);
        var allCards1 = ds1!.Piles[CardsGameBuilder.Piles.Draw].ToList();
        var discard1 = new DiscardCardsEvent(deck1!, CardsGameBuilder.Piles.Discard, allCards1);
        s1 = new GameProgress(p1.Engine, s1, p1.Events).HandleEvent(discard1).State;

        var ds2 = s2.GetState<DeckState>(deck2!);
        var allCards2 = ds2!.Piles[CardsGameBuilder.Piles.Draw].ToList();
        var discard2 = new DiscardCardsEvent(deck2!, CardsGameBuilder.Piles.Discard, allCards2);
        s2 = new GameProgress(p2.Engine, s2, p2.Events).HandleEvent(discard2).State;

        // act
        s1 = new GameProgress(p1.Engine, s1, p1.Events).HandleEvent(new ReshuffleEvent(deck1!, CardsGameBuilder.Piles.Discard, CardsGameBuilder.Piles.Draw)).State;
        s2 = new GameProgress(p2.Engine, s2, p2.Events).HandleEvent(new ReshuffleEvent(deck2!, CardsGameBuilder.Piles.Discard, CardsGameBuilder.Piles.Draw)).State;

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
    public void Reshuffle_EmptyDiscard_ResultsInEmptyDraw()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        var create = builder.CreateInitialDeckEvent();
        var state = progress.HandleEvent(create).State;
        var deck = create.Deck;

        // Draw all cards to hand (emptying draw pile)
        var drawEvt = new DrawCardsEvent(deck!, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 5);
        state = new GameProgress(progress.Engine, state, progress.Events).HandleEvent(drawEvt).State;

        // act - reshuffle with empty discard
        var reshuffleEvt = new ReshuffleEvent(deck!, CardsGameBuilder.Piles.Discard, CardsGameBuilder.Piles.Draw);
        var afterReshuffle = new GameProgress(progress.Engine, state, progress.Events).HandleEvent(reshuffleEvt).State;

        // assert
        var ds = afterReshuffle.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        ds!.Piles[CardsGameBuilder.Piles.Draw].Count.Should().Be(0);
        ds!.Piles[CardsGameBuilder.Piles.Discard].Count.Should().Be(0);
    }
}
