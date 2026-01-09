using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions.Commitment;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Examples.RockPaperScissors;

/// <summary>
/// Game builder for Rock-Paper-Scissors demonstrating simultaneous commitment and reveal mechanics.
/// </summary>
public sealed class RockPaperScissorsGameBuilder : GameBuilder
{
    /// <inheritdoc />
    protected override void Build()
    {
        BoardId = "rock-paper-scissors";

        // Add two players
        AddPlayer("player-1");
        AddPlayer("player-2");

        // Add minimal board structure (required by engine)
        AddDirection("forward");
        AddTile("tile-1").WithRelationTo("tile-2").InDirection("forward");
        AddTile("tile-2");

        // Commitment phase: players commit their choices
        AddGamePhase("commitment")
            .If<CommitmentPhaseActiveCondition>()
            .Then()
                .ForEvent<CommitActionEvent>()
                    .If<CommitActionCondition>()
                .Then()
                    .Do<CommitActionStateMutator>();

        // Reveal phase: reveal and apply all choices
        AddGamePhase("reveal")
            .If<AllPlayersCommittedCondition>()
            .Then()
                .ForEvent<RevealCommitmentsEvent>()
                    .If<RevealCommitmentsCondition>()
                .Then()
                    .Do<RevealCommitmentsStateMutator>();

        // Choice application phase: record choices after reveal
        AddGamePhase("choice-application")
            .If<NullGameStateCondition>()
            .Then()
                .ForEvent<SelectChoiceEvent>()
                    .If<SelectChoiceCondition>()
                .Then()
                    .Do<SelectChoiceStateMutator>();
    }
}
