using System.Collections.Generic;

using AwesomeAssertions;

using Veggerby.Boards.Artifacts; // Player
using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events;
using Veggerby.Boards.States; // GameProgress

using Xunit;

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingTerminationTests
{
    private static (GameProgress progress, Deck deck) BuildGame()
    {
        var builder = new DeckBuildingGameBuilder();
        builder.WithCards("estate");
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");
        return (progress, deck);
    }

    [Fact]
    public void GivenScoresComputedAndMaxTurnsReached_WhenEndGameEvent_ThenGameEndedStateAdded()
    {
        // arrange
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var (progress, deck) = BuildGame();
        // Register definition and create deck
        progress = progress.HandleEvent(new RegisterCardDefinitionEvent("estate", "Estate", new List<string> { "Victory" }, 2, 1));
        progress = progress.HandleEvent(new CreateDeckEvent(deck, new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>{ progress.Game.GetArtifact<Card>("estate") } },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        }));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main));
        progress = progress.HandleEvent(new CleanupToDiscardEvent(deck));
        progress = progress.HandleEvent(new ComputeScoresEvent());

        // act
        progress = progress.HandleEvent(new EndGameEvent());

        // assert
        progress.State.GetStates<GameEndedState>().Should().HaveCount(1);
    }

    [Fact]
    public void GivenNoScores_WhenEndGameEvent_ThenIgnored()
    {
        // arrange
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var (progress, deck) = BuildGame();
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main));
        progress = progress.HandleEvent(new CleanupToDiscardEvent(deck));

        // act
        var before = progress.State;
        progress = progress.HandleEvent(new EndGameEvent());

        // assert
        progress.State.Should().BeSameAs(before);
    }
}