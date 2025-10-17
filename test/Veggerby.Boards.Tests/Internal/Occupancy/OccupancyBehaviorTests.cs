using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Internal.Occupancy;

public class OccupancyBehaviorTests
{
    [Fact]
    public void GivenMove_WhenHandled_ThenOccupancyReflectsVacatedAndNewTile()
    {
        // arrange
        using var flags = new Infrastructure.FeatureFlagScope(bitboards: false); // use naive occupancy for deterministic ground truth
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        // Choose a white pawn with a clear single-step forward move (b2 -> b3) in initial setup.
        var piece = game.GetPiece("white-pawn-2");
        piece.Should().NotBeNull();
        var fromState = progress.State.GetState<PieceState>(piece!);
        fromState.Should().NotBeNull(); // sanity
        var from = fromState!.CurrentTile;
        // Target tile one rank forward.
        var to = game.GetTile(ChessIds.Tiles.B3);
        to.Should().NotBeNull();
        // Pre condition (ground truth via state): source occupied by piece, destination unoccupied.
        var allPieceStates = progress.State.GetStates<PieceState>().ToArray();
        allPieceStates.Should().Contain(ps => ps.CurrentTile == from && ps.Artifact == piece!);
        allPieceStates.Should().NotContain(ps => ps.CurrentTile == to);

        var path = progress.ResolvePathCompiledFirst(piece!, from, to!);
        path.Should().NotBeNull();

        // act
        var after = progress.HandleEvent(new MovePieceGameEvent(piece, path!));

        // assert (use GameState as source of truth; occupancy should align)
        var afterPieceState = after.State.GetState<PieceState>(piece!);
        afterPieceState.Should().NotBeNull();
        afterPieceState!.CurrentTile.Should().NotBeNull();
        afterPieceState!.CurrentTile!.Should().Be(to);

        after.Engine.Should().NotBeNull();
        after.Engine!.Capabilities.Should().NotBeNull();
        var accel = after.Engine!.Capabilities!.AccelerationContext;
        accel.Should().NotBeNull();
        var occ = accel!.Occupancy;
        occ.Should().NotBeNull();

        // collect diagnostics if expectations fail
        var fromEmpty = occ.IsEmpty(from);
        var toEmpty = occ.IsEmpty(to);
        var beforePieces = string.Join(", ", progress.State.GetStates<PieceState>().Select(ps => $"{ps.Artifact.Id}@{ps.CurrentTile.Id}"));
        var afterPieces = string.Join(", ", after.State.GetStates<PieceState>().Select(ps => $"{ps.Artifact.Id}@{ps.CurrentTile.Id}"));
        fromEmpty.Should().BeTrue($"Occupancy mismatch. fromEmpty={fromEmpty} toEmpty={toEmpty} before=[{beforePieces}] after=[{afterPieces}]"); // origin should now be empty
        toEmpty.Should().BeFalse($"Occupancy mismatch. fromEmpty={fromEmpty} toEmpty={toEmpty} before=[{beforePieces}] after=[{afterPieces}]"); // destination should be occupied
        occ.IsOwnedBy(to, piece.Owner).Should().BeTrue();
    }
}