using System.Collections.Generic;

using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events; // EndTurnSegmentEvent
using Veggerby.Boards.States;

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Isolates raw condition evaluation cost for GainFromSupplyEventCondition (valid vs failing) without mutator execution.
/// Useful to separate gating overhead from state copy allocations.
/// </summary>
[MemoryDiagnoser]
public class DeckBuildingConditionOnlyBenchmark
{
    private GameEngine _engine = null!;
    private GameState _state = null!;
    private GameState _stateMissingPile = null!;
    private GameState _stateMissingCardArtifact = null!;
    private readonly GainFromSupplyEventCondition _condition = new();
    private GainFromSupplyEvent _validEvent = null!;
    private GainFromSupplyEvent _badPileEvent = null!;
    private GainFromSupplyEvent _badCardEvent = null!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new DeckBuildingGameBuilder();
        builder.WithCard("c1");
        var progress = builder.Compile();
        _engine = progress.Engine;
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");
        var p1 = progress.Game.GetPlayer("P1");

        var piles = new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        };
        var supply = new Dictionary<string, int> { { "c1", 1 } };
        progress = progress.HandleEvent(new CreateDeckEvent(deck, piles, supply));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        _state = progress.State;

        // State with missing pile (remove Discard)
        var pilesMissing = new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        };
        var supply2 = new Dictionary<string, int> { { "c1", 1 } };
        var deckStateMissing = new DeckState(deck, pilesMissing, supply2);
        _stateMissingPile = _state.Next([deckStateMissing]);

        // State referencing unknown card artifact in supply
        var supplyGhost = new Dictionary<string, int> { { "ghost", 1 } };
        var deckStateGhost = new DeckState(deck, piles, supplyGhost);
        _stateMissingCardArtifact = _state.Next([deckStateGhost]);

        _validEvent = new GainFromSupplyEvent(p1, deck, "c1", DeckBuildingGameBuilder.Piles.Discard);
        _badPileEvent = new GainFromSupplyEvent(p1, deck, "c1", "unknown");
        _badCardEvent = new GainFromSupplyEvent(p1, deck, "ghost", DeckBuildingGameBuilder.Piles.Discard);
    }

    [Benchmark]
    public ConditionResponse GainFromSupply_Valid_ConditionOnly() => _condition.Evaluate(_engine, _state, _validEvent);

    [Benchmark]
    public ConditionResponse GainFromSupply_Fail_UnknownPile_ConditionOnly() => _condition.Evaluate(_engine, _state, _badPileEvent);

    [Benchmark]
    public ConditionResponse GainFromSupply_Fail_UnknownCard_ConditionOnly() => _condition.Evaluate(_engine, _stateMissingCardArtifact, _badCardEvent);
}