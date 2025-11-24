using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Support; using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.Events;
using Veggerby.Boards.Cards.States;
// TurnStateAssertions

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingCleanupToDiscardTests
{
    [Fact]
    public void GivenCardsInHandAndInPlay_WhenCleanup_ThenAllMoveToDiscardAndSourcesCleared()
    {
        // arrange

        // act

        // assert

        using var guard = FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var builder = new DeckBuildingGameBuilder();
        builder.WithSeed(123UL);
        var c1 = new Card("a");
        var c2 = new Card("b");
        var c3 = new Card("c");
        var c4 = new Card("d");
        builder.WithCard(c1.Id);
        builder.WithCard(c2.Id);
        builder.WithCard(c3.Id);
        builder.WithCard(c4.Id);
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");
        deck.Should().NotBeNull();

        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card> { },
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card> { c1, c2 },
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card> { c3, c4 },
        };
        var supply = new Dictionary<string, int>();
        progress = progress.HandleEvent(new CreateDeckEvent(deck!, piles, supply));
        progress.ShouldHaveSingleTurnState();
        progress.State.GetState<DeckState>(deck).Should().NotBeNull();

        // act
        // advance Start -> Main -> End (cleanup gated on End segment)
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start)); // Start -> Main
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main));  // Main -> End
        progress = progress.HandleEvent(new CleanupToDiscardEvent(deck));

        // assert
        var ds = progress.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        ds!.Piles[DeckBuildingGameBuilder.Piles.Discard].Should().ContainInOrder(new[] { c1, c2, c3, c4 });
        ds.Piles[DeckBuildingGameBuilder.Piles.Hand].Count.Should().Be(0);
        ds.Piles[DeckBuildingGameBuilder.Piles.InPlay].Count.Should().Be(0);
    }

    [Fact]
    public void GivenNoCardsInHandOrInPlay_WhenCleanup_ThenNoOp()
    {
        // arrange

        // act

        // assert

        using var guard = FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var builder = new DeckBuildingGameBuilder();
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");
        deck.Should().NotBeNull();
        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = new List<Card>(),
            [DeckBuildingGameBuilder.Piles.Discard] = new List<Card> { },
            [DeckBuildingGameBuilder.Piles.Hand] = new List<Card> { },
            [DeckBuildingGameBuilder.Piles.InPlay] = new List<Card> { },
        };
        var supply2 = new Dictionary<string, int>();
        progress = progress.HandleEvent(new CreateDeckEvent(deck!, piles, supply2));
        progress.ShouldHaveSingleTurnState();
        progress.State.GetState<DeckState>(deck).Should().NotBeNull();

        // act
        // advance Start -> Main -> End
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start)); // Start -> Main
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main));  // Main -> End
        var updated = progress.HandleEvent(new CleanupToDiscardEvent(deck!));

        // assert
        // No-op path must return the original state instance per engine invariants
        ReferenceEquals(updated.State, progress.State).Should().BeTrue();
        var ds = updated.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        ds!.Piles[DeckBuildingGameBuilder.Piles.Discard].Count.Should().Be(0);
        ds.Piles[DeckBuildingGameBuilder.Piles.Hand].Count.Should().Be(0);
        ds.Piles[DeckBuildingGameBuilder.Piles.InPlay].Count.Should().Be(0);
    }
}
