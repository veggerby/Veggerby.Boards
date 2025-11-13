using System;

using Veggerby.Boards.Internal.Layout;

namespace Veggerby.Boards.Tests.Bitboards;

/// <summary>
/// Direct unit tests for <see cref="SegmentedBitboard"/> covering edge cases and branches not exercised by parity/integration tests.
/// </summary>
public class SegmentedBitboardTests
{
    [Fact]
    public void GivenEmptySegments_WhenBuilding_ThenEmptyBitboardReturned()
    {
        // arrange

        // act

        // assert

        var bb = SegmentedBitboard.FromSegments(ReadOnlySpan<ulong>.Empty);

        // assert
        bb.SegmentCount.Should().Be(1); // coerced to single empty segment
        bb.Any.Should().BeFalse();
        bb.None.Should().BeTrue();
        bb.PopCount().Should().Be(0);
        bb.Test(0).Should().BeFalse();
    }

    [Fact]
    public void GivenSingleSegment_WhenSettingAndClearingBits_ThenStateReflectsChanges()
    {
        // arrange

        // act

        // assert

        var bb = SegmentedBitboard.FromSegments(stackalloc ulong[] { 0UL });

        // act - set bit 5
        var withBit = bb.WithSet(5);
        var againSet = withBit.WithSet(5); // idempotent
        // act - clear
        var cleared = withBit.WithCleared(5);

        // assert original unchanged
        bb.PopCount().Should().Be(0);
        withBit.PopCount().Should().Be(1);
        againSet.PopCount().Should().Be(1);
        cleared.PopCount().Should().Be(0);
        withBit.Test(5).Should().BeTrue();
        cleared.Test(5).Should().BeFalse();
    }

    [Fact]
    public void GivenNegativeIndex_WhenTestingAndSetting_ThenNoEffect()
    {
        // arrange

        // act

        // assert

        var bb = SegmentedBitboard.FromSegments(stackalloc ulong[] { 0UL });

        bb.Test(-1).Should().BeFalse();
        var mutated = bb.WithSet(-10); // no-op
        mutated.PopCount().Should().Be(0);
        mutated.Low64.Should().Be(bb.Low64);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(127)]
    [InlineData(128)]
    [InlineData(191)]
    [InlineData(192)]
    [InlineData(255)]
    public void GivenFourInlineSegments_WhenSettingBitsAcrossBoundaries_ThenBitsVisible(int tileIndex)
    {
        // arrange: 4 segments (256 tiles) all zero
        var bb = SegmentedBitboard.FromSegments(stackalloc ulong[] { 0, 0, 0, 0 });

        // act
        var updated = bb.WithSet(tileIndex);

        // assert segment selection works (also exercises GetSegment switch cases 0..3)
        updated.Test(tileIndex).Should().BeTrue();
        updated.PopCount().Should().Be(1);
        // original not mutated
        bb.Test(tileIndex).Should().BeFalse();
    }

    [Fact]
    public void GivenSetBit_WhenClearingBitAcrossEachInlineSegment_ThenBitRemoved()
    {
        // arrange

        // act

        // assert

        var bb = SegmentedBitboard.FromSegments(stackalloc ulong[] { 1UL, 1UL, 1UL, 1UL });
        bb.PopCount().Should().Be(4);

        // act & assert clearing start bit of each segment
        var c0 = bb.WithCleared(0);
        c0.PopCount().Should().Be(3);
        var c1 = c0.WithCleared(64);
        c1.PopCount().Should().Be(2);
        var c2 = c1.WithCleared(128);
        c2.PopCount().Should().Be(1);
        var c3 = c2.WithCleared(192);
        c3.PopCount().Should().Be(0);
    }

    [Fact]
    public void GivenOutOfRangeIndex_WhenSettingOrClearing_ThenNoChange()
    {
        // arrange

        // act

        // assert

        var bb = SegmentedBitboard.FromSegments(stackalloc ulong[] { 0UL });
        var set = bb.WithSet(9999); // segment index far beyond count => no-op
        var cleared = bb.WithCleared(9999);
        set.Low64.Should().Be(bb.Low64);
        cleared.Low64.Should().Be(bb.Low64);
        set.PopCount().Should().Be(0);
        cleared.PopCount().Should().Be(0);
    }

    [Fact]
    public void GivenSpillSegment_WhenSettingAndClearing_ThenSpillArrayClonedAndUpdated()
    {
        // arrange

        // act

        // assert

        var bb = SegmentedBitboard.FromSegments(stackalloc ulong[] { 0UL, 0UL, 0UL, 0UL, 0UL });
        bb.SegmentCount.Should().Be(5);
        bb.Any.Should().BeFalse();

        // act: set a bit in spill segment 4 (tile index 4*64 + 7)
        int tile = (4 * 64) + 7; // segment 4, offset 7
        var withSpillBit = bb.WithSet(tile);

        // assert
        withSpillBit.Test(tile).Should().BeTrue();
        withSpillBit.PopCount().Should().Be(1);
        withSpillBit.Any.Should().BeTrue();
        bb.Any.Should().BeFalse(); // immutability

        // act: clear the bit
        var cleared = withSpillBit.WithCleared(tile);
        cleared.PopCount().Should().Be(0);
        cleared.Test(tile).Should().BeFalse();
    }

    [Fact]
    public void GivenSpillOnlyHasBit_WhenCheckingAny_ThenSpillIterationDetected()
    {
        // arrange

        // act

        // assert

        var segments = new ulong[] { 0UL, 0UL, 0UL, 0UL, 0UL, 0UL };
        var bb = SegmentedBitboard.FromSegments(segments);
        var withBit = bb.WithSet((5 * 64) + 13); // 6th segment (index 5)

        withBit.Any.Should().BeTrue();
        withBit.None.Should().BeFalse();
        bb.Any.Should().BeFalse();
    }

    [Fact]
    public void GivenMultipleSegmentsWithBits_WhenPopCount_ThenTotalAccurate()
    {
        // arrange

        // act

        // assert

        var bb = SegmentedBitboard.FromSegments(stackalloc ulong[] { 0b1011UL, 0b1000_0001UL, 0UL, ulong.MaxValue, 0b1UL });

        // act
        var count = bb.PopCount();

        // assert
        count.Should().Be(70);
    }
}
