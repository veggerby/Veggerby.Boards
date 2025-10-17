using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Support; // TurnStateAssertions

namespace Veggerby.Boards.Tests.DeckBuilding;

// Tests covering phase gating by TurnSegment for deck-building module
public class DeckBuildingTurnSegmentGatingTests
{
    private static (GameEngine engine, GameProgress progress, Deck p1Deck, Deck p2Deck, Player p1) BuildGame()
    {
        var builder = new DeckBuildingGameBuilder();
        // minimal cards used in tests
        builder.WithCards("c1", "c2", "c3", "c4", "c5", "c6");
        var compiled = builder.Compile();
        var p1Deck = compiled.Game.GetArtifact<Deck>("p1-deck");
        var p2Deck = compiled.Game.GetArtifact<Deck>("p2-deck");
        p1Deck.Should().NotBeNull();
        p2Deck.Should().NotBeNull();
        Player? p1 = null; foreach (var pl in compiled.Game.Players) { p1 = pl; break; }
        p1.Should().NotBeNull();
        return (compiled.Engine, compiled, p1Deck!, p2Deck!, p1!);
    }

    private static IDictionary<string, IList<Card>> MakeSimplePiles(Game game)
    {
        var c1 = game.GetArtifact<Card>("c1"); c1.Should().NotBeNull();
        var c2 = game.GetArtifact<Card>("c2"); c2.Should().NotBeNull();
        var c3 = game.GetArtifact<Card>("c3"); c3.Should().NotBeNull();
        return new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card> { c1!, c2!, c3! },
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card>()
        };
    }

    [Fact]
    public void GivenStartSegment_WhenActionPhaseEventSubmitted_ThenIgnored()
    {
        using var guard = FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var build = BuildGame();
        var progress = build.progress; var p1Deck = build.p1Deck;
        p1Deck.Should().NotBeNull();
        var piles = MakeSimplePiles(progress.Game);
        var supply = new Dictionary<string, int>();
        var afterCreate = progress.HandleEvent(new CreateDeckEvent(p1Deck!, piles, supply));
        afterCreate.ShouldHaveSingleTurnState();
        afterCreate.State.GetState<DeckState>(p1Deck).Should().NotBeNull();
        var stateBefore = afterCreate.State; // should still be Start segment

        // act
        var afterDraw = afterCreate.HandleEvent(new DrawWithReshuffleEvent(p1Deck!, 1));

        // assert
        ReferenceEquals(afterDraw.State, stateBefore).Should().BeTrue(); // ignored -> no new snapshot
    }

    [Fact]
    public void GivenMainSegment_WhenCleanupEventSubmitted_ThenIgnored()
    {
        using var guard = FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var build = BuildGame();
        var progress = build.progress; var p1Deck = build.p1Deck;
        p1Deck.Should().NotBeNull();
        var piles = MakeSimplePiles(progress.Game);
        var afterCreate = progress.HandleEvent(new CreateDeckEvent(p1Deck!, piles, new Dictionary<string, int>()));
        afterCreate.ShouldHaveSingleTurnState();
        afterCreate.State.GetState<DeckState>(p1Deck).Should().NotBeNull();
        // advance from Start -> Main (EndTurnSegment(Start) triggers transition to next segment via mutator)
        var afterEndStart = afterCreate.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        var stateBefore = afterEndStart.State;

        // act
        var afterCleanup = afterEndStart.HandleEvent(new CleanupToDiscardEvent(p1Deck!));

        // assert
        ReferenceEquals(afterCleanup.State, stateBefore).Should().BeTrue(); // ignored in Main segment
    }

    [Fact]
    public void GivenMainSegment_WhenActionEventSubmitted_ThenProcessed()
    {
        using var guard = FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var build = BuildGame();
        var progress = build.progress; var p1Deck = build.p1Deck;
        p1Deck.Should().NotBeNull();
        var piles = MakeSimplePiles(progress.Game);
        var afterCreate = progress.HandleEvent(new CreateDeckEvent(p1Deck!, piles, new Dictionary<string, int>()));
        afterCreate.ShouldHaveSingleTurnState();
        afterCreate.State.GetState<DeckState>(p1Deck).Should().NotBeNull();
        var afterEndStart = afterCreate.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start)); // to Main
        afterEndStart.ShouldBeSegment(TurnSegment.Main); // verify segment advanced
        var stateBefore = afterEndStart.State;

        // act
        var afterDraw = afterEndStart.HandleEvent(new DrawWithReshuffleEvent(p1Deck!, 2));

        // assert
        ReferenceEquals(afterDraw.State, stateBefore).Should().BeFalse(); // processed -> new snapshot
        var deckState = afterDraw.State.GetState<DeckState>(p1Deck!);
        deckState.Should().NotBeNull();
        deckState!.Piles[DeckBuildingGameBuilder.Piles.Hand].Count.Should().Be(2);
    }

    [Fact]
    public void GivenEndSegment_WhenGainEventSubmitted_ThenIgnored()
    {
        using var guard = FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var build = BuildGame();
        var progress = build.progress; var p1Deck = build.p1Deck; var p1 = build.p1;
        p1Deck.Should().NotBeNull();
        p1.Should().NotBeNull();
        var piles = MakeSimplePiles(progress.Game);
        var supply = new Dictionary<string, int> { { "c4", 5 } };
        var afterCreate = progress.HandleEvent(new CreateDeckEvent(p1Deck!, piles, supply));
        afterCreate.ShouldHaveSingleTurnState();
        afterCreate.State.GetState<DeckState>(p1Deck).Should().NotBeNull();
        var afterEndStart = afterCreate.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start)); // Start -> Main
        var afterEndMain = afterEndStart.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main)); // Main -> End
        var stateBefore = afterEndMain.State;

        // act
        var afterGain = afterEndMain.HandleEvent(new GainFromSupplyEvent(p1!, p1Deck!, "c4", DeckBuildingGameBuilder.Piles.Discard));

        // assert
        ReferenceEquals(afterGain.State, stateBefore).Should().BeTrue();
    }
}