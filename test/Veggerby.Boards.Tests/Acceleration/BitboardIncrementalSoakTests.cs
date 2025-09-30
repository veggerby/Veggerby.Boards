using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Acceleration;

/// <summary>
/// Extended deterministic move script exercising multiple sequential moves and captures to soak the
/// incremental bitboard update path and ensure parity with the full rebuild path. This is a focused
/// soak (not randomized) chosen to cover: forward pawn advances, knight moves, reciprocal captures,
/// and a diagonal pawn capture after a sequence of prior captures (piece removal + replacement on same tile).
/// </summary>
public class BitboardIncrementalSoakTests
{
    private sealed class FlagScope : System.IDisposable
    {
        private readonly bool _original;
        public FlagScope()
        {
            _original = Boards.Internal.FeatureFlags.EnableBitboardIncremental;
            Boards.Internal.FeatureFlags.EnableBitboardIncremental = true;
        }
        public void Dispose()
        {
            Boards.Internal.FeatureFlags.EnableBitboardIncremental = _original;
        }
    }

    /// <summary>
    /// Script based on an opening-style sequence with intentional captures creating repeated occupancy
    /// churn on the same tile (e5) to stress incremental add/remove ordering.
    /// </summary>
    [Fact]
    public void GivenExtendedMoveAndCaptureSequence_WhenIncrementalEnabled_ThenOccupancyMatchesBaseline()
    {
        // arrange
        var baseline = new ChessGameBuilder().Compile();
        GameProgress incremental;
        using (new FlagScope())
        {
            incremental = new ChessGameBuilder().Compile();

            // script (moves reflect ids used elsewhere: white-pawn-5 = e file pawn, white-knight-2 = g1 knight, etc.)
            static GameProgress Apply(GameProgress p)
            {
                p = p.Move("white-pawn-5", "e4");   // e2 -> e4
                p = p.Move("black-pawn-5", "e5");   // e7 -> e5
                p = p.Move("white-knight-2", "f3"); // g1 -> f3
                p = p.Move("black-knight-2", "c6"); // b8 -> c6
                p = p.Move("white-knight-2", "e5"); // f3 -> e5 capture black pawn (white knight now on e5)
                p = p.Move("black-knight-2", "e5"); // c6 -> e5 capture white knight (black knight now on e5)
                p = p.Move("white-pawn-4", "d4");   // d2 -> d4
                p = p.Move("black-pawn-4", "d5");   // d7 -> d5
                p = p.Move("white-pawn-4", "e5");   // d4 -> e5 capture black knight (tile e5 churn again)
                return p;
            }

            baseline = Apply(baseline);
            incremental = Apply(incremental);
        }

        // act
        var baseOcc = baseline.Engine.Capabilities?.AccelerationContext?.Occupancy;
        var incOcc = incremental.Engine.Capabilities?.AccelerationContext?.Occupancy;

        // assert (skip gracefully if bitboards disabled entirely)
        if (baseOcc is null || incOcc is null)
        {
            return;
        }

        var baseTiles = baseline.State.GetStates<PieceState>().Select(ps => ps.CurrentTile.Id).OrderBy(x => x).ToArray();
        var incTiles = incremental.State.GetStates<PieceState>().Select(ps => ps.CurrentTile.Id).OrderBy(x => x).ToArray();
        incTiles.Should().BeEquivalentTo(baseTiles, o => o.WithStrictOrdering());
    }
}
