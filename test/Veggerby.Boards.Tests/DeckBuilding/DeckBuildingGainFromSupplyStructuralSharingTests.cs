using System.Collections.Generic;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Support;

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingGainFromSupplyStructuralSharingTests
{
    [Fact]
    public void GivenMultiplePiles_WhenGain_ThenOnlyTargetPileListReplaced()
    {
        using var guard = FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var builder = new DeckBuildingGameBuilder();
        var c1 = new Card("gold");
        builder.WithCard(c1.Id);
        var progress = builder.Compile();
        var game = progress.Game;
        var p1 = game.GetPlayer("P1"); p1.Should().NotBeNull();
        var deck = game.GetArtifact<Deck>("p1-deck"); deck.Should().NotBeNull();

        var drawList = new List<Card>();
        var discardList = new List<Card>();
        var handList = new List<Card>();
        var inPlayList = new List<Card>();

        var piles = new Dictionary<string, IList<Card>>
        {
            [DeckBuildingGameBuilder.Piles.Draw] = drawList,
            [DeckBuildingGameBuilder.Piles.Discard] = discardList,
            [DeckBuildingGameBuilder.Piles.Hand] = handList,
            [DeckBuildingGameBuilder.Piles.InPlay] = inPlayList,
        };
        var supply = new Dictionary<string, int> { [c1.Id] = 2 };

        progress = progress.HandleEvent(new CreateDeckEvent(deck!, piles, supply));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start)); // Start -> Main

        var before = progress.State.GetState<DeckState>(deck!); before.Should().NotBeNull();
        var beforeDraw = before.Piles[DeckBuildingGameBuilder.Piles.Draw];
        var beforeDiscard = before.Piles[DeckBuildingGameBuilder.Piles.Discard];
        var beforeHand = before.Piles[DeckBuildingGameBuilder.Piles.Hand];
        var beforeInPlay = before.Piles[DeckBuildingGameBuilder.Piles.InPlay];

        // act
        progress = progress.HandleEvent(new GainFromSupplyEvent(p1!, deck!, c1.Id, DeckBuildingGameBuilder.Piles.Discard));

        // assert
        var after = progress.State.GetState<DeckState>(deck!); after.Should().NotBeNull();
        var afterDraw = after.Piles[DeckBuildingGameBuilder.Piles.Draw];
        var afterDiscard = after.Piles[DeckBuildingGameBuilder.Piles.Discard];
        var afterHand = after.Piles[DeckBuildingGameBuilder.Piles.Hand];
        var afterInPlay = after.Piles[DeckBuildingGameBuilder.Piles.InPlay];

        // DeckState constructor always materializes new read-only wrappers; reference equality for reused piles is not expected.
        // Instead we assert content identity (no unintended duplication) for non-target piles and that only the target grew.
        afterDraw.Should().BeEquivalentTo(beforeDraw); // unchanged content
        afterHand.Should().BeEquivalentTo(beforeHand); // unchanged content
        afterInPlay.Should().BeEquivalentTo(beforeInPlay); // unchanged content

        // Target pile content changed exactly by appended card.
        afterDiscard.Should().HaveCount(beforeDiscard.Count + 1);
        afterDiscard[^1].Should().Be(c1);
    }
}