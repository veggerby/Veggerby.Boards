using System.Collections.Generic;

using Veggerby.Boards.Artifacts; // Player
using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events;
using Veggerby.Boards.States; // GameProgress

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingScoringTests
{
    private static (GameProgress progress, Deck? p1Deck, Deck? p2Deck, Player? p1, Player? p2) BuildGame()
    {
        var builder = new DeckBuildingGameBuilder();
        builder.WithCards("copper", "estate");
        var progress = builder.Compile();
        // Extract artifacts
        var p1 = progress.Game.GetPlayer("P1");
        var p2 = progress.Game.GetPlayer("P2");
        var p1Deck = progress.Game.GetArtifact<Deck>("p1-deck");
        var p2Deck = progress.Game.GetArtifact<Deck>("p2-deck");
        return (progress, p1Deck, p2Deck, p1, p2);
    }

    [Fact]
    public void GivenDefinitionsAndCards_WhenComputeScores_ThenVictoryPointsAggregated()
    {
        // arrange
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var (progress, p1Deck, p2Deck, p1, p2) = BuildGame();
        progress.Should().NotBeNull();
        p1Deck.Should().NotBeNull();
        p2Deck.Should().NotBeNull();
        p1.Should().NotBeNull();
        p2.Should().NotBeNull();
        // Register definitions (estate=1 VP, copper=0)
        progress = progress.HandleEvent(new RegisterCardDefinitionEvent("estate", "Estate", new List<string> { "Victory" }, 2, 1));
        progress = progress.HandleEvent(new RegisterCardDefinitionEvent("copper", "Copper", new List<string> { "Treasure" }, 0, 0));
        // Initialize decks: P1 has 2 estates, P2 has 1 estate
        var estate1 = progress.Game.GetArtifact<Card>("estate");
        var estate2 = progress.Game.GetArtifact<Card>("estate");
        estate1.Should().NotBeNull();
        estate2.Should().NotBeNull();
        progress = progress.HandleEvent(new CreateDeckEvent(p1Deck!, new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>{ estate1!, estate2! } },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        }));
        var estate3 = progress.Game.GetArtifact<Card>("estate");
        estate3.Should().NotBeNull();
        progress = progress.HandleEvent(new CreateDeckEvent(p2Deck!, new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>{ estate3! } },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        }));
        // Advance turn segments to End so cleanup phase can process scoring
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main));

        // act
        progress = progress.HandleEvent(new CleanupToDiscardEvent(p1Deck)); // no-op cleanup
        progress = progress.HandleEvent(new ComputeScoresEvent());

        // assert
        var scoreStates = progress.State.GetStates<ScoreState>();
        scoreStates.Should().NotBeNull();
        // We expect 2 players
        scoreStates.Should().HaveCount(2);
        scoreStates.Should().Contain(s => s.Artifact.Equals(p1) && s.VictoryPoints == 2);
        scoreStates.Should().Contain(s => s.Artifact.Equals(p2) && s.VictoryPoints == 1);
    }

    [Fact]
    public void GivenScoresAlreadyComputed_WhenComputeScoresAgain_ThenIgnored()
    {
        // arrange
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var (progress, p1Deck, _, _, _) = BuildGame();
        progress.Should().NotBeNull();
        p1Deck.Should().NotBeNull();
        progress = progress.HandleEvent(new RegisterCardDefinitionEvent("estate", "Estate", new List<string> { "Victory" }, 2, 1));
        var estate = progress.Game.GetArtifact<Card>("estate");
        estate.Should().NotBeNull();
        progress = progress.HandleEvent(new CreateDeckEvent(p1Deck!, new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>{ estate! } },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        }));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main));
        progress = progress.HandleEvent(new CleanupToDiscardEvent(p1Deck));
        progress = progress.HandleEvent(new ComputeScoresEvent());
        var scoredState = progress.State;

        // act
        progress = progress.HandleEvent(new ComputeScoresEvent());

        // assert
        progress.State.Should().BeSameAs(scoredState);
    }

    [Fact]
    public void GivenNoDefinitions_WhenComputeScores_ThenAllZero()
    {
        // arrange
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var (progress, p1Deck, p2Deck, p1, p2) = BuildGame();
        progress.Should().NotBeNull();
        p1Deck.Should().NotBeNull();
        p2Deck.Should().NotBeNull();
        p1.Should().NotBeNull();
        p2.Should().NotBeNull();
        // Decks with cards that lack definitions (should count 0)
        var estate = progress.Game.GetArtifact<Card>("estate");
        var copper = progress.Game.GetArtifact<Card>("copper");
        estate.Should().NotBeNull();
        copper.Should().NotBeNull();
        progress = progress.HandleEvent(new CreateDeckEvent(p1Deck!, new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>{ estate!, copper! } },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        }));
        var copper2 = progress.Game.GetArtifact<Card>("copper");
        copper2.Should().NotBeNull();
        progress = progress.HandleEvent(new CreateDeckEvent(p2Deck!, new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>{ copper2! } },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        }));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main));

        // act
        progress = progress.HandleEvent(new CleanupToDiscardEvent(p1Deck));
        progress = progress.HandleEvent(new ComputeScoresEvent());

        // assert
        var scores = progress.State.GetStates<ScoreState>();
        scores.Should().HaveCount(2);
        scores.Should().Contain(s => s.Artifact.Equals(p1) && s.VictoryPoints == 0);
        scores.Should().Contain(s => s.Artifact.Equals(p2) && s.VictoryPoints == 0);
    }
}