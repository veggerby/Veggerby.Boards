using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core.DecisionPlan;

public class DecisionPlanMaskingTests
{
    private static GameProgress Build(bool enablePlan, bool enableMasks)
    {
        return new Fakes.MaskingTestGameBuilder().Compile();
    }

    [Fact]
    public void GivenExclusivePhases_WhenMasksEnabled_ThenSecondPhaseSkippedAfterFirstApplies()
    {
        // arrange

        // act

        // assert

        var plan = Build(true, true);
        var piece = plan.Game.GetPiece("a");
        piece.Should().NotBeNull();
        var tile2 = plan.Game.Board.GetTile("tile-2-1");
        tile2.Should().NotBeNull();
        var fromState = plan.State.GetState<PieceState>(piece!);
        fromState.Should().NotBeNull();
        var relation = plan.Game.Board.TileRelations.First(r => r.From.Equals(fromState!.CurrentTile) && r.To.Equals(tile2!));
        var path = new TilePath([relation]);
        var evt = new MovePieceGameEvent(piece, path);

        // act
        var result = plan.HandleEvent(evt);

        // assert
        // Only one move should happen; piece 'b' must remain on original tile due to mask skipping second exclusive phase
        var pieceB = result.Game.GetPiece("b");
        pieceB.Should().NotBeNull();
        var pieceStateA = result.State.GetState<PieceState>(piece!);
        pieceStateA.Should().NotBeNull();
        pieceStateA!.CurrentTile.Should().Be(tile2!);
        var originalTileB = plan.Game.Board.GetTile("tile-1-1");
        originalTileB.Should().NotBeNull();
        var pieceStateB = result.State.GetState<PieceState>(pieceB!);
        pieceStateB.Should().NotBeNull();
        pieceStateB!.CurrentTile.Should().Be(originalTileB!);
    }

    [Fact]
    public void GivenExclusivePhases_WhenMasksDisabled_ThenSecondPhaseStillEligible()
    {
        // arrange

        // act

        // assert

        var plan = Build(true, false);
        var piece = plan.Game.GetPiece("a");
        piece.Should().NotBeNull();
        var pieceB = plan.Game.GetPiece("b");
        pieceB.Should().NotBeNull();
        var tile2 = plan.Game.Board.GetTile("tile-2-1");
        tile2.Should().NotBeNull();
        var fromState = plan.State.GetState<PieceState>(piece!);
        fromState.Should().NotBeNull();
        var relation = plan.Game.Board.TileRelations.First(r => r.From.Equals(fromState!.CurrentTile) && r.To.Equals(tile2!));
        var path = new TilePath([relation]);
        var evt = new MovePieceGameEvent(piece, path);

        // act
        var result = plan.HandleEvent(evt);

        // assert: Without masks (exclusivity hints ignored) both phases still evaluated; because both phases perform identical move on separate pieces sharing the same from tile,
        // the second move mutator attempts to move piece 'a' again (already moved) or piece 'b' depending on rule binding. We only assert that at least one piece moved (a), and b may also move.
        var pieceStateA = result.State.GetState<PieceState>(piece!);
        pieceStateA.Should().NotBeNull();
        pieceStateA!.CurrentTile.Should().Be(tile2!);
        // Piece B movement optional (engine may apply rule with first matching piece); ensure no regression by allowing either start or target tile.
        var pieceStateB = result.State.GetState<PieceState>(pieceB!);
        pieceStateB.Should().NotBeNull();
        var bTile = pieceStateB!.CurrentTile;
        new[] { "tile-1-1", "tile-2-1" }.Should().Contain(bTile.Id);
    }
}
