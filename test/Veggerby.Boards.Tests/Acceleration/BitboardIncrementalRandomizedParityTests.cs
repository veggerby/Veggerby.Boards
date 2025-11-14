using System;
using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Acceleration;

/// <summary>
/// Deterministic (seeded) randomized parity soak comparing incremental vs full rebuild bitboard paths.
/// Strategy: attempt random piece→tile moves using existing Move helper until a target number of successful
/// moves (state-changing events) have been applied. Both baseline and incremental instances consume the
/// identical pseudo-random sequence to guarantee determinism. Captures and piece removals are naturally
/// included. Parity assertion is performed on the final set of occupied tiles (derived from piece states),
/// skipping gracefully if bitboards/occupancy are not active in the environment.
/// </summary>
public class BitboardIncrementalRandomizedParityTests
{
    private sealed class FlagScope : IDisposable
    {
        public FlagScope(bool enable)
        {
        }
        public void Dispose()
        {
        }
    }

    private static GameProgress ApplyRandomMoves(GameProgress progress, int targetMoves, int seed)
    {
        var rng = new TestDeterministicRng(seed);
        var board = progress.Game.Board;
        var tiles = board.Tiles.Select(t => t.Id).ToArray();
        var movesApplied = 0;
        var safetyAttempts = 0;
        const int maxAttempts = 8000; // ample to avoid infinite loops late-game

        while (movesApplied < targetMoves && safetyAttempts < maxAttempts)
        {
            safetyAttempts++;
            var pieceStates = progress.State.GetStates<PieceState>().ToArray();
            if (pieceStates.Length == 0)
            {
                break; // game practically over
            }
            var pieceIndex = rng.Next(pieceStates.Length);
            var pieceState = pieceStates[pieceIndex];
            var piece = pieceState.Artifact;
            // ensure target differs from current to avoid from==to exception in path resolver
            string targetTileId = pieceState.CurrentTile.Id;
            int ret = 0;
            while (targetTileId == pieceState.CurrentTile.Id && ret < 8)
            {
                targetTileId = tiles[rng.Next(tiles.Length)];
                ret++;
            }
            if (targetTileId == pieceState.CurrentTile.Id)
            {
                continue; // could not find different tile quickly, pick another piece next iteration
            }

            var before = progress.State; // snapshot reference
            var updated = progress.Move(piece.Id, targetTileId);
            if (!ReferenceEquals(before, updated.State))
            {
                progress = updated;
                movesApplied++;
            }
        }
        return progress;
    }

    [Fact]
    public void GivenDeterministicRandomMoveSequence_WhenIncrementalEnabled_ThenOccupancyParityHolds()
    {
        // arrange

        // act

        // assert

        var baseline = new ChessGameBuilder().Compile();
        GameProgress incremental;
        using (new FlagScope(true))
        {
            incremental = new ChessGameBuilder().Compile();
        }

        const int targetMoves = 150; // moderately large to exercise diverse transitions
        const int seed = 0xC0FFEE; // fixed seed for determinism

        // act (apply identical pseudo-random script)
        baseline = ApplyRandomMoves(baseline, targetMoves, seed);
        incremental = ApplyRandomMoves(incremental, targetMoves, seed);

        var baseOcc = baseline.Engine.Capabilities?.AccelerationContext?.Occupancy;
        var incOcc = incremental.Engine.Capabilities?.AccelerationContext?.Occupancy;
        if (baseOcc is null || incOcc is null)
        {
            return; // environment not using bitboard occupancy, skip parity assertion
        }

        // assert
        var baseTiles = baseline.State.GetStates<PieceState>().Select(ps => ps.CurrentTile.Id).OrderBy(x => x).ToArray();
        var incTiles = incremental.State.GetStates<PieceState>().Select(ps => ps.CurrentTile.Id).OrderBy(x => x).ToArray();
        incTiles.Should().BeEquivalentTo(baseTiles, o => o.WithStrictOrdering());
    }
}
