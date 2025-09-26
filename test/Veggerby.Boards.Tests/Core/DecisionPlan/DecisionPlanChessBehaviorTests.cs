using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

/// <summary>
/// Direct (non-parity) behavior test for Chess exercising a small opening under the DecisionPlan executor.
/// Assumes DecisionPlan will become the canonical path (no feature flag toggling here).
/// </summary>
public class DecisionPlanChessBehaviorTests
{
    [Fact]
    public void GivenSimpleOpening_WhenApplyingMoves_ThenExpectedPiecePositionsPersist()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();

        // act
        progress = progress.Move("white-pawn-5", "e3");
        progress = progress.Move("black-pawn-5", "e6");
        progress = progress.Move("white-knight-2", "f3");

        // assert
        var pieces = progress.State.GetStates<PieceState>().ToDictionary(p => p.Artifact.Id, p => p.CurrentTile.Id);
        pieces["white-pawn-5"].Should().Be("tile-e3");
        pieces["black-pawn-5"].Should().Be("tile-e6");
        pieces["white-knight-2"].Should().Be("tile-f3");
    }
}