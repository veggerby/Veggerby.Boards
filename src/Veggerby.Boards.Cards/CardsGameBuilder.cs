using System;
using System.Collections.Generic;

using Veggerby.Boards.Cards.Mutators;
using Veggerby.Boards.Cards.Rules;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Cards;

/// <summary>
/// Minimal game builder wiring a single deck with basic phases and rules for cards operations.
/// </summary>
public class CardsGameBuilder : GameBuilder
{
    /// <summary>Predefined pile ids for the basic deck.</summary>
    public static class Piles
    {
        /// <summary>Draw pile identifier.</summary>
        public const string Draw = "draw";
        /// <summary>Discard pile identifier.</summary>
        public const string Discard = "discard";
        /// <summary>Hand pile identifier.</summary>
        public const string Hand = "hand";
        /// <summary>In-play pile identifier.</summary>
        public const string InPlay = "inplay";
    }

    /// <summary>Identifiers for demonstration cards.</summary>
    public static class CardIds
    {
        /// <summary>Demo card 1 id.</summary>
        public const string C1 = "card-1";
        /// <summary>Demo card 2 id.</summary>
        public const string C2 = "card-2";
        /// <summary>Demo card 3 id.</summary>
        public const string C3 = "card-3";
        /// <summary>Demo card 4 id.</summary>
        public const string C4 = "card-4";
        /// <summary>Demo card 5 id.</summary>
        public const string C5 = "card-5";
    }

    private Deck? _deck;
    private readonly Dictionary<string, IList<Card>> _initialPiles = new();

    /// <summary>
    /// Configures a simple deck with known card ids and four piles; initial pile is draw in ascending order.
    /// </summary>
    protected override void Build()
    {
        BoardId = "cards-basic"; // no tiles used

        // Minimal board topology to satisfy core invariant (Board requires at least one relation)
        AddDirection("N");
        AddTile("tile-a");
        AddTile("tile-b");
        WithTile("tile-a").WithRelationTo("tile-b").InDirection("N");

        // Minimal players to satisfy core invariant (Game requires players)
        AddPlayer("P1");
        AddPlayer("P2");

        // Use artifacts to host deck & cards (no board relations needed for this module)
        var pileIds = new[] { Piles.Draw, Piles.Discard, Piles.Hand, Piles.InPlay };
        var deck = new Deck("deck-1", pileIds);
        _deck = deck;
        AddArtifact(deck.Id).WithFactory(id => deck);

        var c1 = new Card(CardIds.C1); AddArtifact(c1.Id).WithFactory(id => c1);
        var c2 = new Card(CardIds.C2); AddArtifact(c2.Id).WithFactory(id => c2);
        var c3 = new Card(CardIds.C3); AddArtifact(c3.Id).WithFactory(id => c3);
        var c4 = new Card(CardIds.C4); AddArtifact(c4.Id).WithFactory(id => c4);
        var c5 = new Card(CardIds.C5); AddArtifact(c5.Id).WithFactory(id => c5);

        _initialPiles[Piles.Draw] = new List<Card> { c1, c2, c3, c4, c5 };
        _initialPiles[Piles.Discard] = new List<Card>();
        _initialPiles[Piles.Hand] = new List<Card>();
        _initialPiles[Piles.InPlay] = new List<Card>();

        // Single open phase accepting card operations
        AddGamePhase("cards-ops")
            .If<NullGameStateCondition>()
            .Then()
            .ForEvent<CreateDeckEvent>().If<CreateDeckEventCondition>().Then().Do<CreateDeckStateMutator>()
            .ForEvent<ShuffleDeckEvent>().If<ShuffleDeckEventCondition>().Then().Do<ShuffleDeckStateMutator>()
            .ForEvent<DrawCardsEvent>().If<DrawCardsEventCondition>().Then().Do<DrawCardsStateMutator>()
            .ForEvent<MoveCardsEvent>().If<MoveCardsEventCondition>().Then().Do<MoveCardsStateMutator>()
            .ForEvent<DiscardCardsEvent>().If<DiscardCardsEventCondition>().Then().Do<DiscardCardsStateMutator>();

        // Emit initial CreateDeck event via pre-processor? Not available here; instead users send event after Compile.
        // We keep builder minimal and deterministic.
    }

    /// <summary>
    /// Helper to build initial CreateDeckEvent payload.
    /// </summary>
    public CreateDeckEvent CreateInitialDeckEvent()
    {
        if (_deck is null)
        {
            throw new InvalidOperationException("Deck not initialized. Build must be invoked before calling this helper.");
        }
        return new CreateDeckEvent(_deck, _initialPiles);
    }
}