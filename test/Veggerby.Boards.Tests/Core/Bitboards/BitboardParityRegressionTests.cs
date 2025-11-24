using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States; // GameProgress
using Veggerby.Boards.Tests.Infrastructure;

namespace Veggerby.Boards.Tests.Core.Bitboards;

/// <summary>
/// Regression parity tests ensuring bitboard backed occupancy masks remain in lockstep with
/// authoritative piece state enumeration after sequences of moves. Guards against reintroduction
/// of the incremental snapshot desync that motivated temporarily disabling the incremental path.
/// </summary>
public class BitboardParityRegressionTests
{
    private static ulong ComputeNaiveMask(GameProgress progress)
    {
        // Mirror BoardShape.Build ordering (tiles ordered lexicographically by id)
        var tiles = progress.Game.Board.Tiles.OrderBy(t => t.Id, StringComparer.Ordinal).ToArray();
        var tileIndex = new Dictionary<string, int>(tiles.Length);
        for (int i = 0; i < tiles.Length; i++)
        {
            tileIndex[tiles[i].Id] = i;
        }

        ulong mask = 0UL;
        foreach (var ps in progress.State.GetStates<PieceState>())
        {
            if (ps.CurrentTile is null)
            {
                continue;
            }

            if (!tileIndex.TryGetValue(ps.CurrentTile.Id, out var idx))
            {
                continue;
            }

            mask |= 1UL << idx;
        }
        return mask;
    }

    private static ulong ComputeNaivePlayerMask(GameProgress progress, string playerId)
    {
        var tiles = progress.Game.Board.Tiles.OrderBy(t => t.Id, StringComparer.Ordinal).ToArray();
        var tileIndex = new Dictionary<string, int>(tiles.Length);
        for (int i = 0; i < tiles.Length; i++)
        {
            tileIndex[tiles[i].Id] = i;
        }

        ulong mask = 0UL;
        foreach (var ps in progress.State.GetStates<PieceState>())
        {
            if (ps.CurrentTile is null || ps.Artifact.Owner is null || !string.Equals(ps.Artifact.Owner.Id, playerId, StringComparison.Ordinal))
            {
                continue;
            }

            if (!tileIndex.TryGetValue(ps.CurrentTile.Id, out var idx))
            {
                continue;
            }

            mask |= 1UL << idx;
        }
        return mask;
    }

    [Fact]
    public void GivenSequenceOfMoves_WhenBitboardsEnabled_ThenMasksMatchNaiveEnumeration()
    {
        // arrange

        // act

        // assert

        var progress = new ChessGameBuilder().Compile();

        // deterministic move sequence (single step pawn advances + a knight move)
        var moves = new (string piece, string to)[]
        {
            ("white-pawn-5", "e3"),
            ("black-pawn-5", "e6"),
            ("white-knight-2", "f3"),
            ("black-knight-2", "f6"),
            ("white-pawn-6", "f3") // capture knight (tests removal + add same tile)
        };

        for (int i = 0; i < moves.Length; i++)
        {
            var (piece, to) = moves[i];
            progress = progress.Move(piece, to);

            var ok = progress.TryGetBitboards(out var occ, out var perPlayer);
            ok.Should().BeTrue("bitboards should be active in parity regression test");

            var naiveGlobal = ComputeNaiveMask(progress);
            occ.Mask.Should().Be(naiveGlobal, $"global mask diverged after step {i} ({piece}->{to})");

            // per-player masks
            var whiteMask = ComputeNaivePlayerMask(progress, Veggerby.Boards.Chess.Constants.ChessIds.Players.White);
            var blackMask = ComputeNaivePlayerMask(progress, Veggerby.Boards.Chess.Constants.ChessIds.Players.Black);
            perPlayer.Should().NotBeNull();
            var whiteEntry = perPlayer.Single(p => p.Key.Id == Veggerby.Boards.Chess.Constants.ChessIds.Players.White);
            var blackEntry = perPlayer.Single(p => p.Key.Id == Veggerby.Boards.Chess.Constants.ChessIds.Players.Black);
            whiteEntry.Value.Should().NotBeNull();
            blackEntry.Value.Should().NotBeNull();
            var white = whiteEntry.Value.Mask;
            var black = blackEntry.Value.Mask;
            white.Should().Be(whiteMask, $"white mask diverged after step {i}");
            black.Should().Be(blackMask, $"black mask diverged after step {i}");
        }
    }
}
