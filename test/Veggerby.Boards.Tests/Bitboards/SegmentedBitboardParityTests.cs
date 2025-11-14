using System;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Tests.Bitboards;

/// <summary>
/// Parity tests ensuring segmented bitboard mirrors legacy 64-bit and 128-bit occupancy masks.
/// </summary>
public class SegmentedBitboardParityTests
{
    private IDisposable EnableSegmentedFlag()
    {
        // No-op: Segmented bitboards were removed (experimental feature, no current use case)
        return new ActionOnDispose(() => { });
    }

    [Fact]
    public void GivenChessInitialPosition_WhenBuildingSnapshot_ThenSegmentedMatchesLegacy64()
    {
        // arrange

        // act

        // assert

        using var _ = EnableSegmentedFlag();
        var progress = new ChessGameBuilder().Compile();
        var game = progress.Game;
        var state = progress.State;
        var layout = BitboardLayout.Build(game);
        var shape = BoardShape.Build(game.Board);
        var snapshot = BitboardSnapshot.Build(layout, state, shape);

        snapshot.GlobalSegmented.HasValue.Should().BeTrue();
        var seg = snapshot.GlobalSegmented!.Value;
        seg.SegmentCount.Should().Be(1);
        seg.Low64.Should().Be(snapshot.GlobalOccupancy);

        snapshot.PlayerSegmented.Length.Should().Be(layout.PlayerCount);
        for (int p = 0; p < layout.PlayerCount; p++)
        {
            snapshot.PlayerSegmented[p].Low64.Should().Be(snapshot.PlayerOccupancy[p]);
        }

        // Tile by tile parity
        for (int t = 0; t < shape.TileCount; t++)
        {
            bool legacy = (snapshot.GlobalOccupancy & (1UL << t)) != 0;
            seg.Test(t).Should().Be(legacy);
        }
    }

    private sealed class Synthetic128BoardBuilder(int tileCount) : GameBuilder
    {
        private readonly int _tileCount = tileCount;
        protected override void Build()
        {
            BoardId = "synthetic-128";
            AddPlayer("p1");
            AddPlayer("p2");
            AddDirection("east");
            for (int i = 0; i < _tileCount; i++)
            {
                AddTile($"t{i}");
            }
            for (int i = 1; i < _tileCount; i++)
            {
                WithTile($"t{i - 1}").WithRelationTo($"t{i}").InDirection("east").Done();
            }
            if (_tileCount > 1)
            {
                WithTile($"t{_tileCount - 1}").WithRelationTo("t0").InDirection("east").Done();
            }
            // place pieces every 4 tiles alternating owner
            for (int i = 0; i < _tileCount; i += 4)
            {
                var owner = (i / 4) % 2 == 0 ? "p1" : "p2";
                AddPiece($"pc{i}").WithOwner(owner).OnTile($"t{i}");
            }
        }
    }

    [Fact]
    public void Given128TileBoard_WhenBuildingSnapshot_ThenSegmentedMatchesBitboard128()
    {
        // arrange

        // act

        // assert

        using var _ = EnableSegmentedFlag();
        var progress = new Synthetic128BoardBuilder(128).Compile();
        var game = progress.Game;
        var state = progress.State;
        var layout = BitboardLayout.Build(game);
        var shape = BoardShape.Build(game.Board);
        var snapshot = BitboardSnapshot.Build(layout, state, shape);

        snapshot.GlobalOccupancy128.HasValue.Should().BeTrue();
        snapshot.GlobalSegmented.HasValue.Should().BeTrue();
        var seg = snapshot.GlobalSegmented!.Value;
        seg.SegmentCount.Should().Be(2);

        // Low segment parity
        seg.Low64.Should().Be(snapshot.GlobalOccupancy128!.Value.Low);
        // High segment parity via reconstruction
        ReconstructSegment(seg, 1).Should().Be(snapshot.GlobalOccupancy128!.Value.High);

        // Tile-by-tile parity across 128 tiles
        for (int t = 0; t < shape.TileCount; t++)
        {
            bool legacy = t < 64
                ? (snapshot.GlobalOccupancy128.Value.Low & (1UL << t)) != 0
                : (snapshot.GlobalOccupancy128.Value.High & (1UL << (t - 64))) != 0;
            seg.Test(t).Should().Be(legacy);
        }
    }

    private static ulong ReconstructSegment(SegmentedBitboard bb, int segmentIndex)
    {
        ulong value = 0UL;
        for (int bit = 0; bit < 64; bit++)
        {
            int tileIndex = (segmentIndex << 6) | bit;
            if (bb.Test(tileIndex))
            {
                value |= 1UL << bit;
            }
        }
        return value;
    }

    private sealed class ActionOnDispose : IDisposable
    {
        private readonly Action _action;
        public ActionOnDispose(Action action) => _action = action;
        public void Dispose() => _action();
    }
}
