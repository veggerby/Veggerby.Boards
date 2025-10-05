using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Internal;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Capture;

public class MinimalCaptureTests
{
    [Fact]
    public void GivenLinearBoard_WhenSlidingPieceMovesOntoOpponent_ThenCaptureOccurs()
    {
        // arrange
        using var _ = new FeatureFlagScope(compiledPatterns: true); // bitboards optional here
        var progress = new MinimalCaptureGameBuilder().Compile();
        var white = progress.Game.GetPiece("white-slider");
        var black = progress.Game.GetPiece("black-block");
        var a1 = progress.Game.GetTile(ChessIds.Tiles.A1);
        var a2 = progress.Game.GetTile(ChessIds.Tiles.A2);
        var a3 = progress.Game.GetTile(ChessIds.Tiles.A3);
        // Build multi-step path a1 -> a2 -> a3
        var rel1 = progress.Game.Board.TileRelations.Single(r => r.From.Equals(a1) && r.To.Equals(a2));
        var rel2 = progress.Game.Board.TileRelations.Single(r => r.From.Equals(a2) && r.To.Equals(a3));
        var path = new TilePath([rel1, rel2]);
        var evt = new MovePieceGameEvent(white, path);

        // act
        var beforeState = progress.State;
        var updated = progress.HandleEvent(evt);

        // assert attacker relocation
        updated.State.GetState<PieceState>(white).CurrentTile.Should().Be(a3);
        // captured state marker exists
        updated.State.IsCaptured(black).Should().BeTrue();
        // destination no longer lists black piece
        updated.State.GetPiecesOnTile(a3).Any(p => p.Equals(black)).Should().BeFalse();

        // state diff expectations:
        // - attacker PieceState replaced (relocation) (net 0)
        // - defender PieceState removed (-1)
        // - captured marker state added (separate CapturedPieceState, not counted among PieceState)
        var beforePieces = beforeState.GetStates<PieceState>().Select(ps => (ps.Artifact.Id, ps.CurrentTile?.Id)).OrderBy(x => x.Item1).ToArray();
        var afterPieces = updated.State.GetStates<PieceState>().Select(ps => (ps.Artifact.Id, ps.CurrentTile?.Id)).OrderBy(x => x.Item1).ToArray();
        beforePieces.Length.Should().Be(afterPieces.Length + 1, "one material piece state (defender) must be removed");
        // captured piece no longer has a PieceState entry
        afterPieces.Any(p => p.Item1 == black.Id).Should().BeFalse();
        // but a captured marker state exists exactly once
        updated.State.GetStates<CapturedPieceState>().Count(s => s.Artifact.Equals(black)).Should().Be(1);

        // Bitboard parity (only if feature enabled and board <=64 tiles) – occupied squares decrease by one
        if (FeatureFlags.EnableBitboards)
        {
            var beforeOccupied = beforePieces.Count(p => p.Item2 is not null);
            var afterOccupied = afterPieces.Count(p => p.Item2 is not null);
            // One piece removed (black), attacker still occupies destination -> occupied squares decrease by one
            afterOccupied.Should().Be(beforeOccupied - 1);
        }

        // Observer path verification: capture mutator must have applied, move mutator must not.
        CaptureTrackingObserver.Instance.CaptureMutatorApplied.Should().BeTrue();
        CaptureTrackingObserver.Instance.MoveMutatorApplied.Should().BeFalse();

        // Tight diff assertion: Only allowed changes are
        //  - white-slider tile changed from a1 to a3
        //  - black-block PieceState removed and replaced by CapturedPieceState
        //  - no other piece state changes
        var beforeMap = beforePieces.ToDictionary(p => p.Item1, p => p.Item2);
        var afterMap = afterPieces.ToDictionary(p => p.Item1, p => p.Item2);
        // white-slider present both before and after with different tile
        beforeMap.ContainsKey(white.Id).Should().BeTrue();
        afterMap.ContainsKey(white.Id).Should().BeTrue();
        beforeMap[white.Id].Should().Be(a1.Id);
        afterMap[white.Id].Should().Be(a3.Id);
        // black-block removed
        beforeMap.ContainsKey(black.Id).Should().BeTrue();
        afterMap.ContainsKey(black.Id).Should().BeFalse();
        // No other ids changed (no extras, so only two pieces exist originally)
        beforeMap.Keys.Except(new[] { white.Id, black.Id }).Should().BeEmpty();
        afterMap.Keys.Except(new[] { white.Id }).Should().BeEmpty();
    }
}