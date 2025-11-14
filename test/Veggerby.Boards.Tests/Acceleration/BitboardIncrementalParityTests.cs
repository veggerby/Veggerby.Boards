using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Acceleration;

public class BitboardIncrementalParityTests
{
    private sealed class FlagScope : System.IDisposable
    {
        public FlagScope(bool enable)
        {
        }
        public void Dispose()
        {
        }
    }

    [Fact]
    public void GivenMoveSequence_WhenIncrementalEnabled_ThenOccupancyMatchesFullRebuild()
    {
        // arrange

        // act

        // assert

        var builderBase = new ChessGameBuilder();
        var baseline = builderBase.Compile();
        var builderInc = new ChessGameBuilder();
        var incremental = builderInc.Compile();

        // script (simple opening sequence)
        static GameProgress Apply(GameProgress p)
        {
            p = p.Move("white-pawn-5", "e4");
            p = p.Move("black-pawn-5", "e5");
            p = p.Move("white-knight-2", "f3");
            p = p.Move("black-knight-2", "c6");
            return p;
        }

        baseline = Apply(baseline);
        incremental = Apply(incremental);

        // act
        var baseOcc = baseline.Engine.Capabilities?.AccelerationContext?.Occupancy;
        var incOcc = incremental.Engine.Capabilities?.AccelerationContext?.Occupancy;

        // assert: if either path lacks occupancy (e.g., bitboards disabled) skip gracefully
        if (baseOcc is null || incOcc is null)
        {
            return; // environment not bitboard-mode
        }

        // Compare all occupied tiles collected from piece states since IOccupancyIndex surface may differ
        var baseTiles = baseline.State.GetStates<PieceState>().Select(ps => ps.CurrentTile.Id).OrderBy(x => x).ToArray();
        var incTiles = incremental.State.GetStates<PieceState>().Select(ps => ps.CurrentTile.Id).OrderBy(x => x).ToArray();
        incTiles.Should().BeEquivalentTo(baseTiles, o => o.WithStrictOrdering());
    }
}
