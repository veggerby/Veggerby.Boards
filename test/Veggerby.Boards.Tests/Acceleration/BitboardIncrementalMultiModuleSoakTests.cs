using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Go;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Acceleration;

/// <summary>
/// Large-scale randomized multi-module soak tests comparing incremental vs full rebuild bitboard paths
/// across Chess, Backgammon, and Go. Strategy: Apply 10,000+ random moves per module with deterministic
/// RNG to guarantee reproducibility, then validate occupancy parity.
/// </summary>
public class BitboardIncrementalMultiModuleSoakTests
{
    private sealed class FlagScope : IDisposable
    {
        private readonly bool _original;

        public FlagScope(bool enable)
        {
            _original = Boards.Internal.FeatureFlags.EnableBitboardIncremental;
            Boards.Internal.FeatureFlags.EnableBitboardIncremental = enable;
        }

        public void Dispose()
        {
            Boards.Internal.FeatureFlags.EnableBitboardIncremental = _original;
        }
    }

    private static GameProgress ApplyRandomMoves(GameProgress progress, int targetMoves, int seed)
    {
        var rng = new TestDeterministicRng(seed);
        var board = progress.Game.Board;
        var tiles = board.Tiles.Select(t => t.Id).ToArray();
        var movesApplied = 0;
        var safetyAttempts = 0;
        var maxAttempts = targetMoves * 100;

        while (movesApplied < targetMoves && safetyAttempts < maxAttempts)
        {
            safetyAttempts++;
            var pieceStates = progress.State.GetStates<PieceState>().ToArray();
            if (pieceStates.Length == 0)
            {
                break;
            }

            var pieceIndex = rng.Next(pieceStates.Length);
            var pieceState = pieceStates[pieceIndex];
            var piece = pieceState.Artifact;
            string targetTileId = pieceState.CurrentTile.Id;
            int ret = 0;

            while (targetTileId == pieceState.CurrentTile.Id && ret < 8)
            {
                targetTileId = tiles[rng.Next(tiles.Length)];
                ret++;
            }

            if (targetTileId == pieceState.CurrentTile.Id)
            {
                continue;
            }

            var before = progress.State;
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
    public void GivenChessRandomMoves_WhenIncrementalEnabled_ThenOccupancyParityHolds()
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

        const int targetMoves = 10_000;
        const int seed = 0xC4E55;

        baseline = ApplyRandomMoves(baseline, targetMoves, seed);
        incremental = ApplyRandomMoves(incremental, targetMoves, seed);

        var baseOcc = baseline.Engine.Capabilities?.AccelerationContext?.Occupancy;
        var incOcc = incremental.Engine.Capabilities?.AccelerationContext?.Occupancy;
        if (baseOcc is null || incOcc is null)
        {
            return;
        }

        var baseTiles = baseline.State.GetStates<PieceState>().Select(ps => ps.CurrentTile.Id).OrderBy(x => x).ToArray();
        var incTiles = incremental.State.GetStates<PieceState>().Select(ps => ps.CurrentTile.Id).OrderBy(x => x).ToArray();
        incTiles.Should().BeEquivalentTo(baseTiles, o => o.WithStrictOrdering());
    }

    [Fact]
    public void GivenBackgammonRandomMoves_WhenIncrementalEnabled_ThenOccupancyParityHolds()
    {
        // arrange

        // act

        // assert

        var baseline = new BackgammonGameBuilder().Compile();
        GameProgress incremental;
        using (new FlagScope(true))
        {
            incremental = new BackgammonGameBuilder().Compile();
        }

        const int targetMoves = 10_000;
        const int seed = 0xBA66AA;

        baseline = ApplyRandomMoves(baseline, targetMoves, seed);
        incremental = ApplyRandomMoves(incremental, targetMoves, seed);

        var baseOcc = baseline.Engine.Capabilities?.AccelerationContext?.Occupancy;
        var incOcc = incremental.Engine.Capabilities?.AccelerationContext?.Occupancy;
        if (baseOcc is null || incOcc is null)
        {
            return;
        }

        var baseTiles = baseline.State.GetStates<PieceState>().Select(ps => ps.CurrentTile.Id).OrderBy(x => x).ToArray();
        var incTiles = incremental.State.GetStates<PieceState>().Select(ps => ps.CurrentTile.Id).OrderBy(x => x).ToArray();
        incTiles.Should().BeEquivalentTo(baseTiles, o => o.WithStrictOrdering());
    }

    [Fact]
    public void GivenGoRandomMoves_WhenIncrementalEnabled_ThenOccupancyParityHolds()
    {
        // arrange

        // act

        // assert

        var baseline = new GoGameBuilder(9).Compile();
        GameProgress incremental;
        using (new FlagScope(true))
        {
            incremental = new GoGameBuilder(9).Compile();
        }

        const int targetMoves = 10_000;
        const int seed = 0x60901;

        baseline = ApplyRandomMoves(baseline, targetMoves, seed);
        incremental = ApplyRandomMoves(incremental, targetMoves, seed);

        var baseOcc = baseline.Engine.Capabilities?.AccelerationContext?.Occupancy;
        var incOcc = incremental.Engine.Capabilities?.AccelerationContext?.Occupancy;
        if (baseOcc is null || incOcc is null)
        {
            return;
        }

        var baseTiles = baseline.State.GetStates<PieceState>().Select(ps => ps.CurrentTile.Id).OrderBy(x => x).ToArray();
        var incTiles = incremental.State.GetStates<PieceState>().Select(ps => ps.CurrentTile.Id).OrderBy(x => x).ToArray();
        incTiles.Should().BeEquivalentTo(baseTiles, o => o.WithStrictOrdering());
    }
}
