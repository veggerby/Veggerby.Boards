using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Cards;

namespace Veggerby.Boards.Tests.Cards;

public class CardsModuleMoreTests
{
    [Fact]
    public void Draw_Zero_IsNoOp()
    {
        // arrange
        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        var afterCreate = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = afterCreate.Game.GetArtifact<Deck>("deck-1");
        deck.Should().NotBeNull();
        var before = afterCreate.State.GetState<DeckState>(deck!);
        before.Should().NotBeNull();

        // act
        var after = afterCreate.HandleEvent(new DrawCardsEvent(deck!, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 0));

        // assert
        var ds = after.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        ds!.Piles[CardsGameBuilder.Piles.Draw].Count.Should().Be(before!.Piles[CardsGameBuilder.Piles.Draw].Count);
        ds!.Piles[CardsGameBuilder.Piles.Hand].Count.Should().Be(before!.Piles[CardsGameBuilder.Piles.Hand].Count);
    }

    [Fact]
    public void Move_ByCount_MovesTopNPreservingOrder()
    {
        // arrange
        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = progress.Game.GetArtifact<Deck>("deck-1");
        deck.Should().NotBeNull();

        // act
        progress = progress.HandleEvent(new MoveCardsEvent(deck!, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 3));

        // assert
        var ds = progress.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        var hand = ds.Piles[CardsGameBuilder.Piles.Hand].Select(c => c.Id).ToArray();
        hand.Should().Equal(new[] { CardsGameBuilder.CardIds.C1, CardsGameBuilder.CardIds.C2, CardsGameBuilder.CardIds.C3 });
        ds.Piles[CardsGameBuilder.Piles.Draw].Count.Should().Be(2);
    }

    [Fact]
    public void Move_ExplicitCards_FromSourceOnly_PreservesProvidedOrder()
    {
        // arrange
        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = progress.Game.GetArtifact<Deck>("deck-1");
        deck.Should().NotBeNull();
        var ds = progress.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        var draw = ds.Piles[CardsGameBuilder.Piles.Draw];
        var c1 = draw[0]; var c3 = draw[2];

        // act
        progress = progress.HandleEvent(new MoveCardsEvent(deck!, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, new List<Card> { c3, c1 }));

        // assert
        ds = progress.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        ds.Piles[CardsGameBuilder.Piles.Hand].Select(c => c.Id).Should().Equal(new[] { c3.Id, c1.Id });
        ds.Piles[CardsGameBuilder.Piles.Draw].Select(c => c.Id).Should().Equal(new[] { CardsGameBuilder.CardIds.C2, CardsGameBuilder.CardIds.C4, CardsGameBuilder.CardIds.C5 });
    }

    [Fact]
    public void Discard_FromMultiplePiles_AppendsInOrder()
    {
        // arrange
        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = progress.Game.GetArtifact<Deck>("deck-1");
        deck.Should().NotBeNull();
        // move two to hand first
        progress = progress.HandleEvent(new MoveCardsEvent(deck!, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 2));
        var ds = progress.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        var cFromDraw = ds.Piles[CardsGameBuilder.Piles.Draw][0];
        var cFromHand = ds.Piles[CardsGameBuilder.Piles.Hand][1];

        // act
        progress = progress.HandleEvent(new DiscardCardsEvent(deck!, CardsGameBuilder.Piles.Discard, new List<Card> { cFromHand, cFromDraw }));

        // assert
        ds = progress.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        ds.Piles[CardsGameBuilder.Piles.Discard].Select(c => c.Id).Should().Equal(new[] { cFromHand.Id, cFromDraw.Id });
    }

    [Fact]
    public void Shuffle_UnknownPile_IsInvalid()
    {
        // arrange
        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = progress.Game.GetArtifact<Deck>("deck-1");
        deck.Should().NotBeNull();

        // act
        Action act = () => progress.HandleEvent(new ShuffleDeckEvent(deck!, "unknown"));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }

    [Fact]
    public void Discard_EmptyList_IsNoOp()
    {
        // arrange
        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = progress.Game.GetArtifact<Deck>("deck-1");
        deck.Should().NotBeNull();
        var before = progress.State.GetState<DeckState>(deck!);
        before.Should().NotBeNull();

        // act
        var after = progress.HandleEvent(new DiscardCardsEvent(deck!, CardsGameBuilder.Piles.Discard, Array.Empty<Card>()));

        // assert
        var ds = after.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        ds!.Piles[CardsGameBuilder.Piles.Discard].Count.Should().Be(before!.Piles[CardsGameBuilder.Piles.Discard].Count);
        ds!.Piles[CardsGameBuilder.Piles.Draw].Count.Should().Be(before!.Piles[CardsGameBuilder.Piles.Draw].Count);
    }
}