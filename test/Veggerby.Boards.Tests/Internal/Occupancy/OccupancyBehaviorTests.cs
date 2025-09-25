using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Internal.Occupancy;

public class OccupancyBehaviorTests
{
    [Fact]
    public void GivenMove_WhenHandled_ThenOccupancyReflectsVacatedAndNewTile()
    {
        // arrange
        using var flags = new Veggerby.Boards.Tests.Infrastructure.FeatureFlagScope(bitboards: false); // use naive occupancy for deterministic ground truth
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        // Choose a white pawn with a clear single-step forward move (b2 -> b3) in initial setup.
        var piece = game.GetPiece("white-pawn-2");
        var fromState = progress.State.GetState<PieceState>(piece);
        Assert.NotNull(fromState); // sanity
        var from = fromState.CurrentTile;
        // Target tile one rank forward.
        var to = game.GetTile("tile-b3");
        // Pre condition (ground truth via state): source occupied by piece, destination unoccupied.
        var allPieceStates = progress.State.GetStates<PieceState>().ToArray();
        Assert.Contains(allPieceStates, ps => ps.CurrentTile == from && ps.Artifact == piece);
        Assert.DoesNotContain(allPieceStates, ps => ps.CurrentTile == to);

        var path = progress.ResolvePathCompiledFirst(piece, from, to);
        Assert.NotNull(path);

        // act
        var after = progress.HandleEvent(new MovePieceGameEvent(piece, path!));

        // assert (use GameState as source of truth; occupancy should align)
        var afterPieceState = after.State.GetState<PieceState>(piece);
        Assert.Equal(to, afterPieceState.CurrentTile);

        var occ = after.Engine.Capabilities.AccelerationContext.Occupancy;

        // collect diagnostics if expectations fail
        var fromEmpty = occ.IsEmpty(from);
        var toEmpty = occ.IsEmpty(to);
        if (!fromEmpty || toEmpty == true)
        {
            var beforePieces = string.Join(", ", progress.State.GetStates<PieceState>().Select(ps => $"{ps.Artifact.Id}@{ps.CurrentTile.Id}"));
            var afterPieces = string.Join(", ", after.State.GetStates<PieceState>().Select(ps => $"{ps.Artifact.Id}@{ps.CurrentTile.Id}"));
            Assert.Fail($"Occupancy mismatch. fromEmpty={fromEmpty} toEmpty={toEmpty} before=[{beforePieces}] after=[{afterPieces}]");
        }

        Assert.True(fromEmpty); // origin should now be empty
        Assert.False(toEmpty); // destination should be occupied
        Assert.True(occ.IsOwnedBy(to, piece.Owner));
    }
}