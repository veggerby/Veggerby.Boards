using System.Linq;

using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.Layout;

/// <summary>
/// Validates BitboardSnapshot build path for >64 tile board using Bitboard128 path.
/// </summary>
public class Bitboard128SnapshotTests
{
    private sealed class SyntheticLargeBoardBuilder(int tileCount) : GameBuilder
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
            // add linear relations after all tiles exist
            for (int i = 1; i < _tileCount; i++)
            {
                WithTile($"t{i - 1}").WithRelationTo($"t{i}").InDirection("east").Done();
            }
            // add one wrap relation to guarantee at least one cycle (ensures non-empty even if count=1 edge case avoided)
            if (_tileCount > 1)
            {
                WithTile($"t{_tileCount - 1}").WithRelationTo("t0").InDirection("east").Done();
            }
            // pieces every 8 tiles alternating owners
            for (int i = 0; i < _tileCount; i += 8)
            {
                var owner = (i / 8) % 2 == 0 ? "p1" : "p2";
                AddPiece($"pc{i}").WithOwner(owner).OnTile($"t{i}");
            }
        }
    }

    [Fact]
    public void GivenBoardWithMoreThan64Tiles_WhenSnapshotBuilt_Then128BitOccupancyPopCountMatches()
    {
        // arrange
        var progress = new SyntheticLargeBoardBuilder(72).Compile();
        var game = progress.Game;
        var state = progress.State;
        var shape = Veggerby.Boards.Internal.Layout.BoardShape.Build(game.Board);
        var layout = Veggerby.Boards.Internal.Layout.BitboardLayout.Build(game);

        // act
        var snapshot = Veggerby.Boards.Internal.Layout.BitboardSnapshot.Build(layout, state, shape);

        // assert
        snapshot.GlobalOccupancy128.HasValue.Should().BeTrue();
        var occupiedCount = state.GetStates<PieceState>().Count();
        var lowBits = System.Numerics.BitOperations.PopCount(snapshot.GlobalOccupancy128.Value.Low);
        var highBits = System.Numerics.BitOperations.PopCount(snapshot.GlobalOccupancy128.Value.High);
        (lowBits + highBits).Should().Be(occupiedCount);
    }
}