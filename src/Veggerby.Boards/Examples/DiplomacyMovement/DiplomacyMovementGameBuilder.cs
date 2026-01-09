using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions.Commitment;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Examples.DiplomacyMovement;

/// <summary>
/// Simplified Diplomacy-style game demonstrating simultaneous movement orders.
/// </summary>
/// <remarks>
/// This example shows:
/// 1. Players commit movement orders for their units simultaneously
/// 2. All orders are revealed together
/// 3. Conflicts (multiple units moving to same territory) are resolved by player order
/// 
/// Real Diplomacy includes:
/// - Support orders (units helping other units' moves)
/// - Hold orders (defensive positions)
/// - Convoy orders (naval transport)
/// - Retreat phases (after failed moves)
/// - Build/disband phases (reinforcement)
/// 
/// This simplified version focuses on demonstrating the commit/reveal pattern for
/// simultaneous movement with conflict resolution.
/// </remarks>
public sealed class DiplomacyMovementGameBuilder : GameBuilder
{
    /// <inheritdoc />
    protected override void Build()
    {
        BoardId = "diplomacy-movement";

        // Add three players (simplified from 7 in full Diplomacy)
        AddPlayer("england");
        AddPlayer("france");
        AddPlayer("germany");

        // Add simplified map (subset of Europe)
        // Real Diplomacy has ~70 territories; this uses 5 for demonstration
        AddDirection("adjacent");

        // England territories
        AddTile("london");
        AddTile("edinburgh").WithRelationTo("london").InDirection("adjacent");

        // France territories
        AddTile("paris").WithRelationTo("london").InDirection("adjacent");
        AddTile("marseilles").WithRelationTo("paris").InDirection("adjacent");

        // Germany territory
        AddTile("berlin").WithRelationTo("paris").InDirection("adjacent");

        // Add units for each player (simplified: one unit per player)
        AddPiece("england-army-1")
            .WithOwner("england")
            .HasDirection("adjacent");

        AddPiece("france-army-1")
            .WithOwner("france")
            .HasDirection("adjacent");

        AddPiece("germany-army-1")
            .WithOwner("germany")
            .HasDirection("adjacent");

        // Initial positions
        WithPiece("england-army-1").OnTile("london");
        WithPiece("france-army-1").OnTile("paris");
        WithPiece("germany-army-1").OnTile("berlin");

        // Commitment phase: players submit secret orders
        AddGamePhase("commitment")
            .If<CommitmentPhaseActiveCondition>()
            .Then()
                .ForEvent<CommitActionEvent>()
                    .If<CommitActionCondition>()
                .Then()
                    .Do<CommitActionStateMutator>();

        // Reveal phase: reveal and resolve all orders simultaneously
        AddGamePhase("reveal")
            .If<AllPlayersCommittedCondition>()
            .Then()
                .ForEvent<RevealCommitmentsEvent>()
                    .If<RevealCommitmentsCondition>()
                .Then()
                    .Do<RevealCommitmentsStateMutator>();

        // Movement phase: apply move orders after reveal
        AddGamePhase("movement")
            .If<NullGameStateCondition>()
            .Then()
                .ForEvent<MoveOrderEvent>()
                    .If<MoveOrderCondition>()
                .Then()
                    .Do<MoveOrderStateMutator>();
    }
}
