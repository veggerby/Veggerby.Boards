using Veggerby.Boards.Cards;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Flows.Rules.Conditions.Turn;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions; // for NullGameStateCondition

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Minimal game builder wiring scaffolding for deck-building core on top of the Cards module.
/// </summary>
public class DeckBuildingGameBuilder : GameBuilder
{
    private Deck _p1Deck;
    private Deck _p2Deck;

    /// <summary>Standard pile identifiers for player decks.</summary>
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

    /// <summary>
    /// Registers a concrete card artifact with the given id into the game being built.
    /// Must be called before <see cref="GameBuilder.Compile"/>.
    /// </summary>
    /// <param name="cardId">Stable card identifier.</param>
    public void WithCard(string cardId)
    {
        var card = new Card(cardId);
        AddArtifact(card.Id).WithFactory(_ => card);
    }

    /// <summary>
    /// Registers multiple concrete card artifacts with the given ids into the game being built.
    /// Must be called before <see cref="GameBuilder.Compile"/>.
    /// </summary>
    /// <param name="cardIds">Stable card identifiers.</param>
    public void WithCards(params string[] cardIds)
    {
        if (cardIds is null)
        {
            return;
        }

        for (var i = 0; i < cardIds.Length; i++)
        {
            var id = cardIds[i];
            if (!string.IsNullOrEmpty(id))
            {
                WithCard(id);
            }
        }
    }

    /// <summary>
    /// Builds minimal topology and players to satisfy core invariants.
    /// </summary>
    protected override void Build()
    {
        BoardId = "deckbuilding-core";

        // Minimal board topology (no tiles needed by logic but required by core invariants)
        AddDirection("N");
        AddTile("db-tile-a");
        AddTile("db-tile-b");
        WithTile("db-tile-a").WithRelationTo("db-tile-b").InDirection("N");

        // Minimal players
        var p1 = AddPlayer("P1");
        var p2 = AddPlayer("P2");

        // Player decks (share pile identifiers)
        var pileIds = new[] { Piles.Draw, Piles.Discard, Piles.Hand, Piles.InPlay };
        _p1Deck = new Deck("p1-deck", pileIds);
        _p2Deck = new Deck("p2-deck", pileIds);
        AddArtifact(_p1Deck.Id).WithFactory(_ => _p1Deck);
        AddArtifact(_p2Deck.Id).WithFactory(_ => _p2Deck);

        // Minimal card definitions + concrete cards (supply will be set by callers via events later)
        // Project scope: keep builder lean; supply/state created through events in tests or higher-level builders.

        // Phases
        // Setup: deck initialization + segment advancement (Start->Main)
        AddGamePhase("db-setup")
            .If<TurnSegmentStartCondition>()
            .Then()
            .ForEvent<CreateDeckEvent>().If<DeckBuildingCreateDeckEventCondition>().Then().Do<DeckBuildingCreateDeckStateMutator>()
            .ForEvent<EndTurnSegmentEvent>().If<DbEndTurnSegmentAlwaysCondition>().Then().Do<DbTurnAdvanceStateMutator>();

        // Action phase (subset of former main) + advancement (Main->End)
        AddGamePhase("db-action")
            .If<TurnSegmentMainCondition>()
            .Then()
            .ForEvent<DrawWithReshuffleEvent>().If<DrawWithReshuffleEventCondition>().Then().Do<DrawWithReshuffleStateMutator>()
            .ForEvent<TrashFromHandEvent>().If<TrashFromHandEventCondition>().Then().Do<TrashFromHandStateMutator>()
            .ForEvent<EndTurnSegmentEvent>().If<DbEndTurnSegmentAlwaysCondition>().Then().Do<DbTurnAdvanceStateMutator>();

        // Buy phase (supply gains) shares Main segment – ordering after action phase provides deterministic separation
        AddGamePhase("db-buy")
            .If<TurnSegmentMainCondition>()
            .Then()
            .ForEvent<GainFromSupplyEvent>().If<GainFromSupplyEventCondition>().Then().Do<GainFromSupplyStateMutator>()
            .ForEvent<EndTurnSegmentEvent>().If<DbEndTurnSegmentAlwaysCondition>().Then().Do<DbTurnAdvanceStateMutator>();

        // Cleanup: end of turn cleanup + advancement (End->next Start)
        AddGamePhase("db-cleanup")
            .If<TurnSegmentEndCondition>()
            .Then()
            .ForEvent<CleanupToDiscardEvent>().If<CleanupToDiscardEventCondition>().Then().Do<CleanupToDiscardStateMutator>()
            .ForEvent<EndTurnSegmentEvent>().If<DbEndTurnSegmentAlwaysCondition>().Then().Do<DbTurnAdvanceStateMutator>();
    }
}