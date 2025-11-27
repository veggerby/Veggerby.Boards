using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Monopoly.States;

namespace Veggerby.Boards.Tests.Monopoly;

public class TradeTests
{
    [Fact]
    public void CreateTradeProposalState_InitiallyInactive()
    {
        // arrange & act
        var tradeState = new TradeProposalState();

        // assert
        tradeState.IsActive.Should().BeFalse();
        tradeState.ProposerOffer.Should().BeNull();
        tradeState.TargetOffer.Should().BeNull();
    }

    [Fact]
    public void CreateTradeOffer_WithCashOnly()
    {
        // arrange & act
        var offer = new TradeOffer("player-1", cash: 500);

        // assert
        offer.PlayerId.Should().Be("player-1");
        offer.Cash.Should().Be(500);
        offer.PropertyPositions.Should().BeEmpty();
        offer.GetOutOfJailCard.Should().BeFalse();
    }

    [Fact]
    public void CreateTradeOffer_WithProperties()
    {
        // arrange & act
        var offer = new TradeOffer("player-1", propertyPositions: new[] { 1, 3 });

        // assert
        offer.PropertyPositions.Should().HaveCount(2);
        offer.PropertyPositions.Should().Contain(1);
        offer.PropertyPositions.Should().Contain(3);
    }

    [Fact]
    public void CreateTradeOffer_WithGetOutOfJailCard()
    {
        // arrange & act
        var offer = new TradeOffer("player-1", getOutOfJailCard: true);

        // assert
        offer.GetOutOfJailCard.Should().BeTrue();
    }

    [Fact]
    public void Propose_CreatesActiveTradeState()
    {
        // arrange
        var tradeState = new TradeProposalState();
        var proposerOffer = new TradeOffer("player-1", cash: 200);
        var targetOffer = new TradeOffer("player-2", propertyPositions: new[] { 1 });

        // act
        var newState = tradeState.Propose(proposerOffer, targetOffer);

        // assert
        newState.IsActive.Should().BeTrue();
        newState.ProposerOffer.Should().NotBeNull();
        newState.TargetOffer.Should().NotBeNull();
        newState.ProposerOffer!.PlayerId.Should().Be("player-1");
        newState.TargetOffer!.PlayerId.Should().Be("player-2");
    }

    [Fact]
    public void Cancel_ReturnsInactiveState()
    {
        // arrange
        var proposerOffer = new TradeOffer("player-1", cash: 200);
        var targetOffer = new TradeOffer("player-2", propertyPositions: new[] { 1 });
        var tradeState = new TradeProposalState(proposerOffer, targetOffer);

        // act
        var cancelledState = tradeState.Cancel();

        // assert
        cancelledState.IsActive.Should().BeFalse();
    }

    [Fact]
    public void TradeOffer_WithAllOptions()
    {
        // arrange & act
        var offer = new TradeOffer(
            "player-1",
            cash: 100,
            propertyPositions: new[] { 1, 3, 5 },
            getOutOfJailCard: true);

        // assert
        offer.PlayerId.Should().Be("player-1");
        offer.Cash.Should().Be(100);
        offer.PropertyPositions.Should().HaveCount(3);
        offer.GetOutOfJailCard.Should().BeTrue();
    }

    [Fact]
    public void TradeProposalState_Equality_Works()
    {
        // arrange
        var proposerOffer = new TradeOffer("player-1", cash: 200);
        var targetOffer = new TradeOffer("player-2", propertyPositions: new[] { 1 });
        var state1 = new TradeProposalState(proposerOffer, targetOffer);
        var state2 = new TradeProposalState(proposerOffer, targetOffer);

        // act & assert
        state1.Equals(state2).Should().BeTrue();
    }

    [Fact]
    public void TradeProposalState_InactiveEquality_Works()
    {
        // arrange
        var state1 = new TradeProposalState();
        var state2 = new TradeProposalState();

        // act & assert
        state1.Equals(state2).Should().BeTrue();
    }

    [Fact]
    public void TradeProposalState_Constructor_SetsActive()
    {
        // arrange
        var proposerOffer = new TradeOffer("player-1", cash: 200);
        var targetOffer = new TradeOffer("player-2", cash: 100);

        // act
        var tradeState = new TradeProposalState(proposerOffer, targetOffer);

        // assert
        tradeState.IsActive.Should().BeTrue();
        tradeState.ProposerOffer.Should().Be(proposerOffer);
        tradeState.TargetOffer.Should().Be(targetOffer);
    }

    [Fact]
    public void TradeOffer_NullPlayerId_ThrowsException()
    {
        // arrange
        string? nullPlayerId = null;

        // act & assert
        Action act = () => new TradeOffer(nullPlayerId!, cash: 100);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PropertySwap_BothSidesOffer()
    {
        // arrange
        var proposerOffer = new TradeOffer("player-1", propertyPositions: new[] { 1 });
        var targetOffer = new TradeOffer("player-2", propertyPositions: new[] { 6 });

        // act
        var tradeState = new TradeProposalState(proposerOffer, targetOffer);

        // assert
        tradeState.IsActive.Should().BeTrue();
        tradeState.ProposerOffer!.PropertyPositions.Should().HaveCount(1);
        tradeState.TargetOffer!.PropertyPositions.Should().HaveCount(1);
        tradeState.ProposerOffer.PropertyPositions[0].Should().Be(1);
        tradeState.TargetOffer.PropertyPositions[0].Should().Be(6);
    }
}
