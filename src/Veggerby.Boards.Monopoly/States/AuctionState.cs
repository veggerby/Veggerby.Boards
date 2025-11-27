using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.States;

/// <summary>
/// Marker artifact for auction state.
/// </summary>
internal sealed class AuctionMarker : Artifact
{
    public static readonly AuctionMarker Instance = new();

    private AuctionMarker() : base("monopoly-auction") { }
}

/// <summary>
/// Represents a bid in an auction.
/// </summary>
public sealed record AuctionBid(string PlayerId, int Amount);

/// <summary>
/// Represents the state of a property auction.
/// </summary>
public sealed class AuctionState : IArtifactState
{
    /// <summary>
    /// Gets the position of the property being auctioned.
    /// </summary>
    public int PropertyPosition
    {
        get;
    }

    /// <summary>
    /// Gets the current highest bid amount.
    /// </summary>
    public int CurrentBid
    {
        get;
    }

    /// <summary>
    /// Gets the current highest bidder player ID (null if no bids yet).
    /// </summary>
    public string? HighestBidderId
    {
        get;
    }

    /// <summary>
    /// Gets the list of players who have passed in this auction.
    /// </summary>
    public IReadOnlyList<string> PassedPlayers
    {
        get;
    }

    /// <summary>
    /// Gets the list of all active players who can bid.
    /// </summary>
    public IReadOnlyList<string> EligiblePlayers
    {
        get;
    }

    /// <summary>
    /// Gets whether the auction is active.
    /// </summary>
    public bool IsActive
    {
        get;
    }

    /// <inheritdoc />
    public Artifact Artifact => AuctionMarker.Instance;

    /// <summary>
    /// Initializes a new AuctionState with no active auction.
    /// </summary>
    public AuctionState()
    {
        PropertyPosition = -1;
        CurrentBid = 0;
        HighestBidderId = null;
        PassedPlayers = Array.Empty<string>();
        EligiblePlayers = Array.Empty<string>();
        IsActive = false;
    }

    /// <summary>
    /// Initializes a new AuctionState with the specified parameters.
    /// </summary>
    public AuctionState(
        int propertyPosition,
        int currentBid,
        string? highestBidderId,
        IReadOnlyList<string> passedPlayers,
        IReadOnlyList<string> eligiblePlayers,
        bool isActive)
    {
        PropertyPosition = propertyPosition;
        CurrentBid = currentBid;
        HighestBidderId = highestBidderId;
        PassedPlayers = passedPlayers;
        EligiblePlayers = eligiblePlayers;
        IsActive = isActive;
    }

    /// <summary>
    /// Starts a new auction for a property.
    /// </summary>
    public AuctionState StartAuction(int propertyPosition, IEnumerable<string> eligiblePlayerIds)
    {
        var players = eligiblePlayerIds.ToList().AsReadOnly();

        return new AuctionState(
            propertyPosition,
            currentBid: 0,
            highestBidderId: null,
            passedPlayers: Array.Empty<string>(),
            eligiblePlayers: players,
            isActive: true);
    }

    /// <summary>
    /// Places a bid in the auction.
    /// </summary>
    public AuctionState PlaceBid(string playerId, int bidAmount)
    {
        if (bidAmount <= CurrentBid)
        {
            throw new InvalidOperationException($"Bid must be higher than current bid of ${CurrentBid}");
        }

        return new AuctionState(
            PropertyPosition,
            currentBid: bidAmount,
            highestBidderId: playerId,
            passedPlayers: PassedPlayers,
            eligiblePlayers: EligiblePlayers,
            isActive: true);
    }

    /// <summary>
    /// Records a player passing on the auction.
    /// </summary>
    public AuctionState PassBid(string playerId)
    {
        var newPassedPlayers = PassedPlayers.Append(playerId).ToList().AsReadOnly();

        // Check if auction should end
        var remainingBidders = EligiblePlayers.Where(p => !newPassedPlayers.Contains(p)).ToList();

        // Auction ends when:
        // 1. All players have passed (no bids made)
        // 2. Only the highest bidder remains (all others passed)
        var auctionEnding = remainingBidders.Count == 0 ||
                            (remainingBidders.Count == 1 && string.Equals(remainingBidders[0], HighestBidderId, StringComparison.Ordinal));

        return new AuctionState(
            PropertyPosition,
            CurrentBid,
            HighestBidderId,
            passedPlayers: newPassedPlayers,
            eligiblePlayers: EligiblePlayers,
            isActive: !auctionEnding);
    }

    /// <summary>
    /// Ends the auction.
    /// </summary>
    public AuctionState EndAuction()
    {
        return new AuctionState();
    }

    /// <summary>
    /// Gets whether a player can still bid.
    /// </summary>
    public bool CanPlayerBid(string playerId)
    {
        return IsActive &&
               EligiblePlayers.Contains(playerId) &&
               !PassedPlayers.Contains(playerId);
    }

    /// <summary>
    /// Gets whether all players except the winner have passed.
    /// </summary>
    public bool ShouldEnd()
    {
        if (!IsActive)
        {
            return false;
        }

        var remainingBidders = EligiblePlayers.Where(p => !PassedPlayers.Contains(p)).ToList();

        // End if everyone passed (no bids) or only the highest bidder remains
        if (remainingBidders.Count == 0)
        {
            return true;
        }

        if (remainingBidders.Count == 1 && string.Equals(remainingBidders[0], HighestBidderId, StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool Equals(IArtifactState other)
    {
        if (other is not AuctionState auctionState)
        {
            return false;
        }

        return PropertyPosition == auctionState.PropertyPosition &&
               CurrentBid == auctionState.CurrentBid &&
               string.Equals(HighestBidderId, auctionState.HighestBidderId, StringComparison.Ordinal) &&
               IsActive == auctionState.IsActive &&
               PassedPlayers.SequenceEqual(auctionState.PassedPlayers) &&
               EligiblePlayers.SequenceEqual(auctionState.EligiblePlayers);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is AuctionState auctionState && Equals(auctionState);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(PropertyPosition, CurrentBid, HighestBidderId, IsActive);
    }
}
