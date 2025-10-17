using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingRuleGuardsTests
{
    [Fact]
    public void GainFromSupply_NotApplicable_WhenNoDeckState()
    {
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var builder = new DeckBuildingGameBuilder();
        builder.WithCard("c1");
        var progress = builder.Compile();
        var p1 = progress.Game.GetPlayer("P1"); p1.Should().NotBeNull();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck"); deck.Should().NotBeNull();

        // act
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        var before = progress.State;
        var after = progress.HandleEvent(new GainFromSupplyEvent(p1!, deck!, "c1", DeckBuildingGameBuilder.Piles.Discard));

        // assert - ignored silently via NotApplicable (state unchanged)
        after.State.Should().BeSameAs(before);
    }

    [Fact]
    public void GainFromSupply_Fails_WhenUnknownPile()
    {
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var builder = new DeckBuildingGameBuilder();
        builder.WithCard("c1");
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck"); deck.Should().NotBeNull();
        var p1 = progress.Game.GetPlayer("P1"); p1.Should().NotBeNull();

        // minimal deck state with supply set
        var piles = new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        };
        var supply = new Dictionary<string, int> { { "c1", 1 } };
        progress = progress.HandleEvent(new CreateDeckEvent(deck!, piles, supply));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));

        var act = () => progress.HandleEvent(new GainFromSupplyEvent(p1!, deck!, "c1", "unknown-pile"));
        act.Should().Throw<InvalidGameEventException>().WithMessage("*Unknown pile*");
    }

    [Fact]
    public void GainFromSupply_Fails_WhenCardArtifactMissing()
    {
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var builder = new DeckBuildingGameBuilder();
        // Intentionally do NOT register card artifact
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck"); deck.Should().NotBeNull();
        var p1 = progress.Game.GetPlayer("P1"); p1.Should().NotBeNull();

        var piles = new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        };
        var supply = new Dictionary<string, int> { { "ghost", 1 } }; // supply references unknown card artifact
        progress = progress.HandleEvent(new CreateDeckEvent(deck!, piles, supply));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));

        var act = () => progress.HandleEvent(new GainFromSupplyEvent(p1!, deck!, "ghost", DeckBuildingGameBuilder.Piles.Discard));
        act.Should().Throw<InvalidGameEventException>().WithMessage("*Unknown card id*");
    }

    [Fact]
    public void EndGame_Ignored_WhenScoresMissing_AndOptionsPresent()
    {
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var builder = new DeckBuildingGameBuilder().WithEndTrigger(new DeckBuildingEndTriggerOptions(emptySupplyPilesThreshold: 1));
        builder.WithCard("c1");
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck"); deck.Should().NotBeNull();

        // supply with one empty pile condition satisfied, but no scores computed yet
        var piles = new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        };
        var supply = new Dictionary<string, int> { { "c1", 0 } }; // empty triggers threshold
        progress = progress.HandleEvent(new CreateDeckEvent(deck!, piles, supply));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main));
        var before = progress.State;
        progress = progress.HandleEvent(new EndGameEvent());
        progress.State.Should().BeSameAs(before); // ignored due to missing scores prerequisite
    }

    [Fact]
    public void EndGame_Ignored_WhenAlreadyEnded_AndOptionsPresent()
    {
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var builder = new DeckBuildingGameBuilder().WithEndTrigger(new DeckBuildingEndTriggerOptions(emptySupplyPilesThreshold: 1));
        builder.WithCard("c1");
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck"); deck.Should().NotBeNull();

        var piles = new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        };
        var supply = new Dictionary<string, int> { { "c1", 0 } }; // depletion satisfied
        progress = progress.HandleEvent(new CreateDeckEvent(deck!, piles, supply));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main));
        progress = progress.HandleEvent(new CleanupToDiscardEvent(deck));
        progress = progress.HandleEvent(new ComputeScoresEvent());
        progress = progress.HandleEvent(new EndGameEvent()); // first end -> valid
        var before = progress.State;
        progress = progress.HandleEvent(new EndGameEvent()); // second end -> ignored
        progress.State.Should().BeSameAs(before);
    }
}