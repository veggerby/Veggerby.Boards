using BenchmarkDotNet.Attributes;

using Veggerby.Boards.Cards; // Deck & Card artifacts
using Veggerby.Boards.DeckBuilding; // Deck-building builders & events
using Veggerby.Boards.Events; // EndTurnSegmentEvent, EndGameEvent, ComputeScoresEvent
using Veggerby.Boards.States; // TurnSegment enum

namespace Veggerby.Boards.Benchmarks;

/// <summary>
/// Benchmarks deck-building condition overhead for GainFromSupply and EndGame (with and without depletion satisfied).
/// Focus is on per-event dispatch cost rather than end-to-end game throughput.
/// </summary>
[MemoryDiagnoser]
public class DeckBuildingConditionsBenchmark
{
    private GameProgress _progress = null!;
    private GameProgress _progressAltEnd = null!;
    private GainFromSupplyEvent _gain = null!;
    private GainFromSupplyEvent _gainInvalidPile = null!;
    private readonly EndGameEvent _end = new();

    [GlobalSetup]
    public void Setup()
    {
        // Base game without alternate end trigger
        var builder = new DeckBuildingGameBuilder();
        builder.WithCard("c1");
        _progress = builder.Compile();
        var deck = _progress.Game.GetArtifact<Deck>("p1-deck") ?? throw new InvalidOperationException("DeckBuildingConditions: deck p1-deck missing");
        var p1 = _progress.Game.GetPlayer("P1") ?? throw new InvalidOperationException("DeckBuildingConditions: player P1 missing");

        // Deck state with supply containing one card
        var piles = new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        };
        var supply = new Dictionary<string, int> { { "c1", 1 } };
        _progress = _progress.HandleEvent(new CreateDeckEvent(deck, piles, supply));
        _progress = _progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        _gain = new GainFromSupplyEvent(p1, deck, "c1", DeckBuildingGameBuilder.Piles.Discard);
        _gainInvalidPile = new GainFromSupplyEvent(p1, deck, "c1", "missing");

        // Alternate end trigger build (threshold 1 so empty supply triggers immediately)
        var builderAlt = new DeckBuildingGameBuilder().WithEndTrigger(new DeckBuildingEndTriggerOptions(emptySupplyPilesThreshold: 1));
        builderAlt.WithCard("c2");
        _progressAltEnd = builderAlt.Compile();
        var deck2 = _progressAltEnd.Game.GetArtifact<Deck>("p1-deck") ?? throw new InvalidOperationException("DeckBuildingConditions: deck2 p1-deck missing");
        var p1b = _progressAltEnd.Game.GetPlayer("P1") ?? throw new InvalidOperationException("DeckBuildingConditions: player P1 missing (alt end)");
        var piles2 = new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        };
        var supply2 = new Dictionary<string, int> { { "c2", 0 } }; // empty triggers threshold
        _progressAltEnd = _progressAltEnd.HandleEvent(new CreateDeckEvent(deck2, piles2, supply2));
        _progressAltEnd = _progressAltEnd.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        _progressAltEnd = _progressAltEnd.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main));
        // Scores required for validation
        _progressAltEnd = _progressAltEnd.HandleEvent(new ComputeScoresEvent());
    }

    [Benchmark]
    public GameProgress GainFromSupply_Valid()
    {
        var gp = _progress;
        gp = gp.HandleEvent(_gain);
        return gp;
    }

    [Benchmark]
    public GameProgress GainFromSupply_FailUnknownPile()
    {
        var gp = _progress;
        try
        {
            gp = gp.HandleEvent(_gainInvalidPile);
        }
        catch (InvalidGameEventException) { }
        return gp;
    }

    [Benchmark]
    public GameProgress EndGame_NoAlternateTrigger_NoOp()
    {
        // Without options configured and no threshold reached this should ignore when invoked early.
        var gp = _progress;
        gp = gp.HandleEvent(_end); // ignored (scores not yet computed)
        return gp;
    }

    [Benchmark]
    public GameProgress EndGame_AlternateTrigger_DepletionValid()
    {
        var gp = _progressAltEnd;
        gp = gp.HandleEvent(_end); // should validate and end game
        return gp;
    }
}