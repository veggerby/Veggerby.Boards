using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Cards;
using Veggerby.Boards.Cards.Artifacts;
using Veggerby.Boards.Cards.Events;
using Veggerby.Boards.Cards.States;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.DeckBuilding.Artifacts;
using Veggerby.Boards.DeckBuilding.Events;
using Veggerby.Boards.Events;
using Veggerby.Boards.States;
namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingAlternateEndTriggerTests
{
    private static (GameProgress progress, Deck? deck) BuildGame(DeckBuildingEndTriggerOptions options)
    {
        var builder = new DeckBuildingGameBuilder();
        if (options is not null)
        {
            builder.WithEndTrigger(options);
        }
        builder.WithCards("estate", "duchy", "province");
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");
        return (progress, deck);
    }

    private static GameProgress BootstrapWithSupply(GameProgress progress, Deck deck, IDictionary<string, int> supply)
    {
        // Register definitions
        foreach (var kv in supply)
        {
            progress = progress.HandleEvent(new RegisterCardDefinitionEvent(kv.Key, kv.Key, new List<string> { "Victory" }, 1, 1));
        }
        // Create deck (empty piles except draw) with supply counts
        var piles = new Dictionary<string, IList<Card>>{
            { DeckBuildingGameBuilder.Piles.Draw, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Discard, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.Hand, new List<Card>() },
            { DeckBuildingGameBuilder.Piles.InPlay, new List<Card>() },
        };
        progress = progress.HandleEvent(new CreateDeckEvent(deck, piles, supply));
        // Advance through setup and action/buy to cleanup for scoring/end evaluation
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main)); // action
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Main)); // buy
        progress = progress.HandleEvent(new CleanupToDiscardEvent(deck));
        // compute scores (needed for end game)
        progress = progress.HandleEvent(new ComputeScoresEvent());
        return progress;
    }

    [Fact]
    public void GivenThresholdSatisfied_WhenEndGameEvent_ThenEnds()
    {
        // arrange

        // act

        // assert

        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var (progress, deck) = BuildGame(new DeckBuildingEndTriggerOptions(emptySupplyPilesThreshold: 2));
        progress.Should().NotBeNull();
        deck.Should().NotBeNull();
        var supply = new Dictionary<string, int> { { "estate", 0 }, { "duchy", 0 }, { "province", 5 } };
        progress = BootstrapWithSupply(progress, deck, supply);

        // act
        progress = progress.HandleEvent(new EndGameEvent());

        // assert
        progress.State.GetStates<GameEndedState>().Should().HaveCount(1);
    }

    [Fact]
    public void GivenThresholdNotSatisfied_WhenEndGameEvent_ThenIgnored()
    {
        // arrange

        // act

        // assert

        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var (progress, deck) = BuildGame(new DeckBuildingEndTriggerOptions(emptySupplyPilesThreshold: 3));
        progress.Should().NotBeNull();
        deck.Should().NotBeNull();
        var supply = new Dictionary<string, int> { { "estate", 0 }, { "duchy", 0 }, { "province", 5 } }; // only 2 empty < 3
        progress = BootstrapWithSupply(progress, deck, supply);
        var before = progress.State;

        // act
        progress = progress.HandleEvent(new EndGameEvent());

        // assert
        progress.State.Should().BeSameAs(before);
    }

    [Fact]
    public void GivenKeyPileEmpty_WhenEndGameEvent_ThenEnds()
    {
        // arrange

        // act

        // assert

        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var (progress, deck) = BuildGame(new DeckBuildingEndTriggerOptions(keyPileCardIds: new[] { "province" }));
        progress.Should().NotBeNull();
        deck.Should().NotBeNull();
        var supply = new Dictionary<string, int> { { "estate", 5 }, { "duchy", 5 }, { "province", 0 } };
        progress = BootstrapWithSupply(progress, deck, supply);

        // act
        progress = progress.HandleEvent(new EndGameEvent());

        // assert
        progress.State.GetStates<GameEndedState>().Should().HaveCount(1);
    }

    [Fact]
    public void GivenKeyPileNotEmpty_WhenEndGameEvent_ThenIgnored()
    {
        // arrange

        // act

        // assert

        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var (progress, deck) = BuildGame(new DeckBuildingEndTriggerOptions(keyPileCardIds: new[] { "province" }));
        progress.Should().NotBeNull();
        deck.Should().NotBeNull();
        var supply = new Dictionary<string, int> { { "estate", 0 }, { "duchy", 0 }, { "province", 1 } }; // province not empty
        progress = BootstrapWithSupply(progress, deck, supply);
        var before = progress.State;

        // act
        progress = progress.HandleEvent(new EndGameEvent());

        // assert
        progress.State.Should().BeSameAs(before);
    }

    [Fact]
    public void GivenCombined_AnySatisfied_AllowsEnd()
    {
        // arrange

        // act

        // assert

        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var (progress, deck) = BuildGame(new DeckBuildingEndTriggerOptions(emptySupplyPilesThreshold: 3, keyPileCardIds: new[] { "province" }));
        progress.Should().NotBeNull();
        deck.Should().NotBeNull();
        var supply = new Dictionary<string, int> { { "estate", 0 }, { "duchy", 0 }, { "province", 0 } }; // key pile empty & threshold satisfied
        progress = BootstrapWithSupply(progress, deck, supply);

        // act
        progress = progress.HandleEvent(new EndGameEvent());

        // assert
        progress.State.GetStates<GameEndedState>().Should().HaveCount(1);
    }

    [Fact]
    public void Determinism_SameSupplyConfig_YieldsSameOutcome()
    {
        // arrange

        // act

        // assert

        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        var opts = new DeckBuildingEndTriggerOptions(emptySupplyPilesThreshold: 2, keyPileCardIds: new[] { "province" });
        var supply = new Dictionary<string, int> { { "estate", 0 }, { "duchy", 0 }, { "province", 5 } }; // threshold satisfied, key not

        var (p1, d1) = BuildGame(opts);
        p1.Should().NotBeNull();
        d1.Should().NotBeNull();
        p1 = BootstrapWithSupply(p1, d1, supply);
        var before1 = p1.State;
        p1 = p1.HandleEvent(new EndGameEvent());

        var (p2, d2) = BuildGame(opts);
        p2.Should().NotBeNull();
        d2.Should().NotBeNull();
        p2 = BootstrapWithSupply(p2, d2, supply);
        var before2 = p2.State;
        p2 = p2.HandleEvent(new EndGameEvent());

        // act & assert
        p1.State.GetStates<GameEndedState>().Count().Should().Be(1);
        p2.State.GetStates<GameEndedState>().Count().Should().Be(1);
        // ensure transitions differ from pre-end state identically (one new GameEndedState)
        before1.GetStates<GameEndedState>().Should().BeEmpty();
        before2.GetStates<GameEndedState>().Should().BeEmpty();
    }

    [Fact]
    public void Options_Invalid_WhenNoThresholdAndNoKeys()
    {
        // arrange

        // act

        // assert

        var act = () => new DeckBuildingEndTriggerOptions(0, null);

        // assert
        act.Should().Throw<ArgumentException>().WithMessage("*threshold > 0 or key pile ids non-empty*");
    }

    [Fact]
    public void Options_Valid_WhenThresholdOnly()
    {
        // arrange

        // act

        // assert

        new DeckBuildingEndTriggerOptions(1, null).Should().NotBeNull();
    }

    [Fact]
    public void Options_Valid_WhenKeysOnly()
    {
        // arrange

        // act

        // assert

        new DeckBuildingEndTriggerOptions(0, new[] { "kp" }).Should().NotBeNull();
    }
}
