using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Examples.SealedBidAuction;

/// <summary>
/// Represents the outcome of a sealed-bid auction.
/// </summary>
public sealed class AuctionOutcome : IGameOutcome
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuctionOutcome"/> class.
    /// </summary>
    /// <param name="winner">The winning player (highest bidder).</param>
    /// <param name="winningBid">The winning bid amount.</param>
    /// <param name="allBids">All bids placed in the auction.</param>
    public AuctionOutcome(
        Player winner,
        int winningBid,
        IReadOnlyDictionary<Player, int> allBids)
    {
        Winner = winner;
        WinningBid = winningBid;
        AllBids = allBids;
    }

    /// <summary>
    /// Gets the winning player (highest bidder).
    /// </summary>
    public Player Winner { get; }

    /// <summary>
    /// Gets the winning bid amount.
    /// </summary>
    public int WinningBid { get; }

    /// <summary>
    /// Gets all bids placed in the auction.
    /// </summary>
    public IReadOnlyDictionary<Player, int> AllBids { get; }

    /// <inheritdoc />
    public string TerminalCondition => "HighestBid";

    /// <summary>
    /// Determines the winner of a sealed-bid auction based on player bids.
    /// </summary>
    /// <param name="state">The game state containing player bids.</param>
    /// <returns>The auction outcome.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no bids are found.</exception>
    public static AuctionOutcome DetermineWinner(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var bids = state.GetStates<PlayerBidState>().ToList();
        if (!bids.Any())
        {
            throw new InvalidOperationException("No bids found");
        }

        // Build dictionary of all bids
        var allBids = new Dictionary<Player, int>();
        foreach (var bid in bids)
        {
            allBids[(Player)bid.Artifact] = bid.BidAmount;
        }

        // Find highest bid (deterministic tie-breaking by player ID ascending)
        var highestBid = bids.Max(b => b.BidAmount);
        var winners = bids.Where(b => b.BidAmount == highestBid).ToList();

        // Deterministic tie-breaking: use player ID order
        var winner = winners.OrderBy(w => w.Artifact.Id, StringComparer.Ordinal).First();

        return new AuctionOutcome((Player)winner.Artifact, highestBid, allBids);
    }

    /// <inheritdoc />
    public IReadOnlyList<PlayerResult> PlayerResults
    {
        get
        {
            // Sort all bids by amount descending, then by player ID for ties
            var sortedBids = AllBids
                .OrderByDescending(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key.Id, StringComparer.Ordinal)
                .ToList();

            var results = new List<PlayerResult>();
            var currentRank = 1;
            int? previousBid = null;

            for (int i = 0; i < sortedBids.Count; i++)
            {
                var bid = sortedBids[i];

                // Update rank only when bid amount changes
                if (previousBid.HasValue && bid.Value < previousBid.Value)
                {
                    currentRank = i + 1;
                }

                // Only the first player with the highest bid is marked as winner
                // (deterministic tie-breaking by player ID)
                var outcome = bid.Key.Equals(Winner)
                    ? OutcomeType.Win
                    : OutcomeType.Loss;

                results.Add(new PlayerResult
                {
                    Player = bid.Key,
                    Outcome = outcome,
                    Rank = currentRank,
                    Metrics = new Dictionary<string, object>
                    {
                        ["BidAmount"] = bid.Value
                    }
                });

                previousBid = bid.Value;
            }

            return results;
        }
    }
}
