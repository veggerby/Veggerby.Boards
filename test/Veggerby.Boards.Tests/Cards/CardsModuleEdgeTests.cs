using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Cards;

using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.Events;
using Veggerby.Boards.Cards.States;
namespace Veggerby.Boards.Tests.Cards;

public class CardsModuleEdgeTests
{
    [Fact]
    public void Draw_MoreThanAvailable_IsInvalid()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = progress.Game.GetArtifact<Deck>("deck-1");
        deck.Should().NotBeNull();

        // act
        Action act = () => progress.HandleEvent(new DrawCardsEvent(deck!, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 100));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }

    [Fact]
    public void Shuffle_SingleCard_NoOp()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = progress.Game.GetArtifact<Deck>("deck-1");
        deck.Should().NotBeNull();
        // move 4 to hand, leaving 1 in draw
        progress = progress.HandleEvent(new MoveCardsEvent(deck!, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 4));
        var before = progress.State.GetState<DeckState>(deck!);
        before.Should().NotBeNull();
        var beforeIds = before!.Piles[CardsGameBuilder.Piles.Draw].Select(c => c.Id).ToArray();

        // act
        progress = progress.HandleEvent(new ShuffleDeckEvent(deck!, CardsGameBuilder.Piles.Draw));

        // assert
        var after = progress.State.GetState<DeckState>(deck!);
        after.Should().NotBeNull();
        after!.Piles[CardsGameBuilder.Piles.Draw].Select(c => c.Id).Should().Equal(beforeIds);
    }

    [Fact]
    public void DeckState_Equality_And_HashCode_Work()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        var create = builder.CreateInitialDeckEvent();
        var deck = create.Deck;
        var after = progress.HandleEvent(create).State;
        var ds1 = after.GetState<DeckState>(deck);
        ds1.Should().NotBeNull();

        // act
        var ds2 = after.GetState<DeckState>(deck);
        ds2.Should().NotBeNull();

        // assert
        ds1!.Equals(ds2).Should().BeTrue();
        ds1.GetHashCode().Should().Be(ds2!.GetHashCode());
    }

    [Fact]
    public void MoveCardsEvent_CountConstructor_Validates()
    {
        // arrange

        // act

        // assert

        var deck = new Deck("d", new[] { "draw", "hand" });

        // act
        Action act = () => new MoveCardsEvent(deck, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, -1);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DiscardCardsEvent_Constructor_Validates()
    {
        // arrange

        // act

        // assert

        var deck = new Deck("d", new[] { "discard" });

        // act
        Action act = () => new DiscardCardsEvent(deck, CardsGameBuilder.Piles.Discard, null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deck_Equality_BasedOnIdAndType()
    {
        // arrange

        // act

        // assert

        var d1 = new Deck("same", new[] { "p" });
        var d2 = new Deck("same", new[] { "p2" });
        var d3 = new Deck("other", new[] { "p" });

        // assert
        d1.Equals(d2).Should().BeTrue();
        d1.Equals(d3).Should().BeFalse();
    }

    [Fact]
    public void Deck_Requires_Unique_Piles()
    {
        // arrange

        // act

        // assert

        var dupPiles = new[] { "draw", "draw" };

        // act
        Action act = () => new Deck("d", dupPiles);

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deck_Requires_AtLeastOnePile()
    {
        // arrange

        // act

        // assert

        var empty = Array.Empty<string>();

        // act
        Action act = () => new Deck("d", empty);

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeckState_MissingRequiredPile_Throws()
    {
        // arrange

        // act

        // assert

        var deck = new Deck("d", new[] { "draw", "discard" });
        var piles = new Dictionary<string, IList<Card>>(StringComparer.Ordinal)
        {
            { "draw", new List<Card>() }
            // discard missing
        };

        // act
        Action act = () => new DeckState(deck, piles);

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Move_ByCount_Insufficient_Throws()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = progress.Game.GetArtifact<Deck>("deck-1");
        deck.Should().NotBeNull();

        // act
        Action act = () => progress.HandleEvent(new MoveCardsEvent(deck!, CardsGameBuilder.Piles.Draw, CardsGameBuilder.Piles.Hand, 10));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }

    [Fact]
    public void Discard_CardNotPresent_Throws()
    {
        // arrange

        // act

        // assert

        var builder = new CardsGameBuilder();
        var progress = builder.Compile();
        progress = progress.HandleEvent(builder.CreateInitialDeckEvent());
        var deck = progress.Game.GetArtifact<Deck>("deck-1");
        deck.Should().NotBeNull();
        var fake = new Card("non-existent");

        // act
        Action act = () => progress.HandleEvent(new DiscardCardsEvent(deck!, CardsGameBuilder.Piles.Discard, new[] { fake }));

        // assert
        act.Should().Throw<InvalidGameEventException>();
    }
}
