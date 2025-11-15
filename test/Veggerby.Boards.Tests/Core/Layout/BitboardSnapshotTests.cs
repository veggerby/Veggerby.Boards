using Veggerby.Boards.Internal;
using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Tests.Core.Layout;

public class BitboardSnapshotTests
{
    private sealed class SyntheticBoardBuilder(int tileCount, bool placePieces = true) : GameBuilder
    {
        private readonly int _tileCount = tileCount;
        private readonly bool _place = placePieces;
        protected override void Build()
        {
            BoardId = $"synthetic-{_tileCount}";
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
            if (_place)
            {
                // One piece for each player at predictable tiles when present
                AddPiece("pcW").WithOwner("p1").OnTile("t1");
                AddPiece("pcB").WithOwner("p2").OnTile("t3");
            }
        }
    }

    [Fact]
    public void GivenSmallBoard_WhenBuild_ThenMasksSetForOwners()
    {
        // arrange

        // act

        // assert

        var progress = new SyntheticBoardBuilder(8).Compile();
        var game = progress.Game;
        var state = progress.State;
        var layout = BitboardLayout.Build(game);
        var shape = BoardShape.Build(game.Board);

        // act
        var snapshot = BitboardSnapshot.Build(layout, state, shape);

        // assert
        // global bits for tiles t1 and t3 should be set (use shape indices)
        var t1 = game.GetTile("t1");
        t1.Should().NotBeNull();
        var t3 = game.GetTile("t3");
        t3.Should().NotBeNull();
        shape.TryGetTileIndex(t1!, out var i1).Should().BeTrue();
        shape.TryGetTileIndex(t3!, out var i3).Should().BeTrue();
        (snapshot.GlobalOccupancy & (1UL << i1)).Should().NotBe(0UL);
        (snapshot.GlobalOccupancy & (1UL << i3)).Should().NotBe(0UL);
        // per player
        (snapshot.PlayerOccupancy[0] & (1UL << i1)).Should().NotBe(0UL);
        (snapshot.PlayerOccupancy[0] & (1UL << i3)).Should().Be(0UL);
        (snapshot.PlayerOccupancy[1] & (1UL << i3)).Should().NotBe(0UL);
        (snapshot.PlayerOccupancy[1] & (1UL << i1)).Should().Be(0UL);
        // segmented disabled by default
        snapshot.GlobalSegmented.HasValue.Should().BeFalse();
    }

    [Fact(Skip = "Segmented bitboards removed - experimental feature with no current use case")]
    public void GivenSegmentedFlagOn_WhenBuildSmallBoard_ThenSegmentedMatchesLegacy64()
    {
        // Placeholder for removed segmented bitboard test
    }

    [Fact]
    public void GivenExpectedFromMismatch_WhenUpdateForMove_ThenNoChange()
    {
        // arrange

        // act

        // assert

        var progress = new SyntheticBoardBuilder(8).Compile();
        var game = progress.Game;
        var state = progress.State;
        var layout = BitboardLayout.Build(game);
        var pieceLayout = PieceMapLayout.Build(game);
        var shape = BoardShape.Build(game.Board);
        var pieceMap = PieceMapSnapshot.Build(pieceLayout, state, shape);
        var snapshot = BitboardSnapshot.Build(layout, state, shape);
        var pcW = game.GetPiece("pcW");
        pcW.Should().NotBeNull();
        ulong beforeGlobal = snapshot.GlobalOccupancy;

        // act
        // use an incorrect expected-from index (not matching current and non-negative triggers check)
        var pcWIndex = pieceMap.GetTileIndex(pcW!);
        var wrongFrom = (short)(pcWIndex + 1);
        var updated = snapshot.UpdateForMove(pcW!, fromTileIndex: wrongFrom, toTileIndex: 5, pieceMap: pieceMap, shape: shape);

        // assert
        updated.GlobalOccupancy.Should().Be(beforeGlobal);

        // also: when pieceMap is null, snapshot should be returned unchanged (reference equality ok)
        // passing correct index but null pieceMap keeps original instance
        var noChange = snapshot.UpdateForMove(pcW!, fromTileIndex: pcWIndex, toTileIndex: 5, pieceMap: null!, shape: shape);
        ReferenceEquals(noChange, snapshot).Should().BeTrue();
    }

    [Fact]
    public void GivenValidMove64_WhenUpdateForMove_ThenBitsUpdatedForOwner()
    {
        // arrange

        // act

        // assert

        var progress = new SyntheticBoardBuilder(8).Compile();
        var game = progress.Game;
        var state = progress.State;
        var layout = BitboardLayout.Build(game);
        var pieceLayout = PieceMapLayout.Build(game);
        var shape = BoardShape.Build(game.Board);
        var pieceMap = PieceMapSnapshot.Build(pieceLayout, state, shape);
        var snapshot = BitboardSnapshot.Build(layout, state, shape);
        var pcW = game.GetPiece("pcW");
        pcW.Should().NotBeNull();

        var t1m = game.GetTile("t1");
        t1m.Should().NotBeNull();
        var t5m = game.GetTile("t5");
        t5m.Should().NotBeNull();
        shape.TryGetTileIndex(t1m!, out var fromIdx).Should().BeTrue();
        shape.TryGetTileIndex(t5m!, out var toIdx).Should().BeTrue();

        // act
        var moved = snapshot.UpdateForMove(pcW!, fromTileIndex: (short)fromIdx, toTileIndex: (short)toIdx, pieceMap: pieceMap, shape: shape);

        // assert
        // global
        (moved.GlobalOccupancy & (1UL << fromIdx)).Should().Be(0UL);
        (moved.GlobalOccupancy & (1UL << toIdx)).Should().NotBe(0UL);
        // per-player for p1
        (moved.PlayerOccupancy[0] & (1UL << fromIdx)).Should().Be(0UL);
        (moved.PlayerOccupancy[0] & (1UL << toIdx)).Should().NotBe(0UL);
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
            // pieces
            AddPiece("pcW").WithOwner("p1").OnTile("t70");
            AddPiece("pcB").WithOwner("p2").OnTile("t3");
        }
    }

    [Fact]
    public void Given128BoardMoveAcrossSegments_WhenUpdateForMove_ThenHighLowUpdated()
    {
        // arrange

        // act

        // assert

        var progress = new Synthetic128BoardBuilder(128).Compile();
        var game = progress.Game;
        var state = progress.State;
        var layout = BitboardLayout.Build(game);
        var pieceLayout = PieceMapLayout.Build(game);
        var shape = BoardShape.Build(game.Board);
        var pieceMap = PieceMapSnapshot.Build(pieceLayout, state, shape);
        var snapshot = BitboardSnapshot.Build(layout, state, shape);
        var pcW = game.GetPiece("pcW");
        pcW.Should().NotBeNull();

        var t70 = game.GetTile("t70");
        t70.Should().NotBeNull();
        var t5 = game.GetTile("t5");
        t5.Should().NotBeNull();
        shape.TryGetTileIndex(t70!, out var fromIdx).Should().BeTrue();
        shape.TryGetTileIndex(t5!, out var toIdx).Should().BeTrue();

        // act
        var moved = snapshot.UpdateForMove(pcW!, fromTileIndex: (short)fromIdx, toTileIndex: (short)toIdx, pieceMap: pieceMap, shape: shape);

        // assert
        moved.GlobalOccupancy128.HasValue.Should().BeTrue();
        var g = moved.GlobalOccupancy128!.Value;
        // verify by testing bits via computed indices
        if (fromIdx >= 64)
        {
            (g.High & (1UL << (fromIdx - 64))).Should().Be(0UL);
        }
        else
        {
            (g.Low & (1UL << fromIdx)).Should().Be(0UL);
        }
        if (toIdx >= 64)
        {
            (g.High & (1UL << (toIdx - 64))).Should().NotBe(0UL);
        }
        else
        {
            (g.Low & (1UL << toIdx)).Should().NotBe(0UL);
        }

        // per-player p1 updated similarly
        var p1 = moved.PlayerOccupancy128[0];
        if (fromIdx >= 64)
        {
            (p1.High & (1UL << (fromIdx - 64))).Should().Be(0UL);
        }
        else
        {
            (p1.Low & (1UL << fromIdx)).Should().Be(0UL);
        }
        if (toIdx >= 64)
        {
            (p1.High & (1UL << (toIdx - 64))).Should().NotBe(0UL);
        }
        else
        {
            (p1.Low & (1UL << toIdx)).Should().NotBe(0UL);
        }
    }
}
