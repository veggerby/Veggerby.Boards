using System;

using Veggerby.Boards.Random;
using Veggerby.Boards.Risk;

namespace Veggerby.Boards.Tests.Risk;

/// <summary>
/// Tests for CombatResolver multi-dice combat algorithm.
/// </summary>
public class CombatResolverTests
{
    [Fact]
    public void Resolve_AttackerWinsAll_3v2_MaxRolls()
    {
        // arrange
        var attackerRolls = new[] { 6, 6, 5 };
        var defenderRolls = new[] { 3, 2 };

        // act
        var result = CombatResolver.ResolveWithRolls(attackerRolls, defenderRolls, 2);

        // assert
        result.AttackerLosses.Should().Be(0);
        result.DefenderLosses.Should().Be(2);
        result.DefenderEliminated.Should().BeTrue();
    }

    [Fact]
    public void Resolve_DefenderWinsAll_Ties_3v2()
    {
        // arrange
        var attackerRolls = new[] { 4, 3, 2 };
        var defenderRolls = new[] { 4, 3 }; // Ties go to defender

        // act
        var result = CombatResolver.ResolveWithRolls(attackerRolls, defenderRolls, 2);

        // assert
        result.AttackerLosses.Should().Be(2);
        result.DefenderLosses.Should().Be(0);
        result.DefenderEliminated.Should().BeFalse();
    }

    [Fact]
    public void Resolve_MixedResult_3v2()
    {
        // arrange
        var attackerRolls = new[] { 6, 3, 1 };
        var defenderRolls = new[] { 5, 4 };

        // act
        var result = CombatResolver.ResolveWithRolls(attackerRolls, defenderRolls, 2);

        // assert
        // Highest: 6 > 5 = defender loses 1
        // Second: 4 > 3 = attacker loses 1
        result.AttackerLosses.Should().Be(1);
        result.DefenderLosses.Should().Be(1);
        result.DefenderEliminated.Should().BeFalse();
    }

    [Fact]
    public void Resolve_2v1_TwoComparedToOne()
    {
        // arrange
        var attackerRolls = new[] { 5, 4 };
        var defenderRolls = new[] { 3 };

        // act
        var result = CombatResolver.ResolveWithRolls(attackerRolls, defenderRolls, 1);

        // assert
        // Only one pair compared: 5 > 3
        result.AttackerLosses.Should().Be(0);
        result.DefenderLosses.Should().Be(1);
        result.DefenderEliminated.Should().BeTrue();
    }

    [Fact]
    public void Resolve_1v1_SinglePairComparison()
    {
        // arrange
        var attackerRolls = new[] { 4 };
        var defenderRolls = new[] { 4 }; // Tie goes to defender

        // act
        var result = CombatResolver.ResolveWithRolls(attackerRolls, defenderRolls, 1);

        // assert
        result.AttackerLosses.Should().Be(1);
        result.DefenderLosses.Should().Be(0);
        result.DefenderEliminated.Should().BeFalse();
    }

    [Fact]
    public void Resolve_DiceSortedDescending()
    {
        // arrange
        var attackerRolls = new[] { 1, 6, 3 }; // Should sort to [6, 3, 1]
        var defenderRolls = new[] { 2, 5 };    // Should sort to [5, 2]

        // act
        var result = CombatResolver.ResolveWithRolls(attackerRolls, defenderRolls, 2);

        // assert
        // After sorting: Attacker [6, 3, 1] vs Defender [5, 2]
        // 6 > 5: defender loses 1
        // 3 > 2: defender loses 1
        result.AttackerLosses.Should().Be(0);
        result.DefenderLosses.Should().Be(2);
        result.AttackerRolls.Should().BeEquivalentTo(new[] { 6, 3, 1 });
        result.DefenderRolls.Should().BeEquivalentTo(new[] { 5, 2 });
    }

    [Fact]
    public void Resolve_DefenderEliminatedWithMoreArmies()
    {
        // arrange
        var attackerRolls = new[] { 6, 5 };
        var defenderRolls = new[] { 3, 2 };

        // Defender has 3 armies, will lose 2
        // act
        var result = CombatResolver.ResolveWithRolls(attackerRolls, defenderRolls, 3);

        // assert
        result.DefenderLosses.Should().Be(2);
        result.DefenderEliminated.Should().BeFalse(); // 3 - 2 = 1 remaining
    }

    [Fact]
    public void Resolve_WithRandomSource_ProducesDeterministicResults()
    {
        // arrange
        var random1 = XorShiftRandomSource.Create(12345);
        var random2 = XorShiftRandomSource.Create(12345); // Same seed

        // act
        var result1 = CombatResolver.Resolve(3, 2, random1);
        var result2 = CombatResolver.Resolve(3, 2, random2);

        // assert - same seed should produce identical results
        result1.AttackerLosses.Should().Be(result2.AttackerLosses);
        result1.DefenderLosses.Should().Be(result2.DefenderLosses);
        result1.AttackerRolls.Should().BeEquivalentTo(result2.AttackerRolls);
        result1.DefenderRolls.Should().BeEquivalentTo(result2.DefenderRolls);
    }

    [Fact]
    public void Resolve_WithRandomSource_DiceValuesInValidRange()
    {
        // arrange
        var random = XorShiftRandomSource.Create(54321);

        // act - run multiple times to verify range
        for (int i = 0; i < 100; i++)
        {
            var result = CombatResolver.Resolve(3, 2, random);

            // assert
            foreach (var roll in result.AttackerRolls)
            {
                roll.Should().BeGreaterThanOrEqualTo(1);
                roll.Should().BeLessThanOrEqualTo(6);
            }

            foreach (var roll in result.DefenderRolls)
            {
                roll.Should().BeGreaterThanOrEqualTo(1);
                roll.Should().BeLessThanOrEqualTo(6);
            }
        }
    }

    [Fact]
    public void Resolve_DefenderGets2Dice_WhenHas2OrMoreArmies()
    {
        // arrange
        var random = XorShiftRandomSource.Create(99999);

        // act
        var result = CombatResolver.Resolve(3, 2, random);

        // assert
        result.DefenderRolls.Length.Should().Be(2);
    }

    [Fact]
    public void Resolve_DefenderGets1Die_WhenHas1Army()
    {
        // arrange
        var random = XorShiftRandomSource.Create(88888);

        // act
        var result = CombatResolver.Resolve(3, 1, random);

        // assert
        result.DefenderRolls.Length.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(-1)]
    public void ResolveWithRolls_InvalidAttackerDiceCount_Throws(int diceCount)
    {
        // arrange
        var attackerRolls = diceCount <= 0 ? Array.Empty<int>() : new int[diceCount];
        var defenderRolls = new[] { 3 };

        // act & assert
        var act = () => CombatResolver.ResolveWithRolls(attackerRolls, defenderRolls, 1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    public void ResolveWithRolls_InvalidDefenderDiceCount_Throws(int diceCount)
    {
        // arrange
        var attackerRolls = new[] { 5 };
        var defenderRolls = diceCount <= 0 ? Array.Empty<int>() : new int[diceCount];

        // act & assert
        var act = () => CombatResolver.ResolveWithRolls(attackerRolls, defenderRolls, 1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
