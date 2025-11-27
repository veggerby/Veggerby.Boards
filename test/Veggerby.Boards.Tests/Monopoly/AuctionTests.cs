using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Monopoly.States;

namespace Veggerby.Boards.Tests.Monopoly;

public class AuctionTests
{
    [Fact]
    public void CreateAuctionState_InitiallyInactive()
    {
        // arrange & act
        var auctionState = new AuctionState();

        // assert
        auctionState.IsActive.Should().BeFalse();
        auctionState.PropertyPosition.Should().Be(-1);
        auctionState.CurrentBid.Should().Be(0);
        auctionState.HighestBidderId.Should().BeNull();
    }

    [Fact]
    public void StartAuction_SetsCorrectProperties()
    {
        // arrange
        var auctionState = new AuctionState();
        var playerIds = new[] { "player-1", "player-2", "player-3" };

        // act
        var newState = auctionState.StartAuction(1, playerIds);

        // assert
        newState.IsActive.Should().BeTrue();
        newState.PropertyPosition.Should().Be(1);
        newState.CurrentBid.Should().Be(0);
        newState.HighestBidderId.Should().BeNull();
        newState.EligiblePlayers.Should().HaveCount(3);
        newState.PassedPlayers.Should().BeEmpty();
    }

    [Fact]
    public void PlaceBid_UpdatesCurrentBidAndBidder()
    {
        // arrange
        var auctionState = new AuctionState()
            .StartAuction(1, new[] { "player-1", "player-2" });

        // act
        var newState = auctionState.PlaceBid("player-1", 100);

        // assert
        newState.CurrentBid.Should().Be(100);
        newState.HighestBidderId.Should().Be("player-1");
    }

    [Fact]
    public void PlaceBid_MultipleRounds_HighestWins()
    {
        // arrange
        var auctionState = new AuctionState()
            .StartAuction(1, new[] { "player-1", "player-2" });

        // act
        var afterBid1 = auctionState.PlaceBid("player-1", 50);
        var afterBid2 = afterBid1.PlaceBid("player-2", 75);
        var afterBid3 = afterBid2.PlaceBid("player-1", 100);

        // assert
        afterBid3.CurrentBid.Should().Be(100);
        afterBid3.HighestBidderId.Should().Be("player-1");
    }

    [Fact]
    public void PlaceBid_BelowCurrent_ThrowsException()
    {
        // arrange
        var auctionState = new AuctionState()
            .StartAuction(1, new[] { "player-1", "player-2" })
            .PlaceBid("player-1", 100);

        // act & assert
        Action act = () => auctionState.PlaceBid("player-2", 50);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PassBid_AddsPlayerToPassedList()
    {
        // arrange
        var auctionState = new AuctionState()
            .StartAuction(1, new[] { "player-1", "player-2", "player-3" })
            .PlaceBid("player-1", 50);

        // act
        var afterPass = auctionState.PassBid("player-2");

        // assert
        afterPass.PassedPlayers.Should().Contain("player-2");
        afterPass.IsActive.Should().BeTrue(); // Still active, one other bidder remains
    }

    [Fact]
    public void Auction_AllPlayersPassedExceptWinner_BecomesInactive()
    {
        // arrange
        var auctionState = new AuctionState()
            .StartAuction(1, new[] { "player-1", "player-2", "player-3" })
            .PlaceBid("player-1", 100)
            .PassBid("player-2")
            .PassBid("player-3");

        // assert - auction is set to inactive when all but winner have passed
        auctionState.IsActive.Should().BeFalse();
        auctionState.HighestBidderId.Should().Be("player-1");
        auctionState.CurrentBid.Should().Be(100);
    }

    [Fact]
    public void Auction_AllPlayersPassedNoBid_BecomesInactive()
    {
        // arrange
        var auctionState = new AuctionState()
            .StartAuction(1, new[] { "player-1", "player-2" })
            .PassBid("player-1")
            .PassBid("player-2");

        // assert - when all pass with no bids, auction becomes inactive but with no winner
        auctionState.IsActive.Should().BeFalse();
        auctionState.HighestBidderId.Should().BeNull();
    }

    [Fact]
    public void CanPlayerBid_EligiblePlayer_ReturnsTrue()
    {
        // arrange
        var auctionState = new AuctionState()
            .StartAuction(1, new[] { "player-1", "player-2" });

        // act
        var canBid = auctionState.CanPlayerBid("player-1");

        // assert
        canBid.Should().BeTrue();
    }

    [Fact]
    public void CanPlayerBid_PassedPlayer_ReturnsFalse()
    {
        // arrange
        var auctionState = new AuctionState()
            .StartAuction(1, new[] { "player-1", "player-2" })
            .PassBid("player-1");

        // act
        var canBid = auctionState.CanPlayerBid("player-1");

        // assert
        canBid.Should().BeFalse();
    }

    [Fact]
    public void CanPlayerBid_NonEligiblePlayer_ReturnsFalse()
    {
        // arrange
        var auctionState = new AuctionState()
            .StartAuction(1, new[] { "player-1", "player-2" });

        // act
        var canBid = auctionState.CanPlayerBid("player-3");

        // assert
        canBid.Should().BeFalse();
    }

    [Fact]
    public void EndAuction_ReturnsInactiveState()
    {
        // arrange
        var auctionState = new AuctionState()
            .StartAuction(1, new[] { "player-1", "player-2" })
            .PlaceBid("player-1", 100);

        // act
        var endedState = auctionState.EndAuction();

        // assert
        endedState.IsActive.Should().BeFalse();
        endedState.PropertyPosition.Should().Be(-1);
    }

    [Fact]
    public void AuctionState_Equality_Works()
    {
        // arrange
        var state1 = new AuctionState().StartAuction(1, new[] { "player-1" });
        var state2 = new AuctionState().StartAuction(1, new[] { "player-1" });

        // act & assert
        state1.Equals(state2).Should().BeTrue();
    }

    [Fact]
    public void AuctionState_Inequality_Works()
    {
        // arrange
        var state1 = new AuctionState().StartAuction(1, new[] { "player-1" });
        var state2 = new AuctionState().StartAuction(2, new[] { "player-1" });

        // act & assert
        state1.Equals(state2).Should().BeFalse();
    }
}
