using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions.Commitment;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Examples.SealedBidAuction;

/// <summary>
/// Game builder for a sealed-bid auction demonstrating simultaneous commitment and reveal mechanics.
/// </summary>
public sealed class SealedBidAuctionGameBuilder : GameBuilder
{
    /// <inheritdoc />
    protected override void Build()
    {
        BoardId = "sealed-bid-auction";

        // Add players
        AddPlayer("player-1");
        AddPlayer("player-2");
        AddPlayer("player-3");

        // Add minimal board structure (required by engine)
        AddDirection("forward");
        AddTile("tile-1").WithRelationTo("tile-2").InDirection("forward");
        AddTile("tile-2");

        // Commitment phase: players commit their bids
        AddGamePhase("commitment")
            .If<CommitmentPhaseActiveCondition>()
            .Then()
                .ForEvent<CommitActionEvent>()
                    .If<CommitActionCondition>()
                .Then()
                    .Do<CommitActionStateMutator>();

        // Reveal phase: reveal and apply all bids
        AddGamePhase("reveal")
            .If<AllPlayersCommittedCondition>()
            .Then()
                .ForEvent<RevealCommitmentsEvent>()
                    .If<RevealCommitmentsCondition>()
                .Then()
                    .Do<RevealCommitmentsStateMutator>();

        // Bid application phase: record bids after reveal
        AddGamePhase("bid-application")
            .If<NullGameStateCondition>()
            .Then()
                .ForEvent<PlaceBidEvent>()
                    .If<PlaceBidCondition>()
                .Then()
                    .Do<PlaceBidStateMutator>();
    }
}
