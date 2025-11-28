using Veggerby.Boards.Random;
using Veggerby.Boards.Utilities;

namespace Veggerby.Boards.Tests.Utils;

public class DiceUtilitiesTests
{
    [Fact]
    public void RollD6_Should_Return_Values_Between_1_And_6()
    {
        // arrange
        var random = XorShiftRandomSource.Create(12345);

        // act
        var results = new int[100];
        for (int i = 0; i < 100; i++)
        {
            results[i] = DiceUtilities.RollD6(random);
        }

        // assert
        foreach (var result in results)
        {
            result.Should().BeGreaterThanOrEqualTo(1);
            result.Should().BeLessThanOrEqualTo(6);
        }
    }

    [Fact]
    public void RollD6_Should_Be_Deterministic()
    {
        // arrange
        var random1 = XorShiftRandomSource.Create(42);
        var random2 = XorShiftRandomSource.Create(42);

        // act
        var results1 = new int[10];
        var results2 = new int[10];
        for (int i = 0; i < 10; i++)
        {
            results1[i] = DiceUtilities.RollD6(random1);
            results2[i] = DiceUtilities.RollD6(random2);
        }

        // assert
        results1.Should().Equal(results2);
    }

    [Fact]
    public void RollMultipleD6_Should_Return_Correct_Count()
    {
        // arrange
        var random = XorShiftRandomSource.Create(12345);

        // act
        var result = DiceUtilities.RollMultipleD6(3, random);

        // assert
        result.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(20)]
    public void RollDN_Should_Return_Values_In_Range(int sides)
    {
        // arrange
        var random = XorShiftRandomSource.Create(12345);

        // act
        var results = new int[50];
        for (int i = 0; i < 50; i++)
        {
            results[i] = DiceUtilities.RollDN(sides, random);
        }

        // assert
        foreach (var result in results)
        {
            result.Should().BeGreaterThanOrEqualTo(1);
            result.Should().BeLessThanOrEqualTo(sides);
        }
    }

    [Fact]
    public void SortDescending_Should_Sort_In_Descending_Order()
    {
        // arrange
        var values = new int[] { 2, 5, 1, 6, 3 };

        // act
        DiceUtilities.SortDescending(values);

        // assert
        values.Should().Equal(6, 5, 3, 2, 1);
    }

    [Fact]
    public void SortDescending_Should_Handle_Empty_Array()
    {
        // arrange
        var values = Array.Empty<int>();

        // act
        DiceUtilities.SortDescending(values);

        // assert
        values.Should().BeEmpty();
    }

    [Fact]
    public void SortDescending_Should_Handle_Single_Element()
    {
        // arrange
        var values = new int[] { 5 };

        // act
        DiceUtilities.SortDescending(values);

        // assert
        values.Should().Equal(5);
    }

    [Fact]
    public void CompareDicePairs_Attacker_Wins_When_Higher()
    {
        // arrange
        var attackerDice = new int[] { 6, 4 };
        var defenderDice = new int[] { 3, 2 };

        // act
        var (attackerWins, defenderWins) = DiceUtilities.CompareDicePairs(attackerDice, defenderDice);

        // assert
        attackerWins.Should().Be(2);
        defenderWins.Should().Be(0);
    }

    [Fact]
    public void CompareDicePairs_Defender_Wins_Ties_By_Default()
    {
        // arrange
        var attackerDice = new int[] { 5, 3 };
        var defenderDice = new int[] { 5, 3 };

        // act
        var (attackerWins, defenderWins) = DiceUtilities.CompareDicePairs(attackerDice, defenderDice);

        // assert
        attackerWins.Should().Be(0);
        defenderWins.Should().Be(2);
    }

    [Fact]
    public void CompareDicePairs_Attacker_Wins_Ties_When_Specified()
    {
        // arrange
        var attackerDice = new int[] { 5, 3 };
        var defenderDice = new int[] { 5, 3 };

        // act
        var (attackerWins, defenderWins) = DiceUtilities.CompareDicePairs(attackerDice, defenderDice, defenderWinsTies: false);

        // assert
        attackerWins.Should().Be(2);
        defenderWins.Should().Be(0);
    }

    [Fact]
    public void CompareDicePairs_Mixed_Results()
    {
        // arrange
        var attackerDice = new int[] { 6, 2 };
        var defenderDice = new int[] { 4, 5 };

        // act
        var (attackerWins, defenderWins) = DiceUtilities.CompareDicePairs(attackerDice, defenderDice);

        // assert
        attackerWins.Should().Be(1);
        defenderWins.Should().Be(1);
    }

    [Fact]
    public void CompareDicePairs_Different_Array_Lengths()
    {
        // arrange
        var attackerDice = new int[] { 6, 5, 4 };
        var defenderDice = new int[] { 3, 2 };

        // act
        var (attackerWins, defenderWins) = DiceUtilities.CompareDicePairs(attackerDice, defenderDice);

        // assert - only 2 comparisons (min of array lengths)
        attackerWins.Should().Be(2);
        defenderWins.Should().Be(0);
    }

    [Fact]
    public void Sum_Should_Return_Sum_Of_All_Values()
    {
        // arrange
        var values = new int[] { 3, 4, 5 };

        // act
        var result = DiceUtilities.Sum(values);

        // assert
        result.Should().Be(12);
    }

    [Fact]
    public void Sum_Should_Return_Zero_For_Empty_Array()
    {
        // arrange
        var values = Array.Empty<int>();

        // act
        var result = DiceUtilities.Sum(values);

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void AreAllEqual_Should_Return_True_For_Doubles()
    {
        // arrange
        var values = new int[] { 4, 4 };

        // act
        var result = DiceUtilities.AreAllEqual(values);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AreAllEqual_Should_Return_False_For_Different_Values()
    {
        // arrange
        var values = new int[] { 4, 5 };

        // act
        var result = DiceUtilities.AreAllEqual(values);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AreAllEqual_Should_Return_True_For_Single_Value()
    {
        // arrange
        var values = new int[] { 6 };

        // act
        var result = DiceUtilities.AreAllEqual(values);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AreAllEqual_Should_Return_True_For_Empty_Array()
    {
        // arrange
        var values = Array.Empty<int>();

        // act
        var result = DiceUtilities.AreAllEqual(values);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Max_Should_Return_Highest_Value()
    {
        // arrange
        var values = new int[] { 2, 6, 3, 1 };

        // act
        var result = DiceUtilities.Max(values);

        // assert
        result.Should().Be(6);
    }

    [Fact]
    public void Min_Should_Return_Lowest_Value()
    {
        // arrange
        var values = new int[] { 2, 6, 3, 1 };

        // act
        var result = DiceUtilities.Min(values);

        // assert
        result.Should().Be(1);
    }
}
