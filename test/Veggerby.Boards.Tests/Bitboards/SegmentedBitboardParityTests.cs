using System;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Tests.Bitboards;

/// <summary>
/// Parity tests for bitboard snapshot generation (segmented bitboards feature removed).
/// </summary>
public class SegmentedBitboardParityTests
{
    [Fact(Skip = "Segmented bitboards removed - experimental feature with no current use case")]
    public void GivenChessInitialPosition_WhenBuildingSnapshot_ThenSegmentedMatchesLegacy64()
    {
        // Placeholder for removed segmented bitboard test
    }

    [Fact(Skip = "Segmented bitboards removed - experimental feature with no current use case")]
    public void Given128TileBoard_WhenBuildingSnapshot_ThenSegmentedMatchesBitboard128()
    {
        // Placeholder for removed segmented bitboard test
    }

    private sealed class ActionOnDispose(Action action) : IDisposable
    {
        private readonly Action _action = action;
        public void Dispose() => _action();
    }
}
