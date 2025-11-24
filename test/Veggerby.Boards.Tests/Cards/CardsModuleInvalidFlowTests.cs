using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards;

using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.Events;
using Veggerby.Boards.Cards.States;
namespace Veggerby.Boards.Tests.Cards;

public class CardsModuleInvalidFlowTests
{
    [Fact]
    public void CreateDeck_MissingRequiredPile_IsInvalid()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        var deck = new Deck("d", new[] { CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand });
        var c1 = new Card("x1");
        var c2 = new Card("x2");
        var piles = new Dictionary<string, IList<Card>>(StringComparer.Ordinal)
        {
            { CardsGameBuilder.Piles.Draw, new List<Card> { c1, c2 } }
            // Missing required 'hand' or other piles -> invalid
        };
        var evt = new CreateDeckEvent(deck, piles);

        // act
        Action act = () => progress.HandleEvent(evt);

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }

    [Fact]
    public void Move_Explicit_IncludesCardNotInSource_IsInvalid()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = progress.Game.GetArtifact<Deck>("deck-1");
        deck.Should().NotBeNull();
        var ds = progress.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        var draw = ds!.Piles[CardsGameBuilder.Piles.Draw];
        var notInDraw = ds.Piles[CardsGameBuilder.Piles.Hand]; // empty, so craft a fake
        var fake = new Card("not-in-draw");

        // act
        Action act = () => progress.HandleEvent(new MoveCardsEvent(deck!, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, new List<Card> { draw[0], fake }));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }

    [Fact]
    public void Discard_ToUnknownPile_IsInvalid()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = progress.Game.GetArtifact<Deck>("deck-1");
        deck.Should().NotBeNull();
        var ds = progress.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        var card = ds!.Piles[CardsGameBuilder.Piles.Draw][0];

        // act
        Action act = () => progress.HandleEvent(new DiscardCardsEvent(deck!, "unknown", new[] { card }));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }
}
