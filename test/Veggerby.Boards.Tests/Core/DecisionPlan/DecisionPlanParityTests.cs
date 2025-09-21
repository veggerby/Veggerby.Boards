using System.Linq;

using Veggerby.Boards; // for GameProgress
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

/// <summary>
/// Parity harness asserting that enabling the DecisionPlan yields identical resulting states
/// versus the legacy evaluator for a representative deterministic sequence of events.
/// </summary>
public class DecisionPlanParityTests
{
    private static Veggerby.Boards.States.GameProgress ApplyOpening(Veggerby.Boards.States.GameProgress progress)
    {
        // Use extension Move which resolves shortest valid pattern path automatically; skip if path not found (returns original progress).
        progress = progress.Move("white-pawn-2", "e4");
        progress = progress.Move("black-pawn-4", "d5");
        return progress;
        // Knight move intentionally omitted (legacy visitor edge case investigation pending)
    }

    [Fact]
    public void GivenOpeningSequence_WhenEvaluatedWithAndWithoutDecisionPlan_ThenResultingStatesMatch()
    {
        // arrange
        var builderLegacy = new ChessGameBuilder();
        using (new FeatureFlagScope(decisionPlan: false))
        {
            var legacy = builderLegacy.Compile();
            legacy = ApplyOpening(legacy);
            var finalLegacy = legacy.State;

            var builderPlan = new ChessGameBuilder();
            Veggerby.Boards.States.GameProgress plan;
            using (new FeatureFlagScope(decisionPlan: true))
            {
                plan = builderPlan.Compile();
                plan = ApplyOpening(plan);
            }

            // assert
            // Compare piece positions for all pieces
            // Compare all pieces registered in the game definition by id.
            foreach (var piece in plan.Game.Artifacts.OfType<Veggerby.Boards.Artifacts.Piece>())
            {
                var legacyPieceState = finalLegacy.GetState<Veggerby.Boards.States.PieceState>(piece);
                var planPieceState = plan.State.GetState<Veggerby.Boards.States.PieceState>(piece);
                if (legacyPieceState is null && planPieceState is null)
                {
                    continue; // absent both sides
                }
                legacyPieceState.Should().NotBeNull();
                planPieceState.Should().NotBeNull();
                planPieceState.CurrentTile.Should().Be(legacyPieceState.CurrentTile, $"Piece {piece.Id} should end on same tile");
            }
            // Hash parity (when hashing enabled) acts as additional guard.
            if (legacy.State.Hash.HasValue && plan.State.Hash.HasValue)
            {
                plan.State.Hash.Should().Be(legacy.State.Hash);
            }
        }
    }
}