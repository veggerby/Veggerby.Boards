using System;

using Veggerby.Boards.Cards;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.DeckBuilding;

public class DeckBuildingSupplyConfiguratorTests
{
    [Fact]
    public void BuildStartupEvents_Definitions_And_Supply_Applied_InInsertionOrder()
    {
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var builder = new DeckBuildingGameBuilder();
        builder.ConfigureSupply(c => c
            .AddDefinition("copper", "Copper", new[] { "Treasure" }, 0, 0)
            .AddDefinition("estate", "Estate", new[] { "Victory" }, 2, 1)
            .AddSupply("copper", 60)
            .AddSupply("estate", 24));
        builder.WithCards("copper", "estate");
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");
        deck.Should().NotBeNull();

        // act
        builder.SupplyConfigurator.Should().NotBeNull();
        var events = builder.SupplyConfigurator!.BuildStartupEvents(deck!);
        foreach (var e in events)
        {
            progress = progress.HandleEvent(e);
        }

        // assert
        var ds = progress.State.GetState<DeckState>(deck!);
        ds.Should().NotBeNull();
        ds!.Supply["copper"].Should().Be(60);
        ds.Supply["estate"].Should().Be(24);
    }

    [Fact]
    public void AddDefinition_Duplicate_Throws()
    {
        // arrange
        var cfg = new DeckBuildingSupplyConfigurator();
        cfg.AddDefinition("copper", "Copper", new[] { "Treasure" }, 0, 0);

        // act
        var act = () => cfg.AddDefinition("copper", "Copper2", new[] { "Treasure" }, 1, 0);

        // assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddSupply_ForUndefinedDefinition_Throws()
    {
        // arrange
        var cfg = new DeckBuildingSupplyConfigurator();

        // act
        var act = () => cfg.AddSupply("copper", 10);

        // assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Integration_GainFromSupply_DecrementsAfterStartup()
    {
        using var guard = Support.FeatureFlagGuard.ForceTurnSequencing(true);
        // arrange
        var builder = new DeckBuildingGameBuilder();
        builder.ConfigureSupply(c => c
            .AddDefinition("copper", "Copper", new[] { "Treasure" }, 0, 0)
            .AddSupply("copper", 5));
        builder.WithCards("copper");
        var progress = builder.Compile();
        var deck = progress.Game.GetArtifact<Deck>("p1-deck");
        deck.Should().NotBeNull();
        builder.SupplyConfigurator.Should().NotBeNull();
        var events = builder.SupplyConfigurator!.BuildStartupEvents(deck!);
        foreach (var e in events)
        {
            progress = progress.HandleEvent(e);
        }
        var enumerator = progress.Game.Players.GetEnumerator();
        enumerator.MoveNext().Should().BeTrue();
        var player = enumerator.Current;
        player.Should().NotBeNull();
        progress = progress.HandleEvent(new EndTurnSegmentEvent(TurnSegment.Start));

        // act
        progress = progress.HandleEvent(new GainFromSupplyEvent(player!, deck!, "copper", DeckBuildingGameBuilder.Piles.Discard));

        // assert
        var ds = progress.State.GetState<DeckState>(deck)!;
        ds.Supply["copper"].Should().Be(4);
        ds.Piles[DeckBuildingGameBuilder.Piles.Discard].Count.Should().Be(1);
    }
}